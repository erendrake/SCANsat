﻿#region license
/* 
 *  [Scientific Committee on Advanced Navigation]
 * 			S.C.A.N. Satellite
 *
 * SCANsat - Skins and Styles setup
 * 
 * Copyright (c)2013 damny;
 * Copyright (c)2014 David Grandy <david.grandy@gmail.com>;
 * Copyright (c)2014 technogeeky <technogeeky@gmail.com>;
 * Copyright (c)2014 (Your Name Here) <your email here>; see LICENSE.txt for licensing details.
 *
 */
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using SCANsat.Platform;
using palette = SCANsat.SCANpalette;
using UnityEngine;


namespace SCANsat.SCAN_UI
{
	[KSPAddon(KSPAddon.Startup.MainMenu, true)]
	class SCANskins: MBE
	{
		internal static GUISkin SCAN_skin;

		internal static GUIStyle SCAN_window;
		internal static GUIStyle SCAN_toggle;
		internal static GUIStyle SCAN_tooltip;
		internal static GUIStyle SCAN_label;

		//Button styles
		internal static GUIStyle SCAN_button;
		internal static GUIStyle SCAN_buttonActive;
		internal static GUIStyle SCAN_buttonFixed;
		internal static GUIStyle SCAN_texButton;
		internal static GUIStyle SCAN_buttonBorderless;
		internal static GUIStyle SCAN_closeButton;

		//Map info readout styles
		internal static GUIStyle SCAN_readoutLabel;
		internal static GUIStyle SCAN_whiteReadoutLabel;
		internal static GUIStyle SCAN_activeReadoutLabel;
		internal static GUIStyle SCAN_inactiveReadoutLabel;
		internal static GUIStyle SCAN_shadowReadoutLabel;

		//Instrument readout styles
		internal static GUIStyle SCAN_insColorLabel;
		internal static GUIStyle SCAN_insWhiteLabel;
		internal static GUIStyle SCAN_anomalyOverlay;

		//Settings menu styles
		internal static GUIStyle SCAN_headline;

		//Styles for map overlay icons
		internal static GUIStyle SCAN_orbitalLabelOn;
		internal static GUIStyle SCAN_orbitalLabelOff;

		//Drop down menu styles
		internal static GUIStyle SCAN_dropDownButton;
		internal static GUIStyle SCAN_dropDownBox;

		internal static Font dotty;

		protected override void OnGUI_FirstRun()
		{
			SCAN_skin = SCAN_SkinsLibrary.CopySkin("Unity");
			SCAN_SkinsLibrary.AddSkin("SCAN_Unity", SCAN_skin);

			ScreenMessages SM = (ScreenMessages)GameObject.FindObjectOfType(typeof(ScreenMessages));
			dotty = SM.textStyles[1].font;

			SCAN_window = new GUIStyle(SCAN_SkinsLibrary.DefUnitySkin.window);
			SCAN_window.name = "SCAN_Window";

			SCAN_label = new GUIStyle(SCAN_SkinsLibrary.DefUnitySkin.label);
			SCAN_label.name = "SCAN_Label";
			SCAN_label.normal.textColor = palette.xkcd_PukeGreen;

			//Initialize button styles
			SCAN_button = new GUIStyle(SCAN_SkinsLibrary.DefUnitySkin.button);
			SCAN_button.name = "SCAN_Button";
			SCAN_button.alignment = TextAnchor.MiddleCenter;
			SCAN_button.normal.textColor = palette.white;
			SCAN_button.active.textColor = palette.xkcd_PukeGreen;

			SCAN_buttonActive = new GUIStyle(SCAN_button);
			SCAN_buttonActive.name = "SCAN_ButtonActive";
			SCAN_buttonActive.normal.textColor = palette.xkcd_PukeGreen;

			SCAN_buttonFixed = new GUIStyle(SCAN_button);
			SCAN_buttonFixed.name = "SCAN_ButtonFixed";
			SCAN_buttonFixed.active.textColor = SCAN_buttonFixed.normal.textColor;

			SCAN_texButton = new GUIStyle(SCAN_button);
			SCAN_texButton.name = "SCAN_TexButton";
			SCAN_texButton.padding = new RectOffset(0, 0, 1, 1);
			SCAN_texButton.normal.background = SCAN_SkinsLibrary.DefUnitySkin.label.normal.background;

			SCAN_buttonBorderless = new GUIStyle(SCAN_button);
			SCAN_buttonBorderless.name = "SCAN_ButtonBorderless";
			SCAN_buttonBorderless.fontSize = 14;
			SCAN_buttonBorderless.margin = new RectOffset(2, 2, 2, 2);
			SCAN_buttonBorderless.padding = new RectOffset(0, 2, 2, 2);
			SCAN_buttonBorderless.normal.background = SCAN_SkinsLibrary.DefUnitySkin.label.normal.background;

			SCAN_closeButton = new GUIStyle(SCAN_buttonBorderless);
			SCAN_closeButton.name = "SCAN_CloseButton";
			SCAN_closeButton.normal.textColor = palette.cb_vermillion;

			//Initialize drop down menu styles
			SCAN_dropDownBox = new GUIStyle(SCAN_SkinsLibrary.DefUnitySkin.box);
			SCAN_dropDownBox.name = "SCAN_DropDownBox";

			SCAN_dropDownButton = new GUIStyle(SCAN_label);
			SCAN_dropDownButton.name = "SCAN_DropDownButton";
			SCAN_dropDownButton.padding = new RectOffset(2, 2, 2, 2);
			SCAN_dropDownButton.normal.textColor = palette.xkcd_PukeGreen;
			SCAN_dropDownButton.hover.textColor = palette.xkcd_PukeGreen;
			Texture2D sortBackground = new Texture2D(1, 1);
			sortBackground.SetPixel(1, 1, palette.xkcd_White);
			sortBackground.Apply();
			SCAN_dropDownButton.hover.background = sortBackground;
			SCAN_dropDownButton.alignment = TextAnchor.MiddleLeft;

			//Initialize info readout styles
			SCAN_readoutLabel = new GUIStyle(SCAN_label);
			SCAN_readoutLabel.name = "SCAN_ReadoutLabel";
			SCAN_readoutLabel.fontStyle = FontStyle.Bold;

			SCAN_whiteReadoutLabel = new GUIStyle(SCAN_readoutLabel);
			SCAN_whiteReadoutLabel.name = "SCAN_WhiteLabel";
			SCAN_whiteReadoutLabel.normal.textColor = palette.white;

			SCAN_activeReadoutLabel = new GUIStyle(SCAN_readoutLabel);
			SCAN_activeReadoutLabel.name = "SCAN_ActiveLabel";
			SCAN_activeReadoutLabel.normal.textColor = palette.cb_bluishGreen;

			SCAN_inactiveReadoutLabel = new GUIStyle(SCAN_readoutLabel);
			SCAN_inactiveReadoutLabel.name = "SCAN_InactiveLabel";
			SCAN_inactiveReadoutLabel.normal.textColor = palette.xkcd_LightGrey;

			SCAN_shadowReadoutLabel = new GUIStyle(SCAN_readoutLabel);
			SCAN_shadowReadoutLabel.name = "SCAN_ShadowLabel";
			SCAN_shadowReadoutLabel.normal.textColor = palette.black;

			//Initialize instrument styles
			SCAN_insColorLabel = new GUIStyle(SCAN_label);
			SCAN_insColorLabel.name = "SCAN_InsColorLabel";
			SCAN_insColorLabel.alignment = TextAnchor.MiddleCenter;
			SCAN_insColorLabel.font = dotty;
			SCAN_insColorLabel.fontSize = 36;

			SCAN_insWhiteLabel = new GUIStyle(SCAN_whiteReadoutLabel);
			SCAN_insWhiteLabel.name = "SCAN_InsWhiteLabel";
			SCAN_insWhiteLabel.alignment = TextAnchor.MiddleCenter;
			SCAN_insWhiteLabel.fontStyle = FontStyle.Normal;
			SCAN_insWhiteLabel.font = dotty;
			SCAN_insWhiteLabel.fontSize = 36;

			SCAN_anomalyOverlay = new GUIStyle(SCAN_SkinsLibrary.DefUnitySkin.label);
			SCAN_anomalyOverlay.name = "SCAN_AnomalyOverlay";
			SCAN_anomalyOverlay.font = dotty;
			SCAN_anomalyOverlay.fontSize = 32;
			SCAN_anomalyOverlay.fontStyle = FontStyle.Bold;
			SCAN_anomalyOverlay.normal.textColor = palette.cb_skyBlue;

			//Initialize settings menu styles
			SCAN_headline = new GUIStyle(SCAN_SkinsLibrary.DefUnitySkin.label);
			SCAN_headline.name = "SCAN_Headline";
			SCAN_headline.normal.textColor = palette.xkcd_YellowGreen;
			SCAN_headline.alignment = TextAnchor.MiddleCenter;
			SCAN_headline.fontSize = 40;
			SCAN_headline.font = dotty;

			SCAN_toggle = new GUIStyle(SCAN_SkinsLibrary.DefUnitySkin.toggle);
			SCAN_toggle.name = "SCAN_Toggle";

			SCAN_tooltip = new GUIStyle(SCAN_SkinsLibrary.DefUnitySkin.label);
			SCAN_tooltip.name = "SCAN_Tooltip";

			SCAN_orbitalLabelOn = new GUIStyle(SCAN_label);
			SCAN_orbitalLabelOn.name = "SCAN_OrbitalLabelOn";
			SCAN_orbitalLabelOn.fontSize = 13;
			SCAN_orbitalLabelOn.fontStyle = FontStyle.Bold;
			SCAN_orbitalLabelOn.normal.textColor = palette.cb_yellow;

			SCAN_orbitalLabelOff = new GUIStyle(SCAN_orbitalLabelOn);
			SCAN_orbitalLabelOff.name = "SCAN_OrbitalLabelOff";
			SCAN_orbitalLabelOff.normal.textColor = palette.white;

			//Add styles to skin
			SCAN_SkinsLibrary.knownSkins["SCAN_Unity"].window = new GUIStyle(SCAN_window);
			SCAN_SkinsLibrary.knownSkins["SCAN_Unity"].button = new GUIStyle(SCAN_button);
			SCAN_SkinsLibrary.knownSkins["SCAN_Unity"].toggle = new GUIStyle(SCAN_toggle);
			SCAN_SkinsLibrary.knownSkins["SCAN_Unity"].label = new GUIStyle(SCAN_label);

			SCAN_SkinsLibrary.AddStyle(SCAN_window, "SCAN_Unity");
			SCAN_SkinsLibrary.AddStyle(SCAN_button, "SCAN_Unity");
			SCAN_SkinsLibrary.AddStyle(SCAN_toggle, "SCAN_Unity");
			SCAN_SkinsLibrary.AddStyle(SCAN_label, "SCAN_Unity");
			SCAN_SkinsLibrary.AddStyle(SCAN_tooltip, "SCAN_Unity");
		}

	}
}