// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using UnityEngine.UI;
using MoonscraperChartEditor.Song;

public class MenuBar : UpdateableService {
    ChartEditor editor;

    [Header("Disableable UI")]
    [SerializeField]
    Button undoButton;
    [SerializeField]
    Button redoButton;

    [Header("Shortcuts")]
    [SerializeField]
    Toggle mouseModeToggle;

    [Header("Preview Mode")]
    [SerializeField]
    GameObject[] disableDuringPreview;

    [Header("Misc")]
    [SerializeField]
    Button playButton;
    [SerializeField]
    Button gameplayButton;

    public static Song.Instrument currentInstrument = Song.Instrument.Amp1;
    public static Song.Difficulty currentDifficulty = Song.Difficulty.Expert;

    int desiredLaneCount = 0;

    // Use this for initialization
    protected override void Start () {
        editor = ChartEditor.Instance;

        editor.events.chartReloadedEvent.Register(PlayEnabledCheck);

        base.Start();
    }

    // Update is called once per frame
    public override void OnServiceUpdate()
    {
        if (editor.currentState == ChartEditor.State.Editor)
        {
            undoButton.interactable = editor.services.CanUndo();
            redoButton.interactable = editor.services.CanRedo();
        }
        else
        {
            undoButton.interactable = false;
            redoButton.interactable = false;
        }

        PlayEnabledCheck();

        if (!Services.IsTyping)
            Controls();
    }

    void Controls()
    {
        // if (MSChartEditorInput.GetInputDown(MSChartEditorInputActions.ToggleExtendedSustains))
        // {
        //     ToggleExtendedSustains();
        //     editor.globals.services.notificationBar.PushNotification("EXTENDED SUSTAINS TOGGLED " + Services.BoolToStrOnOff(Globals.gameSettings.extendedSustainsEnabled), 2, true);
        // }

            // else if (MSChartEditorInput.GetInputDown(MSChartEditorInputActions.ToggleMouseMode))
        if (MSChartEditorInput.GetInputDown(MSChartEditorInputActions.ToggleMouseMode))
        {
            ToggleMouseLockMode();
            editor.globals.services.notificationBar.PushNotification("KEYS MODE TOGGLED " + Services.BoolToStrOnOff(Globals.gameSettings.keysModeEnabled), 2, true);
        }
    }

    public void ToggleMouseLockMode(bool value)
    {
        editor.services.SetKeysMode(value);
        Debug.Log("Keys mode toggled " + value);
    }

    public void ToggleMouseLockMode()
    {
        editor.services.SetKeysMode(!Globals.gameSettings.keysModeEnabled);
        Debug.Log("Keys mode toggled " + Globals.gameSettings.keysModeEnabled);
    }

    public void ToggleExtendedSustains()
    {
        Globals.gameSettings.extendedSustainsEnabled.value = !Globals.gameSettings.extendedSustainsEnabled;
        Debug.Log("Extended sustains toggled " + Globals.gameSettings.extendedSustainsEnabled);
    }

    public void ToggleMetronome()
    {
        Globals.gameSettings.metronomeActive = !Globals.gameSettings.metronomeActive;
        Debug.Log("Metronome toggled " + Globals.gameSettings.metronomeActive);
    }

    public void SetInstrument(string value)
    {
        try
        {
            currentInstrument = (Song.Instrument)System.Enum.Parse(typeof(Song.Instrument), value, true); 
        }
        catch
        {
            Debug.LogError("Invalid instrument set: " + value);
        }

        desiredLaneCount = 0;
    }

    public void SetDifficulty(string value)
    {
        try
        {
            currentDifficulty = (Song.Difficulty)System.Enum.Parse(typeof(Song.Difficulty), value, true);
        }
        catch
        {
            Debug.LogError("Invalid difficulty set: " + value);
        }
    }

    public void SetLaneCount(int value)
    {
        desiredLaneCount = value;
    }

    public void LoadCurrentInstumentAndDifficulty()
    {
        if (!editor)
            editor = ChartEditor.Instance;

        editor.LoadChart(editor.currentSong.GetChart(currentInstrument, currentDifficulty));
        editor.selectedObjectsManager.currentSelectedObject = null;

        editor.events.chartReloadedEvent.Fire();

        if (desiredLaneCount > 0)
            editor.laneInfo.laneCount = desiredLaneCount;
    }

    public static bool previewing { get { return _previewing; } }
    static bool _previewing = false;
    float initialPosition = 0;
    public void PreviewToggle()
    {
        _previewing = !_previewing;

        // Disable UI
        foreach(GameObject go in disableDuringPreview)
        {
            go.SetActive(!_previewing);
        }

        // Set chart view mode
        Globals.viewMode = Globals.ViewMode.Chart;

        if (_previewing)
        {
            // Record the current position
            initialPosition = editor.visibleStrikeline.position.y;

            // Move to the start of the chart
            editor.movement.SetTime(-3);

            // Play
            editor.Play();
        }
        else
        {
            // Stop
            editor.Stop();

            // Restore to initial position
            editor.movement.SetTime(ChartEditor.WorldYPositionToTime(initialPosition));
        }
    }

    void PlayEnabledCheck()
    {
        playButton.interactable = editor.services.CanPlay();
        gameplayButton.interactable = !Globals.ghLiveMode && editor.services.CanPlay();
    }

    #region Difficulty Checkbox Fns
    void SetDifficultyToggle(Toggle toggle, Song.Difficulty diff)
    {
        toggle.isOn = ChartEditor.Instance.currentDifficulty == diff;
    }

    public void SetExpertToggleCheckbox(Toggle toggle)
    {
        SetDifficultyToggle(toggle, Song.Difficulty.Expert);
    }

    public void SetHardToggleCheckbox(Toggle toggle)
    {
        SetDifficultyToggle(toggle, Song.Difficulty.Hard);
    }

    public void SetMediumToggleCheckbox(Toggle toggle)
    {
        SetDifficultyToggle(toggle, Song.Difficulty.Medium);
    }

    public void SetEasyToggleCheckbox(Toggle toggle)
    {
        SetDifficultyToggle(toggle, Song.Difficulty.Easy);
    }
    #endregion

    #region Instrument Checkbox Fns
    void SetInstrumentToggle(Toggle toggle, Song.Instrument instrument)
    {
        toggle.isOn = ChartEditor.Instance.currentInstrument == instrument;
    }

    public void SetAmp1InstrumentToggle(Toggle toggle)
    {
        SetInstrumentToggle(toggle, Song.Instrument.Amp1);
    }
    public void SetAmp2InstrumentToggle(Toggle toggle)
    {
        SetInstrumentToggle(toggle, Song.Instrument.Amp2);
    }
    public void SetAmp3InstrumentToggle(Toggle toggle)
    {
        SetInstrumentToggle(toggle, Song.Instrument.Amp3);
    }
    public void SetAmp4InstrumentToggle(Toggle toggle)
    {
        SetInstrumentToggle(toggle, Song.Instrument.Amp4);
    }
    public void SetAmp5InstrumentToggle(Toggle toggle)
    {
        SetInstrumentToggle(toggle, Song.Instrument.Amp5);
    }
    public void SetAmp6InstrumentToggle(Toggle toggle)
    {
        SetInstrumentToggle(toggle, Song.Instrument.Amp6);
    }


    #endregion

    #region User Settings Toggles
    void SetSettingsToggleCheckbox(Toggle toggle, string settingsToggleKey)
    {
        toggle.isOn = Globals.gameSettings.GetBoolSetting(settingsToggleKey);
    }

    public void SetExtendedSustainsSettingToggle(Toggle toggle)
    {
        SetSettingsToggleCheckbox(toggle, "extendedSustainsEnabled");
    }

    public void SetKeysModeSettingToggle(Toggle toggle)
    {
        SetSettingsToggleCheckbox(toggle, "keysModeEnabled");
    }

    public void SetMetronomeSettingToggle(Toggle toggle)
    {
        SetSettingsToggleCheckbox(toggle, "metronomeActive");
    }
    #endregion
}
