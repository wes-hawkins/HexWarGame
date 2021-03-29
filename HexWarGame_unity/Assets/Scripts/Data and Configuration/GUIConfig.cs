using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "WarGame/GUIConfig")]
public class GUIConfig : SingletonScriptableObject<GUIConfig> {

    [Header("Main UI")]
    [Tooltip("How many pixels the main UI elements will be inset from the edges of the screen.")]
    [SerializeField] private float insetMargin = 0.35f; public static float InsetMargin { get { return Inst.insetMargin; } }

    [Header("Diegetic")]
    [SerializeField] private float gridOutlineThickness = 0.35f; public static float GridOutlineThickness { get { return Inst.gridOutlineThickness; } }
    
} // End of TerrainConfig class.
