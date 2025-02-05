﻿// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoonscraperEngine;
using MoonscraperEngine.Input;

// HOW TO ADD A NEW ACTION
/*
 * 1. Add to the enum list
 * 2. Open Scenes/Config Editors/Input Editor
 * 3. Click on the Input Builder object and locate the Input Config Builder script
 * 4. Click the button "Load Config From File"
 * 5. Open the file /Assets/Database/InputPropertiesConfig.json
 * 6. The field InputProperties/Shortcut Input will now be populated. Scroll down to find your new action and set up the new properties
 * 7. Click the button "Save Config To File" and overwrite InputPropertiesConfig.json
 */

public enum MSChartEditorInputActions
{
    ActionHistoryRedo,
    ActionHistoryUndo,

    AddSongObject,

    BpmIncrease,
    BpmDecrease,

    ChordSelect,

    ClipboardCopy,
    ClipboardCut,
    ClipboardPaste,

    Delete, 

    FileLoad,
    FileNew,
    FileSave,
    FileSaveAs,

    LyricEditorSetTime,

    MoveStepPositive,
    MoveStepNegative,
    MoveMeasurePositive,
    MoveMeasureNegative,

    PlayPause,

    ReloadAudio,

    SelectAll,
    SelectAllSection,
    StepDecrease,
    StepIncrease,
    StepDecreaseBy1,
    StepIncreaseBy1,

    SectionJumpPositive,
    SectionJumpNegative,
    SectionJumpMouseScroll,

    ToggleBpmAnchor,
    ToggleClap,
    ToggleMetronome,
    ToggleMouseMode,
    ToggleViewMode, 
    
    ToolNoteBurst,
    ToolSelectCursor,
    ToolSelectEraser,
    ToolSelectNote,
    ToolSelectStarpower,
    ToolSelectBpm,
    ToolSelectTimeSignature,
    ToolSelectSection,
    ToolSelectEvent,

    ToolNoteLane1,
    ToolNoteLane2,
    ToolNoteLane3,

    CloseMenu,

    // Guitar Actions
    GuitarStrumUp,
    GuitarStrumDown,

    GuitarFretGreen,
    GuitarFretRed,
    GuitarFretYellow,
}

public static class MSChartEditorInput
{
    public static class Category
    {
        // static int to make int conversion way easier. The lack of implicit enum->int conversion is annoying as hell.
        public enum CategoryType
        {
            // Add new categories to the bottom of the list! Order is saved out in properties file.
            Global,
            Editor,
            EditorKeyboardMode,
            EditorToolNote,
            
            GameplayGuitar,
            GameplayDrums,
            GameplayDrumsPro,

            EditorToolGroupNote,
            EditorToolStarpower,
        }

        public static InteractionMatrix interactionMatrix = new InteractionMatrix(EnumX<CategoryType>.Count);
        public static readonly int kEditorCategoryMask 
            = (1 << (int)CategoryType.Editor)
            | (1 << (int)CategoryType.EditorKeyboardMode)
            | (1 << (int)CategoryType.EditorToolNote)
            | (1 << (int)CategoryType.EditorToolGroupNote)
            | (1 << (int)CategoryType.EditorToolStarpower)
            | (1 << (int)CategoryType.Global)
            ;
        public static readonly int kGameplayCategoryMask = (1 << (int)CategoryType.GameplayGuitar) | (1 << (int)CategoryType.GameplayDrums) | (1 << (int)CategoryType.GameplayDrumsPro);
        public static readonly int kGameplayGuitarCategoryMask = (1 << (int)CategoryType.GameplayGuitar);
        public static readonly int kGameplayDrumsCategoryMask = (1 << (int)CategoryType.GameplayDrums);
        public static readonly int kGameplayDrumsProCategoryMask = (1 << (int)CategoryType.GameplayDrumsPro);

        static Category()
        {
            interactionMatrix.SetInteractableAll((int)CategoryType.Global);

            // Set editor interactions
            foreach(var value in EnumX<CategoryType>.Values)
            {
                // Every category interacts with itself
                interactionMatrix.SetInteractable((int)value, (int)value);

                // All editor inputs conflict with Top-Level editor categories
                if (((1 << (int)value) & kEditorCategoryMask) != 0)
                    interactionMatrix.SetInteractable((int)value, (int)CategoryType.Editor);
            }
        }
    }

    const bool kRebindableDefault = true;
    const bool kHiddenInListsDefault = false;
    const int kCategoryDefault = (int)Category.CategoryType.Editor;

    static readonly InputAction.Properties kDefaultProperties = new InputAction.Properties { rebindable = kRebindableDefault, hiddenInLists = kHiddenInListsDefault, category = kCategoryDefault };

    public class MSChartEditorActionContainer : InputActionContainer<MSChartEditorInputActions>
    {
        public MSChartEditorActionContainer()  : base(new EnumLookupTable<MSChartEditorInputActions, InputAction>())
        {
            InputManager inputManager = InputManager.Instance;

            for (int i = 0; i < actionConfigCleanLookup.Count; ++i)
            {
                MSChartEditorInputActions scEnum = (MSChartEditorInputActions)i;
                InputAction.Properties properties;
                if (!inputManager.inputProperties.TryGetPropertiesConfig(scEnum, out properties))
                {
                    properties = kDefaultProperties;
                }

                if (string.IsNullOrEmpty(properties.displayName))
                {
                    properties.displayName = scEnum.ToString();
                }

                actionConfigCleanLookup[scEnum] = new InputAction(properties);
            }
        }
    }

    static MSChartEditorActionContainer primaryInputs { get { return Globals.gameSettings.controls; } }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    #region Input Queries

    public static bool GetInputDown(MSChartEditorInputActions key)
    {
        if (ChartEditor.hasFocus && !Services.IsTyping)
        {
            return primaryInputs.GetActionConfig(key).GetInputDown(InputManager.Instance.devices);
        }

        return false;
    }

    public static bool GetInputUp(MSChartEditorInputActions key)
    {
        if (ChartEditor.hasFocus && !Services.IsTyping)
        {
            return primaryInputs.GetActionConfig(key).GetInputUp(InputManager.Instance.devices);
        }

        return false;
    }

    public static bool GetInput(MSChartEditorInputActions key)
    {
        if (ChartEditor.hasFocus && !Services.IsTyping)
        {
            return primaryInputs.GetActionConfig(key).GetInput(InputManager.Instance.devices);
        }

        return false;
    }

    public static float GetAxis(MSChartEditorInputActions key)
    {
        float? value = GetAxisMaybe(key);
        return value.HasValue ? value.Value : 0;
    }

    public static float? GetAxisMaybe(MSChartEditorInputActions key)
    {
        if (ChartEditor.hasFocus && !Services.IsTyping)
        {
            return primaryInputs.GetActionConfig(key).GetAxisMaybe(InputManager.Instance.devices);
        }

        return null;
    }

    public static bool GetGroupInputDown(MSChartEditorInputActions[] keys)
    {
        foreach (MSChartEditorInputActions key in keys)
        {
            if (GetInputDown(key))
                return true;
        }

        return false;
    }

    public static bool GetGroupInputUp(MSChartEditorInputActions[] keys)
    {
        foreach (MSChartEditorInputActions key in keys)
        {
            if (GetInputUp(key))
                return true;
        }

        return false;
    }

    public static bool GetGroupInput(MSChartEditorInputActions[] keys)
    {
        foreach (MSChartEditorInputActions key in keys)
        {
            if (GetInput(key))
                return true;
        }

        return false;
    }

    #endregion
}
