﻿// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

namespace MoonscraperChartEditor.Song.IO
{
    public static class ChartIOHelper
    {
        public enum FileSubType
        {
            Default,

            // Stores space characters found in ChartEvent objects as Japanese full-width spaces. Need to convert this back when loading.
            MoonscraperPropriety,
        }

        public const string
            c_dataBlockSong = "[Song]"
            , c_dataBlockSyncTrack = "[SyncTrack]"
            , c_dataBlockEvents = "[Events]"
            ;

        public const int c_proDrumsOffset = 64;
        public const int c_instrumentPlusOffset = 32;
        public const int c_starpowerId = 2;
        public const int c_starpowerDrumFillId = 64;

        public static readonly Dictionary<int, int> c_guitarNoteNumLookup = new Dictionary<int, int>()
    {
        { 0, (int)Note.GuitarFret.Green     },
        { 1, (int)Note.GuitarFret.Red       },
        { 2, (int)Note.GuitarFret.Yellow    },
        { 3, (int)Note.GuitarFret.Blue      },
        { 4, (int)Note.GuitarFret.Orange    },
        { 7, (int)Note.GuitarFret.Open      },
    };

        public static readonly Dictionary<int, Note.Flags> c_guitarFlagNumLookup = new Dictionary<int, Note.Flags>()
    {
        { 5      , Note.Flags.Forced },
        { 6      , Note.Flags.Tap },
    };

        public static readonly Dictionary<int, int> c_drumNoteNumLookup = new Dictionary<int, int>()
    {
        { 0, (int)Note.DrumPad.Kick      },
        { 1, (int)Note.DrumPad.Red       },
        { 2, (int)Note.DrumPad.Yellow    },
        { 3, (int)Note.DrumPad.Blue      },
        { 4, (int)Note.DrumPad.Orange    },
        { 5, (int)Note.DrumPad.Green     },
    };

        public static readonly Dictionary<int, int> c_drumNoteToSaveNumberLookup = c_drumNoteNumLookup.ToDictionary((i) => i.Value, (i) => i.Key);

        public static readonly Dictionary<int, Note.Flags> c_drumFlagNumLookup = new Dictionary<int, Note.Flags>()
    {
        { c_proDrumsOffset + 2, Note.Flags.ProDrums_Cymbal },       // Yellow save num from c_drumNoteNumLookup
        { c_proDrumsOffset + 3, Note.Flags.ProDrums_Cymbal },       // Blue save num from c_drumNoteNumLookup
        { c_proDrumsOffset + 4, Note.Flags.ProDrums_Cymbal },       // Orange (Green in 4-lane) save num from c_drumNoteNumLookup
        { c_instrumentPlusOffset, Note.Flags.InstrumentPlus },      // Double Kick
    };

        // Default flags, mark as cymbal for pro drums automatically. Also used for choosing whether to write flag information or not if it's like this by default in the first place.
        public static readonly Dictionary<int, Note.Flags> c_drumNoteDefaultFlagsLookup = new Dictionary<int, Note.Flags>()
    {
        { (int)Note.DrumPad.Kick      , Note.Flags.None },
        { (int)Note.DrumPad.Red       , Note.Flags.None },
        { (int)Note.DrumPad.Yellow    , Note.Flags.None },
        { (int)Note.DrumPad.Blue      , Note.Flags.None },
        { (int)Note.DrumPad.Orange    , Note.Flags.None },   // Orange becomes green during 4-lane
        { (int)Note.DrumPad.Green     , Note.Flags.None },
    };

        public static readonly Dictionary<int, int> c_ghlNoteNumLookup = new Dictionary<int, int>()
    {
        { 0, (int)Note.GHLiveGuitarFret.White1     },
        { 1, (int)Note.GHLiveGuitarFret.White2       },
        { 2, (int)Note.GHLiveGuitarFret.White3    },
        { 3, (int)Note.GHLiveGuitarFret.Black1      },
        { 4, (int)Note.GHLiveGuitarFret.Black2    },
        { 8, (int)Note.GHLiveGuitarFret.Black3      },
        { 7, (int)Note.GHLiveGuitarFret.Open      },
    };

        public static readonly Dictionary<int, Note.Flags> c_ghlFlagNumLookup = c_guitarFlagNumLookup;

        public static readonly Dictionary<string, Song.Difficulty> c_trackNameToTrackDifficultyLookup = new Dictionary<string, Song.Difficulty>()
    {
        { "Easy",   Song.Difficulty.Easy    },
        { "Medium", Song.Difficulty.Medium  },
        { "Hard",   Song.Difficulty.Hard    },
        { "Expert", Song.Difficulty.Expert  },
    };

        public static readonly Dictionary<string, Song.Instrument> c_instrumentStrToEnumLookup = new Dictionary<string, Song.Instrument>()
    {
        { "Amp1",           Song.Instrument.Amp1 },
        { "Amp2",           Song.Instrument.Amp2 },
        { "Amp3",           Song.Instrument.Amp3 },
        { "Amp4",           Song.Instrument.Amp4 },
        { "Amp5",           Song.Instrument.Amp5 },
        { "Amp6",           Song.Instrument.Amp6 },
    };

        public static readonly Dictionary<Song.Instrument, Song.Instrument> c_instrumentParsingTypeLookup = new Dictionary<Song.Instrument, Song.Instrument>()
    {
        // Other instruments default to loading as a guitar type track
        // { Song.Instrument.Drums,          Song.Instrument.Drums },
        // { Song.Instrument.GHLiveGuitar ,  Song.Instrument.GHLiveGuitar },
        // { Song.Instrument.GHLiveBass ,  Song.Instrument.GHLiveBass },
    };

        public static class MetaData
        {
            const string QUOTEVALIDATE = @"""[^""\\]*(?:\\.[^""\\]*)*""";
            const string QUOTESEARCH = "\"([^\"]*)\"";
            const string FLOATSEARCH = @"[\-\+]?\d+(\.\d+)?";       // US culture only

            public static readonly System.Globalization.CultureInfo c_cultureInfo = new System.Globalization.CultureInfo("en-US");

            public enum MetadataValueType
            {
                String,
                Float,
                Player2,
                Difficulty,
                Year,
            }

            public class MetadataItem
            {
                string m_key;
                Regex m_readerParseRegex;
                string m_saveFormat;

                static readonly string c_metaDataSaveFormat = string.Format("{0}{{0}} = \"{{{{0}}}}\"{1}", Globals.TABSPACE, Globals.LINE_ENDING);
                static readonly string c_metaDataSaveFormatNoQuote = string.Format("{0}{{0}} = {{{{0}}}}{1}", Globals.TABSPACE, Globals.LINE_ENDING);

                public string key { get { return m_key; } }
                public Regex regex { get { return m_readerParseRegex; } }
                public string saveFormat { get { return m_saveFormat; } }

                public MetadataItem(string key, MetadataValueType type)
                {
                    m_key = key;

                    Regex parseStrRegex = new Regex(key + " = " + QUOTEVALIDATE, RegexOptions.Compiled);

                    switch (type)
                    {
                        case MetadataValueType.String:
                            {
                                m_readerParseRegex = parseStrRegex;
                                m_saveFormat = string.Format(c_metaDataSaveFormat, key);
                                break;
                            }

                        case MetadataValueType.Float:
                            {
                                m_readerParseRegex = new Regex(key + " = " + FLOATSEARCH, RegexOptions.Compiled);
                                m_saveFormat = string.Format(c_cultureInfo, c_metaDataSaveFormatNoQuote, key);
                                break;
                            }

                        case MetadataValueType.Player2:
                            {
                                m_readerParseRegex = new Regex(key + @" = \w+", RegexOptions.Compiled);
                                m_saveFormat = string.Format(c_metaDataSaveFormatNoQuote, key);
                                break;
                            }

                        case MetadataValueType.Difficulty:
                            {
                                m_readerParseRegex = new Regex(key + @" = \d+", RegexOptions.Compiled);
                                m_saveFormat = string.Format(c_metaDataSaveFormatNoQuote, key);
                                break;
                            }

                        case MetadataValueType.Year:
                            {
                                m_readerParseRegex = parseStrRegex;
                                m_saveFormat = string.Format("{0}{1} = \", {{0}}\"{2}", Globals.TABSPACE, "Year", Globals.LINE_ENDING);
                                break;
                            }

                        default:
                            throw new System.Exception("Unhandled Metadata item type");
                    }
                }
            }

            public readonly static MetadataItem name = new MetadataItem("Name", MetadataValueType.String);
            public readonly static MetadataItem artist = new MetadataItem("Artist", MetadataValueType.String);
            public readonly static MetadataItem charter = new MetadataItem("Charter", MetadataValueType.String);
            public readonly static MetadataItem offset = new MetadataItem("Offset", MetadataValueType.Float);
            public readonly static MetadataItem resolution = new MetadataItem("Resolution", MetadataValueType.Float);
            public readonly static MetadataItem player2 = new MetadataItem("Player2", MetadataValueType.Player2);
            public readonly static MetadataItem difficulty = new MetadataItem("Difficulty", MetadataValueType.Difficulty);
            public readonly static MetadataItem length = new MetadataItem("Length", MetadataValueType.Float);
            public readonly static MetadataItem previewStart = new MetadataItem("PreviewStart", MetadataValueType.Float);
            public readonly static MetadataItem previewEnd = new MetadataItem("PreviewEnd", MetadataValueType.Float);
            public readonly static MetadataItem genre = new MetadataItem("Genre", MetadataValueType.String);
            public readonly static MetadataItem year = new MetadataItem("Year", MetadataValueType.Year);
            public readonly static MetadataItem album = new MetadataItem("Album", MetadataValueType.String);
            public readonly static MetadataItem mediaType = new MetadataItem("MediaType", MetadataValueType.String);
            public readonly static MetadataItem musicStream = new MetadataItem("MusicStream", MetadataValueType.String);
            public readonly static MetadataItem guitarStream = new MetadataItem("GuitarStream", MetadataValueType.String);
            public readonly static MetadataItem bassStream = new MetadataItem("BassStream", MetadataValueType.String);
            public readonly static MetadataItem rhythmStream = new MetadataItem("RhythmStream", MetadataValueType.String);
            public readonly static MetadataItem drumStream = new MetadataItem("DrumStream", MetadataValueType.String);
            public readonly static MetadataItem drum2Stream = new MetadataItem("Drum2Stream", MetadataValueType.String);
            public readonly static MetadataItem drum3Stream = new MetadataItem("Drum3Stream", MetadataValueType.String);
            public readonly static MetadataItem drum4Stream = new MetadataItem("Drum4Stream", MetadataValueType.String);
            public readonly static MetadataItem vocalStream = new MetadataItem("VocalStream", MetadataValueType.String);
            public readonly static MetadataItem keysStream = new MetadataItem("KeysStream", MetadataValueType.String);
            public readonly static MetadataItem crowdStream = new MetadataItem("CrowdStream", MetadataValueType.String);

            public static string ParseAsString(string line)
            {
                return Regex.Matches(line, QUOTESEARCH)[0].ToString().Trim('"');
            }

            public static float ParseAsFloat(string line)
            {
                return float.Parse(Regex.Matches(line, FLOATSEARCH)[0].ToString(), c_cultureInfo);  // .chart format only allows '.' as decimal seperators. Need to parse correctly under any locale.
            }

            public static short ParseAsShort(string line)
            {
                return short.Parse(Regex.Matches(line, FLOATSEARCH)[0].ToString());
            }
        }
    }
}
