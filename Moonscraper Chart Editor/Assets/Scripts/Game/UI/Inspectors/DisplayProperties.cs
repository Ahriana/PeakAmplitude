﻿// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class DisplayProperties : UpdateableService
{
    public Button menuTabButton;
    public RectTransform settingsMenuOption;
    public RectTransform statsMenuOption;

    [Header("Settings")]
    public Text songNameText;
    public Slider hyperspeedSlider;
    public InputField snappingStep;  
    public Text gameSpeed;
    public Slider gameSpeedSlider;
    public Toggle clapToggle; 
    public Toggle metronomeToggle;
    public Slider highwayLengthSlider;
    public Transform maxHighwayLength;
    public float minHighwayLength = 11.75f;
    [SerializeField]
    BGFadeHeightController bgFade;

    [Header("Stats")]
    public Text noteCount;
    public Text spCount;
    public Text localEventsCount;
    public Text bpmCount;
    public Text tsCount;
    public Text sectionCount;
    public Text globalEventsCount;

    ChartEditor editor;

    // Cache whether we need to modify strings or not
    int prevSnappingStep = 16;
    int prevNoteCount = -1;
    int prevSpCount = -1;
    int prevLocalEventCount = -1;
    int prevBpmCount = -1;
    int prevTsCount = -1;
    int prevSecCount = -1;
    int prevGlobalEventCount = -1;

    enum MenuTabOptions
    {
        Settings,
        Stats,
    }

    MenuTabOptions currentMenuTab = MenuTabOptions.Settings;
    const string c_menuTabTextFormat = "< {0} >";

    protected override void Start()
    {
        editor = ChartEditor.Instance;
        hyperspeedSlider.value = Globals.gameSettings.hyperspeed;
        highwayLengthSlider.value = Globals.gameSettings.highwayLength;

        UpdateGameSpeedText();

        snappingStep.onValidateInput = Step.validateStepVal;
        UpdateSnappingStepText();

        OnEnable();

        editor.events.chartReloadedEvent.Register(OnChartReload);
        editor.events.editorStateChangedEvent.Register(OnApplicationModeChanged);

        OnChartReload();

        base.Start();
    }

    void OnEnable()
    {
        clapToggle.isOn = Globals.gameSettings.clapEnabled;
        metronomeToggle.isOn = Globals.gameSettings.metronomeActive;

        UpdateMenuTabDisplay();
    }

    public override void OnServiceUpdate()
    {
        var currentSong = editor.currentSong;
        var currentChart = editor.currentChart;

        // Update strings
        {
            int currentNoteCount = currentChart.note_count;
            int currentSpCount = currentChart.starPower.Count;
            int currentLocalEventCount = currentChart.events.Count;
            int currentBpmCount = currentSong.bpms.Count;
            int currentTsCount = currentSong.timeSignatures.Count;
            int currentSecCount = currentSong.sections.Count;
            int currentGlobalEventCount = currentSong.events.Count;

            if (currentNoteCount != prevNoteCount)
                noteCount.text = "Notes: " + currentNoteCount.ToString();

            if (currentSpCount != prevSpCount)
                spCount.text = "Starpower: " + currentSpCount.ToString();

            if (currentLocalEventCount != prevLocalEventCount)
                localEventsCount.text = "Local Events: " + currentLocalEventCount.ToString();

            if (currentBpmCount != prevBpmCount)
                bpmCount.text = "BPM: " + currentBpmCount.ToString();

            if (currentTsCount != prevTsCount)
                tsCount.text = "Timesignatures: " + currentTsCount.ToString();

            if (currentSecCount != prevSecCount)
                sectionCount.text = "Sections: " + currentSecCount.ToString();

            if (currentGlobalEventCount != prevGlobalEventCount)
                globalEventsCount.text = "Global Events: " + currentGlobalEventCount.ToString();

            if (Globals.gameSettings.step != prevSnappingStep)
                UpdateSnappingStepText();

            prevNoteCount = currentNoteCount;
            prevSpCount = currentSpCount;
            prevLocalEventCount = currentLocalEventCount;
            prevBpmCount = currentBpmCount;
            prevTsCount = currentTsCount;
            prevSecCount = currentSecCount;
            prevGlobalEventCount = currentGlobalEventCount;
        }

        // Shortcuts
        if (MSChartEditorInput.GetInputDown(MSChartEditorInputActions.ToggleClap))
            clapToggle.isOn = !clapToggle.isOn;
    }

    void OnChartReload()
    {
        UpdateSongNameText();
    }

    public void UpdateSongNameText()
    {
        songNameText.text = editor.currentSong.name + " - " + editor.currentChart.name;
    }

    void OnApplicationModeChanged(in ChartEditor.State editorState)
    {
        bool interactable = (editorState != ChartEditor.State.Playing);
        hyperspeedSlider.interactable = interactable;
        gameSpeedSlider.interactable = interactable;
        highwayLengthSlider.interactable = interactable;
    }

    public void SetHyperspeed(float value)
    {
        Globals.gameSettings.hyperspeed = value;
        editor.events.hyperspeedChangeEvent.Fire();
    }

    public void SetGameSpeed(float value)
    {
        value = Mathf.Round(value / 5.0f) * 5;
        Globals.gameSettings.gameSpeed = value / 100.0f;
        UpdateGameSpeedText();

        editor.events.hyperspeedChangeEvent.Fire();
    }

    void UpdateGameSpeedText()
    {
        gameSpeed.text = string.Format("Speed- x{0:0.00}", Globals.gameSettings.gameSpeed);
    }

    public void SetHighwayLength(float value)
    {
        Globals.gameSettings.highwayLength = value;

        Vector3 pos = Vector3.zero;
        pos.y = value * 5 + minHighwayLength;
        maxHighwayLength.transform.localPosition = pos;

        bgFade.AdjustHeight();

        editor.events.hyperspeedChangeEvent.Fire();
    }

    public void ToggleClap(bool value)
    {
        Globals.gameSettings.clapEnabled = value;

        Debug.Log("Clap toggled: " + value);
    }

    public void ToggleMetronome(bool value)
    {
        Globals.gameSettings.metronomeActive = value;

        Debug.Log("Metronome toggled: " + value);
    }

    public void IncrementSnappingStep()
    {
        Globals.gameSettings.snappingStep.Increment();
        UpdateSnappingStepText();
    }

    public void DecrementSnappingStep()
    {
        Globals.gameSettings.snappingStep.Decrement();
        UpdateSnappingStepText();
    }

    void UpdateSnappingStepText()
    {
        snappingStep.text = Globals.gameSettings.step.ToString();
        prevSnappingStep = Globals.gameSettings.step;
    }

    public void SetStep(string value)
    {
        if (value != string.Empty)
        {
            StepInputEndEdit(value);
        }
    }

    public void StepInputEndEdit(string value)
    {
        int stepVal;
        const int defaultControlsStepVal = 16;

        if (value == string.Empty)
            stepVal = defaultControlsStepVal;
        else
        {
            try
            {
                stepVal = int.Parse(value);

                if (stepVal < Step.MIN_STEP)
                    stepVal = Step.MIN_STEP;
                else if (stepVal > Step.FULL_STEP)
                    stepVal = Step.FULL_STEP;
            }
            catch
            {
                stepVal = defaultControlsStepVal;
            }
        }

        Globals.gameSettings.step = stepVal;
        UpdateSnappingStepText();
    }

    public void NextMenuTab()
    {
        currentMenuTab += 1;
        if ((int)currentMenuTab >= MoonscraperEngine.EnumX<MenuTabOptions>.Count)
        {
            currentMenuTab = 0;
        }

        UpdateMenuTabDisplay();
    }

    void UpdateMenuTabDisplay()
    {
        menuTabButton.GetComponentInChildren<Text>().text = string.Format(c_menuTabTextFormat, currentMenuTab.ToString());

        statsMenuOption.gameObject.SetActive(false);
        settingsMenuOption.gameObject.SetActive(false);

        switch (currentMenuTab)
        {
            case MenuTabOptions.Stats:
                {
                    statsMenuOption.gameObject.SetActive(true);
                    break;
                }
            case MenuTabOptions.Settings:
            default:
                {
                    settingsMenuOption.gameObject.SetActive(true);
                    break;
                }
        }
    }
}
