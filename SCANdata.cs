/* 
 * Scientific Committee on Advanced Navigation S.C.A.N. Satellite
 * SCANdata - encapsulates scanned data for a body
 * 
 * Copyright (c)2013 damny; see LICENSE.txt for licensing details.
 */

using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class SCANdata
{
	protected byte[,] coverage = new byte[360, 180];
	protected float[,] heightmap = new float[360,180];
	protected int bigscale = Screen.width/360;
	protected int bigmode = 0;

	public CelestialBody body;
	public int updateSerial;
	public Texture2D height_map_small = new Texture2D(360, 180, TextureFormat.RGB24, false);
	public Texture2D height_map_big;

	public enum SCANtype {
		Nothing = 0,			// no data
		AltimetryLoRes = 1,		// low resolution altimetry (limited zoom)
		AltimetryHiRes = 2,		// high resolution altimetry (unlimited zoom)
		Altimetry = 3,			// both (setting) or either (testing) altimetry
		Slope = 4,				// slope data
		Biome = 8,				// biome data
		Anomaly = 16,			// anomalies (position of anomaly)
		AnomalyDetail = 32,		// anomaly detail (name of anomaly, etc.)
		Everything = 255		// everything
	}

	public void registerPass(float lon, float lat, SCANtype type) {
		// fudging coordinates a bit because KSP may return them unclipped
		int ilon = ((int)(lon + 360 + 180)) % 360;
		int ilat = ((int)(lat + 180 + 90)) % 180;
		if(ilon < 0 || ilat < 0 || ilon >= 360 || ilat >= 180) return;
		coverage[ilon, ilat] |= (byte)type;
		updateSerial += 1;
	}

	public bool isCovered(float lon, float lat, SCANtype type) {
		int ilon = ((int)(lon + 360 + 180)) % 360;
		int ilat = ((int)(lat + 180 + 90)) % 180;
		if(ilon < 0 || ilat < 0 || ilon >= 360 || ilat >= 180) return false;
		return (coverage[ilon, ilat] & (byte)type) != 0;
	}

	public class SCANanomaly {
		public SCANanomaly(string s, double lon, double lat) {
			name = s;
			longitude = (float)lon;
			latitude = (float)lat;
			known = false;
		}
		public bool known;
		public bool detail;
		public string name;
		public float longitude;
		public float latitude;
	}
	SCANanomaly[] anomalies;
	public SCANanomaly[] getAnomalies() {
		if(anomalies == null) {
			PQSCity[] sites = body.GetComponentsInChildren<PQSCity>(true);
			anomalies = new SCANanomaly[sites.Length];
			for(int i=0; i<sites.Length; ++i) {
				anomalies[i] = new SCANanomaly(sites[i].name, body.GetLongitude(sites[i].transform.position), body.GetLatitude(sites[i].transform.position));
			}
		}
		for(int i=0; i<anomalies.Length; ++i) {
			anomalies[i].known = isCovered(anomalies[i].longitude, anomalies[i].latitude, SCANtype.Anomaly);
			anomalies[i].detail = isCovered(anomalies[i].longitude, anomalies[i].latitude, SCANtype.AnomalyDetail);
		}
		return anomalies;
	}

	public string serialize() {
		// convert the byte[,] array into a KSP-savefile-safe variant of Base64
		MemoryStream mem = new MemoryStream();
		BinaryFormatter binf = new BinaryFormatter();
		binf.Serialize(mem, coverage);
		string blob = Convert.ToBase64String(CLZF2.Compress(mem.ToArray()));
		return blob.Replace("/","-").Replace("=","_");
	}

	public void deserialize(string blob) {
		try {
			blob = blob.Replace("-","/").Replace("_","=");
			byte[] bytes = Convert.FromBase64String(blob);
			bytes = CLZF2.Decompress(bytes);
			MemoryStream mem = new MemoryStream(bytes, false);
			BinaryFormatter binf = new BinaryFormatter();
			coverage = (byte[,])binf.Deserialize(mem);
		} catch(Exception e) {
			coverage = new byte[360, 180];
			heightmap = new float[360,180];
			throw e;
		}
		resetImages();
	}

	public void reset() {
		coverage = new byte[360, 180];
		heightmap = new float[360, 180];
		resetImages();
	}

	public void resetImages() {
		// Just draw a simple grid to initialize the image; the map will appear on top of it
		for(int y=0; y<height_map_small.height; y++) {
			for(int x=0; x<height_map_small.width; x++) {
				if((x % 30 == 0 && y % 3 > 0) || (y % 30 == 0 && x % 3 > 0)) {
					height_map_small.SetPixel(x, y, Color.white);
				} else {
					height_map_small.SetPixel(x, y, Color.grey);
				}
			}
		}
		height_map_small.Apply();
	}

	protected Color[] redline;
	public void updateImages() {
		if(redline == null) {
			redline = new Color[360];
			for(int i=0; i<360; i++) redline[i] = Color.red;
		}
		drawHeightScanline();
		if(scanline < 179) {
			height_map_small.SetPixels(0, scanline + 1, 360, 1, redline);
		}
		height_map_small.Apply();
	}

	protected Color[] heightGradient = {XKCDColors.ArmyGreen, XKCDColors.Yellow, XKCDColors.Red, XKCDColors.Magenta, XKCDColors.White, XKCDColors.White};
	public Color heightToColor(float val) {
		if(SCANcontroller.controller.colours == 1) {
			return Color.Lerp(Color.black, Color.white, Mathf.Clamp(val+1500f, 0, 9000)/9000f);
		}
		Color c = Color.black;
		if(val <= 0) {
			val = (Mathf.Clamp(val, -1500, 0) + 1500) / 1000f;
			c = Color.Lerp(XKCDColors.DarkPurple, XKCDColors.Cerulean, val);
		} else {
			val = (heightGradient.Length-2) * Mathf.Clamp(val, 0, 7500) / 7500.0f;
			c = Color.Lerp(heightGradient[(int)val], heightGradient[(int)val + 1], val - (int)val);
		}
		return c;
	}

	public float getElevation(float lon, float lat) {
		if(body.pqsController == null) return 0;
		int ilon = ((int)(lon + 360 + 180)) % 360;
		int ilat = ((int)(lat + 180 + 90)) % 180;
		float rlon = Mathf.Deg2Rad * lon;
		float rlat = Mathf.Deg2Rad * lat;
		Vector3d rad = new Vector3d(Math.Cos(rlat) * Math.Cos(rlon), Math.Sin(rlat), Math.Cos(rlat) * Math.Sin(rlon));
		return (float)Math.Round(body.pqsController.GetSurfaceHeight(rad) - body.pqsController.radius, 1);
	}

	public string getBiomeName(float lon, float lat) {
		CBAttributeMap.MapAttribute a = getBiome(lon, lat);
		if(a == null) return "unknown";
		return a.name;
	}

	public CBAttributeMap.MapAttribute getBiome(float lon, float lat) {
		// It could be so easy, if this function didn't print debug messages to the screen...
		// return body.BiomeMap.GetAtt(Mathf.Deg2Rad * lat, Mathf.Deg2Rad * lon).name;
		if(body.BiomeMap == null) return null;
		if(body.BiomeMap.Map == null) return body.BiomeMap.defaultAttribute;
		float u = ((lon + 360 + 180 + 90)) % 360;
		float v = ((lat + 180 + 90)) % 180;
		if(u < 0 || v < 0 || u >= 360 || v >= 180) return body.BiomeMap.defaultAttribute;
		u /= 360f; v /= 180f;
		Color c = body.BiomeMap.Map.GetPixelBilinear(u, v);
		CBAttributeMap.MapAttribute a = body.BiomeMap.defaultAttribute;
		float maxdiff = 12345;
		for(int i=0; i<body.BiomeMap.Attributes.Length; ++i) {
			CBAttributeMap.MapAttribute x = body.BiomeMap.Attributes[i];
			Color d = x.mapColor;
			float diff = ((Vector4)d - (Vector4)c).sqrMagnitude;
			if(diff < maxdiff) {
				a = x;
				maxdiff = diff;
			}
		}
		return a;
	}

	protected int scanline = 0;
	protected int scanstep = 0;
	public void drawHeightScanline() {
		Color[] cols_height_map_small = height_map_small.GetPixels(0, scanline, 360, 1);
		for(int ilon=0; ilon<360; ilon+=1) {
			float val = heightmap[ilon, scanline];
			if(val == 0 && isCovered(ilon-180, scanline-90, SCANtype.Altimetry)) {
				if(body.pqsController == null) {
					heightmap[ilon, scanline] = 0;
					cols_height_map_small[ilon] = Color.Lerp(Color.black, Color.white, UnityEngine.Random.value);
					continue;
				} else {
					// convert to radial vector
					float rlon = Mathf.Deg2Rad * (ilon - 180);
					float rlat = Mathf.Deg2Rad * (scanline - 90);
					Vector3d rad = new Vector3d(Math.Cos(rlat) * Math.Cos(rlon), Math.Sin(rlat), Math.Cos(rlat) * Math.Sin(rlon));
					// query terrain controller for elevation at this point
					val = (float)Math.Round(body.pqsController.GetSurfaceHeight(rad) - body.pqsController.radius, 1);
					if(val == 0) val = -0.001f; // this is terrible
					heightmap[ilon, scanline] = val;
				}
			}
			Color c = Color.black;
			if(val != 0) {
				c = heightToColor(val);
			} else {
				c = Color.grey;
				if(scanline % 30 == 0 && ilon % 3 == 0) {
					c = Color.white;
				} else if(ilon % 30 == 0 && scanline % 3 == 0) {
					c = Color.white;
				}
			}
			if(c != Color.black) {
				cols_height_map_small[ilon] = c;
			}
		}
		height_map_small.SetPixels(0, scanline, 360, 1, cols_height_map_small);
		scanline = scanline + 1;
		if(scanline >= 180) {
			scanstep += 1;
			scanline = 0;
		}
	}

	protected int bigstep;
	protected bool bigsaved;
	protected float[] bigline;
	public Texture2D getPartialBigMap() {
		Color[] pix;
		if(bigscale > 5) bigscale = 5;
		if(bigscale < 1) bigscale = 1;
		if(height_map_big == null) {
			height_map_big = new Texture2D(360 * bigscale, 180 * bigscale, TextureFormat.RGB24, false);
			pix = height_map_big.GetPixels();
			for(int i=0; i<pix.Length; ++i) pix[i] = Color.grey;
			height_map_big.SetPixels(pix);
		} else if(bigstep >= height_map_big.height) {
			if(!bigsaved) {
				// if we just finished rendering a map, save it to our PluginData folder
				string mode = "unknown";
				if(bigmode == 0) mode = "elevation";
				else if(bigmode == 1) mode = "slope";
				else if(bigmode == 2) mode = "biome";
				if(SCANcontroller.controller.colours == 1) mode += "-grey";
				string filename = body.name + "_" + mode + "_" + height_map_big.width.ToString() + "x" + height_map_big.height.ToString() + ".png";
				KSP.IO.File.WriteAllBytes<SCANdata>(height_map_big.EncodeToPNG(), filename, null);
				bigsaved = true;
			}
			return height_map_big;
		}
		if(bigstep <= 0) {
			bigstep = 0;
			bigline = new float[height_map_big.width];
		}
		pix = height_map_big.GetPixels(0, bigstep, height_map_big.width, 1);
		float lat = (bigstep * 1.0f / bigscale) - 90f;
		for(int i=0; i<height_map_big.width; i++) {
			float lon = (i * 1.0f / bigscale) - 180f;
			if(bigmode == 0) {
				if(!isCovered(lon, lat, SCANtype.Altimetry)) continue;
				if(body.pqsController == null) {
					pix[i] = Color.Lerp(Color.black, Color.white, UnityEngine.Random.value);
					continue;
				}
				float val = getElevation(lon, lat);
				pix[i] = heightToColor(val);
				/* draw height lines - works, but mostly useless...
				int step = (int)(val / 1000);
				int step_h = step, step_v = step;
				if(i > 0) step_h = (int)(bigline[i - 1] / 1000);
				if(bigstep > 0) step_v = (int)(bigline[i] / 1000);
				if(step != step_h || step != step_v) {
					pix[i] = Color.white;
				}
				*/
				bigline[i] = val;
			} else if(bigmode == 1) {
				if(!isCovered(lon, lat, SCANtype.Slope)) continue;
				if(body.pqsController == null) {
					pix[i] = Color.Lerp(Color.black, Color.white, UnityEngine.Random.value);
					continue;
				}
				float val = getElevation(lon, lat);
				if(bigstep == 0) {
					pix[i] = Color.grey;
				} else {
					// This doesn't actually calculate the slope per se, but it's faster
					// than asking for yet more elevation data. Please don't use this
					// code to operate nuclear power plants or rockets.
					float v1 = bigline[i];
					if(i > 0) v1 = Math.Max(v1, bigline[i - 1]);
					if(i < bigline.Length - 1) v1 = Math.Max(v1, bigline[i + 1]);
					float v = Mathf.Clamp(Math.Abs(val - v1) / 1000f, 0, 2f);
					if(SCANcontroller.controller.colours == 1) {
						pix[i] = Color.Lerp(Color.black, Color.white, v / 2f);
					} else {
						if(v < 1) {
							pix[i] = Color.Lerp(XKCDColors.PukeGreen, XKCDColors.Lemon, v);
						} else {
							pix[i] = Color.Lerp(XKCDColors.Lemon, XKCDColors.OrangeRed, v-1);
						}
					}
				}
				bigline[i] = val;
			} else if(bigmode == 2) {
				if(!isCovered(lon, lat, SCANtype.Biome)) continue;
				if(body.BiomeMap == null || body.BiomeMap.Map == null) {
					pix[i] = Color.Lerp(Color.black, Color.white, UnityEngine.Random.value);
					continue;
				}
				float u = ((lon + 360 + 180 + 90)) % 360;
				float v = ((lat + 180 + 90)) % 180;
				if(u < 0 || v < 0 || u >= 360 || v >= 180) continue;
				u /= 360f; v /= 180f;
				pix[i] = body.BiomeMap.Map.GetPixelBilinear(u, v);
			}
		}
		height_map_big.SetPixels(0, bigstep, height_map_big.width, 1, pix);
		height_map_big.Apply();
		bigstep++;
		return height_map_big;
	}

	public bool isBigMapComplete() {
		if(height_map_big == null) return false;
		return bigstep >= height_map_big.height;
	}

	public void resetBigMap() {
		bigstep = 0;
		bigsaved = false;
	}

	public void resetBigMap(int mode) {
		bigmode = mode;
		resetBigMap();
	}
}
