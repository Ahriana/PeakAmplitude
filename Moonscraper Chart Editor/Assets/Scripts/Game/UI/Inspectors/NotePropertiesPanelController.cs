﻿// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using UnityEngine.UI;
using MoonscraperChartEditor.Song;
using System.Text;

public class NotePropertiesPanelController : PropertiesPanelController {
    public Note currentNote { get { return (Note)currentSongObject; } set { currentSongObject = value; } }

    public Text sustainText;
    public Text fretText;
    
    // public Toggle tapToggle;
    // public Toggle forcedToggle;
    // public Toggle cymbalToggle;
    // public Toggle doubleKickToggle;

    public GameObject noteToolObject;
    PlaceNoteController noteToolController;

    Note prevNote = null;
    Note prevClonedNote = new Note(0, 0);

    bool toggleBlockingActive = false;
    bool initialised = false;

    private void Start()
    {
        if (!initialised)
        {
            noteToolController = noteToolObject.GetComponent<PlaceNoteController>();
            editor.events.toolChangedEvent.Register(OnToolChanged);
            initialised = true;
        }

        ChartEditor.Instance.events.drumsModeOptionChangedEvent.Register(UpdateTogglesInteractable);
    }

    void OnToolChanged()
    {
    }

    void OnEnable()
    {
        if (!initialised)
        {
            Start();
        }

        Update();
    }

    protected override void Update()
    {
        UpdateTogglesInteractable();
        UpdateTogglesDisplay();

        UpdateNoteStringsInfo();
        Controls();

        prevNote = currentNote;
    }

    uint lastKnownKeysModePos = uint.MaxValue;
    void UpdateNoteStringsInfo()
    {
        bool hasCurrentNote = currentNote != null;
        bool hasPreviousNote = prevClonedNote != null;
        bool valuesAreTheSame = hasCurrentNote && hasPreviousNote && prevClonedNote.AllValuesCompare(currentNote);

        if (IsInNoteTool() && Globals.gameSettings.keysModeEnabled)
        {
            // Don't update the string unless the position has actually changed. Results in per-frame garbage otherwise
            if (lastKnownKeysModePos != editor.currentTickPos)
            {
                positionText.text = "Position: " + editor.currentTickPos;
                lastKnownKeysModePos = editor.currentTickPos;
            }

            fretText.text = "Fret: N/A";
            sustainText.text = "Length: N/A";
        }
        else if (currentNote != null && (prevClonedNote != currentNote || !valuesAreTheSame))
        {
            if (currentNote.guitarFret == Note.GuitarFret.Green)
            {
                fretText.text = "Fret: Left";
            } else if (currentNote.guitarFret == Note.GuitarFret.Red)
            {
                fretText.text = "Fret: Middle";
            } else if (currentNote.guitarFret == Note.GuitarFret.Yellow)
            {
                fretText.text = "Fret: Right";
            }
            
            positionText.text = "Position: " + currentNote.tick.ToString();
            sustainText.text = "Length: " + currentNote.length.ToString();

            prevClonedNote.CopyFrom(currentNote);
            lastKnownKeysModePos = uint.MaxValue;
        }
    }

    bool IsInNoteTool()
    {
        return editor.toolManager.currentToolId == EditorObjectToolManager.ToolID.Note;
    }

    public Note.Flags GetDisplayFlags()
    {
        Note.Flags flags = Note.Flags.None;
        bool inNoteTool = IsInNoteTool();

        if (inNoteTool)
        {
            flags = noteToolController.GetDisplayFlags();
        }
        else if (currentNote != null)
        {
            flags = currentNote.flags;
        }

        return flags;
    }

    void UpdateTogglesDisplay()
    {
        toggleBlockingActive = true;

        Note.Flags flags = GetDisplayFlags();
        bool inNoteTool = IsInNoteTool();

        if (!inNoteTool && currentNote == null)
        {
            gameObject.SetActive(false);
            Debug.LogError("No note loaded into note inspector");
        }

        // forcedToggle.isOn = (flags & Note.Flags.Forced) != 0;
        // tapToggle.isOn = (flags & Note.Flags.Tap) != 0;
        // cymbalToggle.isOn = (flags & Note.Flags.ProDrums_Cymbal) != 0;
        // doubleKickToggle.isOn = (flags & Note.Flags.DoubleKick) != 0;

        toggleBlockingActive = false;
    }

    void UpdateTogglesInteractable()
    {
        // Prevent users from forcing notes when they shouldn't be forcable but retain the previous user-set forced property when using the note tool
        bool drumsMode = Globals.drumMode;
        bool proDrumsMode = drumsMode && Globals.gameSettings.drumsModeOptions == GameSettings.DrumModeOptions.ProDrums;

        // forcedToggle.gameObject.SetActive(!drumsMode);
        // tapToggle.gameObject.SetActive(!drumsMode);
        // cymbalToggle.gameObject.SetActive(proDrumsMode);
        // doubleKickToggle.gameObject.SetActive(proDrumsMode);

        // if (!drumsMode)
        // {
        //     if (IsInNoteTool() && (noteToolObject.activeSelf || Globals.gameSettings.keysModeEnabled))
        //     {
        //         forcedToggle.interactable = noteToolController.forcedInteractable;
        //         tapToggle.interactable = noteToolController.tapInteractable;
        //     }
        //     else if (!IsInNoteTool())
        //     {
        //         forcedToggle.interactable = !(currentNote.cannotBeForced && !Globals.gameSettings.keysModeEnabled);
        //         tapToggle.interactable = !currentNote.IsOpenNote();
        //     }
        //     else
        //     {
        //         forcedToggle.interactable = true;
        //         tapToggle.interactable = true;
        //     }
        // }
        // else
        // {
        //     if (IsInNoteTool() && noteToolObject.activeSelf)
        //     {
        //         cymbalToggle.interactable = noteToolController.cymbalInteractable;
        //         doubleKickToggle.interactable = noteToolController.doubleKickInteractable;
        //     }
        //     else if (!IsInNoteTool())
        //     {
        //         cymbalToggle.interactable = NoteFunctions.AllowedToBeCymbal(currentNote);
        //         doubleKickToggle.interactable = NoteFunctions.AllowedToBeDoubleKick(currentNote, editor.currentDifficulty);
        //     }
        //     else
        //     {
        //         cymbalToggle.interactable = true;
        //         doubleKickToggle.interactable = true;
        //     }
        // }
    }

    void Controls()
    {
        // if (MSChartEditorInput.GetInputDown(MSChartEditorInputActions.ToggleNoteTap) && tapToggle.interactable)
        // {
        //     tapToggle.isOn = !tapToggle.isOn;
        // }
        // 
        // if (MSChartEditorInput.GetInputDown(MSChartEditorInputActions.ToggleNoteForced) && forcedToggle.interactable)
        // {
        //     forcedToggle.isOn = !forcedToggle.isOn;
        // }
        // 
        // if (MSChartEditorInput.GetInputDown(MSChartEditorInputActions.ToggleNoteCymbal) && cymbalToggle.interactable)
        // {
        //     cymbalToggle.isOn = !cymbalToggle.isOn;
        // }
        // 
        // if (MSChartEditorInput.GetInputDown(MSChartEditorInputActions.ToggleNoteDoubleKick) && doubleKickToggle.interactable)
        // {
        //     doubleKickToggle.isOn = !doubleKickToggle.isOn;
        // }
    }

    new void OnDisable()
    {
        currentNote = null;
    }
	
    public void setTap()
    {
        if (toggleBlockingActive)
            return;

        if (IsInNoteTool())
        {
            SetTapNoteTool();
        }
        else
        {
            SetTapNote();
        }
    }

    void SetTapNoteTool()
    {
        // if (tapToggle.interactable)
        //     SetNoteToolFlag(ref noteToolController.desiredFlags, tapToggle, Note.Flags.Tap);
    }

    void SetTapNote()
    {
        // if (currentNote == prevNote)
        // {
        //     var newFlags = currentNote.flags;
        // 
        //     if (currentNote != null)
        //     {
        //         if (tapToggle.isOn)
        //             newFlags |= Note.Flags.Tap;
        //         else
        //             newFlags &= ~Note.Flags.Tap;
        //     }
        // 
        //     SetNewFlags(currentNote, newFlags);
        // }
    }

    void SetNoteToolFlag(ref Note.Flags flags, Toggle uiToggle, Note.Flags flagsToToggle)
    {
        if ((flags & flagsToToggle) == 0)
            flags |= flagsToToggle;
        else
            flags &= ~flagsToToggle;
    }

    public void setForced()
    {
        if (toggleBlockingActive)
            return;

        if (IsInNoteTool())
        {
            SetForcedNoteTool();
        }
        else
        {
            SetForcedNote();
        }
    }

    public void setCymbal()
    {
        if (toggleBlockingActive)
            return;

        if (IsInNoteTool())
        {
            SetCymbalNoteTool();
        }
        else
        {
            SetCymbalNote();
        }
    }

    public void setDoubleKick()
    {
        if (toggleBlockingActive)
            return;

        if (IsInNoteTool())
        {
            SetDoubleKickNoteTool();
        }
        else
        {
            SetDoubleKickNote();
        }
    }

    void SetForcedNote()
    {
        // if (currentNote == prevNote)
        // {
        //     var newFlags = currentNote.flags;
        // 
        //     if (currentNote != null)
        //     {
        //         if (forcedToggle.isOn)
        //             newFlags |= Note.Flags.Forced;
        //         else
        //             newFlags &= ~Note.Flags.Forced;
        //     }
        // 
        //     SetNewFlags(currentNote, newFlags);
        // }
    }

    void SetForcedNoteTool()
    {
        // if (forcedToggle.interactable)
        //     SetNoteToolFlag(ref noteToolController.desiredFlags, forcedToggle, Note.Flags.Forced);
    }

    void SetCymbalNote()
    {
        // if (currentNote == prevNote)
        // {
        //     var newFlags = currentNote.flags;
        // 
        //     if (currentNote != null)
        //     {
        //         if (cymbalToggle.isOn)
        //             newFlags |= Note.Flags.ProDrums_Cymbal;
        //         else
        //             newFlags &= ~Note.Flags.ProDrums_Cymbal;
        //     }
        // 
        //     SetNewFlags(currentNote, newFlags);
        // }

    }

    void SetCymbalNoteTool()
    {
        // if (cymbalToggle.interactable)
        //     SetNoteToolFlag(ref noteToolController.desiredFlags, cymbalToggle, Note.Flags.ProDrums_Cymbal);
    }

    void SetDoubleKickNote()
    {
        // if (currentNote == prevNote)
        // {
        //     var newFlags = currentNote.flags;
        // 
        //     if (currentNote != null)
        //     {
        //         if (doubleKickToggle.isOn)
        //             newFlags |= Note.Flags.DoubleKick;
        //         else
        //             newFlags &= ~Note.Flags.DoubleKick;
        //     }
        // 
        //     SetNewFlags(currentNote, newFlags);
        // }

    }

    void SetDoubleKickNoteTool()
    {
        // if (doubleKickToggle.interactable)
        //     SetNoteToolFlag(ref noteToolController.desiredFlags, doubleKickToggle, Note.Flags.DoubleKick);
    }

    void SetNewFlags(Note note, Note.Flags newFlags)
    {
        if (note.flags == newFlags)
            return;

        if (editor.toolManager.currentToolId == EditorObjectToolManager.ToolID.Cursor)
        {
            Note newNote = new Note(note.tick, note.rawNote, note.length, newFlags);
            SongEditModifyValidated command = new SongEditModifyValidated(note, newNote);
            editor.commandStack.Push(command);
        }
        else
        {
            // Updating note tool parameters and visuals
            noteToolController.desiredFlags = newFlags;
        }
    }
}
