﻿// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

//#undef UNITY_EDITOR

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;
using MoonscraperEngine;
using MoonscraperChartEditor.Song;

public class SongPropertiesPanelController : TabMenu
{
    public Scrollbar verticalScroll;

    [Header("Audio Settings")]
    public Text musicStream;
    public Text guitarStream;
    public Text bassStream;
    public Text rhythmStream;
	public Text keysStream;
	public Text vocalStream;
    public Text drum1Stream;
	public Text drum2Stream;
	public Text drum3Stream;
	public Text drum4Stream;
    public Text crowdStream;

    bool init = false;

    TimeSpan customTime = new TimeSpan();

    static readonly string[] validAudioExtensions = { "ogg", "wav", "mp3", "opus" };
    readonly ExtensionFilter audioExFilter = new ExtensionFilter("Audio files", validAudioExtensions);

    Dictionary<Song.AudioInstrument, Text> m_audioStreamTextLookup;
    readonly string[] INI_SECTION_HEADER = { "Song", "song" };

    protected override void Start()
    {
        base.Start();
    }

    protected override void OnEnable()
    {
        ResetToInitialMenuItem();

        m_audioStreamTextLookup = new Dictionary<Song.AudioInstrument, Text>()
        {
            { Song.AudioInstrument.Song, musicStream },
            { Song.AudioInstrument.Guitar, guitarStream },
            { Song.AudioInstrument.Bass, bassStream },
            { Song.AudioInstrument.Rhythm, rhythmStream },
            { Song.AudioInstrument.Keys, keysStream },
            { Song.AudioInstrument.Vocals, vocalStream },
            { Song.AudioInstrument.Drum, drum1Stream },
            { Song.AudioInstrument.Drums_2, drum2Stream },
            { Song.AudioInstrument.Drums_3, drum3Stream },
            { Song.AudioInstrument.Drums_4, drum4Stream },
            { Song.AudioInstrument.Crowd, crowdStream },
        };

        bool edit = ChartEditor.isDirty;

        base.OnEnable();

        init = true;
        Song song = editor.currentSong;

        // Init audio names
        setAudioTextLabels();
        init = false;

        customTime = TimeSpan.FromSeconds(editor.currentSongLength);

        ChartEditor.isDirty = edit;
        StartCoroutine(ScrollSetDelay());
    }

    protected override void OnDisable()
    {
        base.OnDisable();
    }

    IEnumerator ScrollSetDelay()
    {
        yield return null;
        // verticalScroll.value = 1;
    }

    void Apply()
    {

    }

    public void setSongProperties()
    {
        if (!init)
        {
            ChartEditor.isDirty = true;
        }
    }

    public void RefreshIniDisplay()
    {
        ChartEditor.isDirty = true;
    }

    public void PopulateIniFromGeneralSettings()
    {
        RefreshIniDisplay();
        ChartEditor.isDirty = true;
    }

    public void OnIniInputValueChanged()
    {
        ChartEditor.isDirty = true;
    }

    public void OnIniInputEndEdit()
    {
        RefreshIniDisplay();
    }

    void ClipText(Text text)
    {
        float maxWidth = text.rectTransform.rect.width;
        if (text.preferredWidth > maxWidth)
        {
            int removePos = text.text.Length - 1;
            text.text += "...";

            while (removePos > 0 && text.preferredWidth > maxWidth)
            {
                text.text = text.text.Remove(removePos--, 1);
            }
        }
    }

   void setAudioTextLabels()
    {
        Song song = editor.currentSong;

        foreach (var audio in EnumX<Song.AudioInstrument>.Values)
        {
            Text audioStreamText;

            if (!m_audioStreamTextLookup.TryGetValue(audio, out audioStreamText))
            {
                Debug.Assert(false, "Audio stream UI Text not linked to an Audio Instrument for instrument " + audio.ToString());
                continue;
            }

            if (editor.currentSongAudio.GetAudioIsLoaded(audio))
            {
                audioStreamText.color = Color.white;
                audioStreamText.text = song.GetAudioName(audio);
                ClipText(audioStreamText);
            }
            else
            {
                audioStreamText.color = Color.red;
                audioStreamText.text = "No audio";
            }
        }

        ChartEditor.isDirty = true;
    }

    string GetAudioFile()
    {
        string audioFilepath = string.Empty;
        string defExt = string.Empty;
        foreach(string extention in validAudioExtensions)
        {
            if (defExt != string.Empty)
                defExt += ",";

            defExt += extention;
        }

        FileExplorer.OpenFilePanel(audioExFilter, defExt, out audioFilepath);
        return audioFilepath;
    }

    void LoadAudioStream(Song.AudioInstrument audioInstrument)
    {
        try
        {
            string filepath = GetAudioFile();
            if (editor.currentSongAudio.LoadAudio(filepath, audioInstrument))
            {
                // Record the filepath
                editor.currentSong.SetAudioLocation(audioInstrument, filepath);
                StartCoroutine(SetAudio());
            }
        }
        catch (Exception e)
        {
            Logger.LogException(e, "Could not open audio");
        }
    }

    void ClearAudioStream(Song.AudioInstrument audio)
    {
        editor.currentSongAudio.Clear(audio);
        editor.currentSong.SetAudioLocation(audio, string.Empty);

        setAudioTextLabels();
    }

    public void RefreshAllAudioStreams()
    {
        StartCoroutine(_RefreshAllAudioStreams());
    }

    IEnumerator _RefreshAllAudioStreams()
    {
        editor.ReloadAudio();

        LoadingTasksManager tasksManager = editor.services.loadingTasksManager;

        while (tasksManager.isRunningTask)
            yield return null;

        setAudioTextLabels();
    }

    IEnumerator SetAudio()
    {
        LoadingTasksManager tasksManager = editor.services.loadingTasksManager;

        List<LoadingTask> tasks = new List<LoadingTask>()
        {
            new LoadingTask("Loading audio", () =>
            {
                while (editor.currentSongAudio.isAudioLoading) ;
            })
        };

        tasksManager.KickTasks(tasks);

        while (tasksManager.isRunningTask)
            yield return null;

        setAudioTextLabels();
    }

    public static char ValidateStringMetadataInput(string text, int charIndex, char addedChar)
    {
        if (addedChar == '"')
        {
            return '\0';
        }

        return addedChar;
    }

    // Unity doesn't support calling methods with enum parameters
    // From button click handlers, so we define load/clear audio
    // implementations for each case here.
    #region Unity button click handlers
    public void LoadMusicStream()
    {
        LoadAudioStream(Song.AudioInstrument.Song);
    }

    public void ClearMusicStream()
    {
        ClearAudioStream(Song.AudioInstrument.Song);
    }

    public void LoadGuitarStream()
    {
        LoadAudioStream(Song.AudioInstrument.Guitar);
    }

    public void ClearGuitarStream()
    {
        ClearAudioStream(Song.AudioInstrument.Guitar);
    }

    public void LoadBassStream()
    {
        LoadAudioStream(Song.AudioInstrument.Bass);
    }

    public void ClearBassStream()
    {
        ClearAudioStream(Song.AudioInstrument.Bass);
    }

    public void LoadRhythmStream()
    {
        LoadAudioStream(Song.AudioInstrument.Rhythm);
    }

    public void ClearRhythmStream()
    {
        ClearAudioStream(Song.AudioInstrument.Rhythm);
    }

    public void LoadVocalStream()
    {
        LoadAudioStream(Song.AudioInstrument.Vocals);
    }

    public void ClearVocalStream()
    {
        ClearAudioStream(Song.AudioInstrument.Vocals);
    }

    public void LoadKeysStream()
    {
        LoadAudioStream(Song.AudioInstrument.Keys);
    }

    public void ClearKeysStream()
    {
        ClearAudioStream(Song.AudioInstrument.Keys);
    }

    public void LoadDrum1Stream()
    {
        LoadAudioStream(Song.AudioInstrument.Drum);
    }

    public void ClearDrum1Stream()
    {
        ClearAudioStream(Song.AudioInstrument.Drum);
    }

    public void LoadDrum2Stream()
    {
        LoadAudioStream(Song.AudioInstrument.Drums_2);
    }

    public void ClearDrum2Stream()
    {
        ClearAudioStream(Song.AudioInstrument.Drums_2);
    }

    public void LoadDrum3Stream()
    {
        LoadAudioStream(Song.AudioInstrument.Drums_3);
    }

    public void ClearDrum3Stream()
    {
        ClearAudioStream(Song.AudioInstrument.Drums_3);
    }

    public void LoadDrum4Stream()
    {
        LoadAudioStream(Song.AudioInstrument.Drums_4);
    }

    public void ClearDrum4Stream()
    {
        ClearAudioStream(Song.AudioInstrument.Drums_4);
    }

    public void LoadCrowdStream()
    {
        LoadAudioStream(Song.AudioInstrument.Crowd);
    }

    public void ClearCrowdStream()
    {
        ClearAudioStream(Song.AudioInstrument.Crowd);
    }
    #endregion
}
