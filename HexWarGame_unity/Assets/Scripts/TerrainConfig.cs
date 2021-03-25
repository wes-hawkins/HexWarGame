using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "WarGame/TerrainConfig")]
public class TerrainConfig : SingletonScriptableObject<TerrainConfig> {

    [Header("Ground height is 0. Mountain height is highest. OceanFloor is lowest.")]
    [SerializeField] private float mountainHeight = 0.8f; public static float MountainHeight { get { return Inst.mountainHeight; } }
    [SerializeField] private float snowcapHeight = 0.7f; public static float SnowcapHeight { get { return Inst.snowcapHeight; } }
    [Tooltip("Over how much elevation do snowcaps 'blend in'")]
    [SerializeField] private float snowcapBlend = 0.2f; public static float SnowcapBlend { get { return Inst.snowcapBlend; } }
    [Tooltip("Y position units will rest on if on the surface of a 'mountain' hex.")]
    [SerializeField] private float mountainUnitHeight = 0.2f; public static float MountainUnitHeight { get { return Inst.mountainUnitHeight; } }

    [Space()]
    [SerializeField] private float highAltitude = 1f; public static float HighAltitude { get { return Inst.highAltitude; } }
    [SerializeField] private float lowAltitude = 1f; public static float LowAltitude { get { return Inst.lowAltitude; } }
    [SerializeField] private float seaLevel = -0.1f; public static float SeaLevel { get { return Inst.seaLevel; } }
    [SerializeField] private float shallowsDepth = -0.4f; public static float ShallowsDepth { get { return Inst.shallowsDepth; } }
    [SerializeField] private float oceanDepth = -1f; public static float OceanFloor { get { return Inst.oceanDepth; } }
    
} // End of TerrainConfig class.
