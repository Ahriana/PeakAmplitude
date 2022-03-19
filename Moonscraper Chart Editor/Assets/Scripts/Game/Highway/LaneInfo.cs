// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoonscraperChartEditor.Song;

public class LaneInfo : MonoBehaviour {

    public Color[] laneColourPalette;

    public int[] guitarFretColourMap;
    public int[] drumPadColourMap;
    public int[] ghlGuitarFretColourMap;
    public int[] drumPadColourMap4LaneOverride;

    Dictionary<Chart.GameMode, int[]> standardGamemodePaletteMap;
    Dictionary<Chart.GameMode, Dictionary<int, int[]>> laneCountPaletteMapOverrides;

    int m_laneCount = 3;
    Dictionary<Chart.GameMode, int> standardGamemodeToLaneCountMap = new Dictionary<Chart.GameMode, int>()
    {
        { Chart.GameMode.Amplitude, 3 },
    };

    public const float positionRangeMin = -1, positionRangeMax = 1;

    // Use this for initialization
    void Start()
    {
        standardGamemodePaletteMap = new Dictionary<Chart.GameMode, int[]>()
        {
            { Chart.GameMode.Amplitude, guitarFretColourMap }
        };

        laneCountPaletteMapOverrides = new Dictionary<Chart.GameMode, Dictionary<int, int[]>>()
        {

        };

        ChartEditor.Instance.events.leftyFlipToggledEvent.Register(OnLanesUpdated);
        ChartEditor.Instance.events.chartReloadedEvent.Register(OnLanesUpdated);
    }

    public int laneCount
    {
        get
        {
            return m_laneCount;
        }
        set
        {
            m_laneCount = value;
            ChartEditor.Instance.events.lanesChangedEvent.Fire(laneCount);
        }
    }

    public int laneMask
    {
        get
        {
            return (1 << laneCount) - 1;
        }
    }

    public Color[] laneColours
    {
        get
        {
            Color[] colours = new Color[laneCount];
            int[] paletteMap;
            Chart.GameMode gameMode = ChartEditor.Instance.currentGameMode;

            if (!standardGamemodePaletteMap.TryGetValue(gameMode, out paletteMap))
                throw new System.Exception("Unable to find standard palette for current game mode");

            {
                Dictionary<int, int[]> overrideDict;
                int[] overridePaletteMap;
                if (laneCountPaletteMapOverrides.TryGetValue(gameMode, out overrideDict) && overrideDict.TryGetValue(laneCount, out overridePaletteMap))
                {
                    paletteMap = overridePaletteMap;
                }
            }

            for (int i = 0; i < colours.Length; ++i)
            {
                colours[i] = laneColourPalette[paletteMap[i]];
            }

            return colours;
        }
    }

    void OnLanesUpdated()
    {
        int newLaneCount = -1;
        if (!standardGamemodeToLaneCountMap.TryGetValue(ChartEditor.Instance.currentGameMode, out newLaneCount))
        {
            newLaneCount = -1;
        }

        if (laneCount >= 0)
            laneCount = newLaneCount;
    }

    public float GetLanePosition(int laneNum, bool flipLefty)
    {
        const float startOffset = positionRangeMin;
        const float endOffset = positionRangeMax;

        float incrementFactor = (endOffset - startOffset) / (laneCount - 1.0f);
        float position = Mathf.Min(startOffset + laneNum * incrementFactor, endOffset);

        if (flipLefty)
            position *= -1;

        return position;
    }
}
