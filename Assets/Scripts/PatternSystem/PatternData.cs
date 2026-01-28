using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "newPattern", menuName = "Platform Spawn/Pattern")]
public class PatternData : ScriptableObject
{
    public string patternName;
    public PatternDifficulty difficulty;
    public PatternRow[] rows;
}

[Serializable]
public class PatternRow
{
    public PlatformData[] platforms; // Bu row içindeki platformlar
}

[Serializable]
public class PlatformData
{
    public PlatformType type;
    public int lane;
    public bool hasMeteor;
}

public enum PlatformType { Single, Double, Bounce, Moving, Breakable }
public enum PatternDifficulty { Easy, Middle, Hard }