﻿// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class ProgramInformationPopulate : MonoBehaviour {
    Text programInfoText;

	// Use this for initialization
	void Start () {
        programInfoText = GetComponent<Text>();

        programInfoText.text = string.Format("{0} v{1} {2} \n\nBuilt on Moonscraper 1.4.3.4\nBy Alexander \"FireFox\" Ong.\n\nModified for use with Amplitude by \"Ahriana\"\n\nBuilt using Unity {3}", 
            Application.productName, 
            Application.version, 
            Globals.applicationBranchName, 
            Application.unityVersion);
    }
}
