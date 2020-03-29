﻿// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;

[System.Serializable]
public abstract class SongObject
{
    /// <summary>
    /// The song this object is connected to.
    /// </summary>
    [System.NonSerialized]
    public Song song;
    /// <summary>
    /// The tick position of the object
    /// </summary>
    public uint tick;
    /// <summary>
    /// Unity only.
    /// </summary>
    [System.NonSerialized]
    public SongObjectController controller;

    public abstract int classID { get; }

    public SongObject (uint _tick)
    {
        tick = _tick;
    }
    
    public float worldYPosition
    {
        get
        {
            return song.TickToWorldYPosition(tick);
        }
    }

    /// <summary>
    /// Automatically converts the object's tick position into the time it will appear in the song.
    /// </summary>
    public float time
    {
        get
        {
            return song.TickToTime(tick, song.resolution);
        }
    }

    internal abstract string GetSaveString();

    /// <summary>
    /// Removes this object from it's song/chart
    /// </summary>
    /// <param name="update">Automatically update all read-only arrays? 
    /// If set to false, you must manually call the updateArrays() method, but is useful when deleting multiple objects as it increases performance dramatically.</param>
    public virtual void Delete(bool update = true)
    {
        if (controller)
        {
            controller.gameObject.SetActive(false);
        }
    }
    public abstract SongObject Clone();

    public T CloneAs<T>() where T : SongObject
    {
        T clone = this.Clone() as T;
        Debug.Assert(clone != null, "Clone As casting type was incorrect");
        return clone;
    }

    public abstract bool AllValuesCompare<T>(T songObject) where T : SongObject;
    
    public static bool operator ==(SongObject a, SongObject b)
    {
        bool aIsNull = ReferenceEquals(a, null);
        bool bIsNull = ReferenceEquals(b, null);

        if (aIsNull || bIsNull)
        {
            if (aIsNull == bIsNull)
                return true;
            else
                return false;
        }
        else
            return a.Equals(b);
    }

    protected virtual bool Equals(SongObject b)
    {
        return tick == b.tick && classID == b.classID;
    }

    public static bool operator !=(SongObject a, SongObject b)
    {
        return !(a == b);
    }

    protected virtual bool LessThan(SongObject b)
    {
        if (tick < b.tick)
            return true;
        else if (tick == b.tick && classID < b.classID)
            return true;
        else
            return false;
    }

    public static bool operator <(SongObject a, SongObject b)
    {
        return a.LessThan(b);
    }

    public static bool operator >(SongObject a, SongObject b)
    {
        if (a != b)
            return !(a < b);
        else
            return false;
    }

    public static bool operator <=(SongObject a, SongObject b)
    {
        return (a < b || a == b);
    }

    public static bool operator >=(SongObject a, SongObject b)
    {
        return (a > b || a == b);
    }

    public override bool Equals(System.Object obj)
    {
        return base.Equals(obj);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    /// <summary>
    /// Allows different classes to be sorted and grouped together in arrays by giving each class a comparable numeric value that is greater or less than other classes.
    /// </summary>
    public enum ID
    {
        TimeSignature, BPM, Anchor, Event, Section, Note, Starpower, ChartEvent
    }
}
