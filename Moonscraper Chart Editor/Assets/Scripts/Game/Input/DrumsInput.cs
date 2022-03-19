// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections.Generic;
using MoonscraperEngine;
using MoonscraperChartEditor.Song;

public static class DrumsInput
{
    static readonly Dictionary<int, Dictionary<Note.DrumPad, MSChartEditorInputActions?>> laneCountGamepadOverridesDict = new Dictionary<int, Dictionary<Note.DrumPad, MSChartEditorInputActions?>>()
    {
        {
            4, new Dictionary<Note.DrumPad, MSChartEditorInputActions?>()
            {
                { Note.DrumPad.Green, null }
            }
        }
    };

    static readonly Dictionary<Note.DrumPad, MSChartEditorInputActions> drumPadToInputDict = new Dictionary<Note.DrumPad, MSChartEditorInputActions>()
    {
    };

    static readonly Dictionary<Note.DrumPad, MSChartEditorInputActions> proDrumsPadToTomInputDict = new Dictionary<Note.DrumPad, MSChartEditorInputActions>()
    {

    };

    static readonly Dictionary<Note.DrumPad, MSChartEditorInputActions> proDrumsPadToCymbalInputDict = new Dictionary<Note.DrumPad, MSChartEditorInputActions>()
    {

    };

    static bool GetPadPressedInput(Note.DrumPad drumFret, LaneInfo laneInfo, Dictionary<Note.DrumPad, MSChartEditorInputActions> inputsToCheck, Dictionary<int, Dictionary<Note.DrumPad, MSChartEditorInputActions?>> laneCountOverridesDict)
    {
        if (laneCountOverridesDict != null)
        {
            Dictionary<Note.DrumPad, MSChartEditorInputActions?> inputOverrideDict;
            MSChartEditorInputActions? overrideInput;

            if (laneCountOverridesDict.TryGetValue(laneInfo.laneCount, out inputOverrideDict) && inputOverrideDict.TryGetValue(drumFret, out overrideInput))
            {
                bool inputFound = false;

                if (overrideInput != null)
                    inputFound = MSChartEditorInput.GetInputDown((MSChartEditorInputActions)overrideInput);

                return inputFound;
            }
        }

        MSChartEditorInputActions input;
        if (inputsToCheck.TryGetValue(drumFret, out input))
        {
            return MSChartEditorInput.GetInputDown(input);
        }

        return false;
    }

    public static bool GetPadPressedInput(Note.DrumPad drumFret, LaneInfo laneInfo)
    {
        return GetPadPressedInput(drumFret, laneInfo, drumPadToInputDict, laneCountGamepadOverridesDict);
    }

    public static bool GetTomPressedInput(Note.DrumPad drumFret, LaneInfo laneInfo)
    {
        return GetPadPressedInput(drumFret, laneInfo, proDrumsPadToTomInputDict, null);
    }

    public static bool GetCymbalPressedInput(Note.DrumPad drumFret, LaneInfo laneInfo)
    {
        return GetPadPressedInput(drumFret, laneInfo, proDrumsPadToCymbalInputDict, null);
    }

    public static int GetPadPressedInputMask(LaneInfo laneInfo)
    {
        int inputMask = 0;

        foreach (Note.DrumPad pad in EnumX<Note.DrumPad>.Values)
        {
            if (GetPadPressedInput(pad, laneInfo))
                inputMask |= 1 << (int)pad;
        }

        return inputMask;
    }

    public static int GetProDrumsTomPressedInputMask(LaneInfo laneInfo)
    {
        int inputMask = 0;

        foreach (Note.DrumPad pad in EnumX<Note.DrumPad>.Values)
        {
            if (GetTomPressedInput(pad, laneInfo))
                inputMask |= 1 << (int)pad;
        }

        return inputMask;
    }

    public static int GetProDrumsCymbalPressedInputMask(LaneInfo laneInfo)
    {
        int inputMask = 0;

        foreach (Note.DrumPad pad in EnumX<Note.DrumPad>.Values)
        {
            if (GetCymbalPressedInput(pad, laneInfo))
                inputMask |= 1 << (int)pad;
        }

        return inputMask;
    }

    public static int GetTomMask(Note note)
    {
        return note.GetMaskWithRequiredFlags(Note.Flags.None);
    }

    public static int GetCymbalMask(Note note)
    {
        return note.GetMaskWithRequiredFlags(Note.Flags.ProDrums_Cymbal);
    }
}
