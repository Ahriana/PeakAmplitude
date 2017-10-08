﻿// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshNoteResources : ScriptableObject {
    [Header("Note models")]
    public MeshFilter standardModel;
    public MeshFilter spModel;
    public MeshFilter openModel;

    [Header("Note renderers")]
    public Renderer strumRenderer;
    public Renderer hopoRenderer;
    public Renderer tapRenderer;
    public Renderer openRenderer;
    public Renderer spStrumRenderer;
    public Renderer spHopoRenderer;
    public Renderer spTapRenderer;

    [Header("Note colours")]
    public Material[] strumColors = new Material[6];
    public Material[] tapColors = new Material[5];

    public Material spTemp;
    public Material spTapTemp;

    public Material[] openMaterials = new Material[4];

    [Header("GHLive Note colours")]
    public Material[] ghlStrumColors = new Material[2];
    public Material[] ghlTapColors = new Material[2];
}
