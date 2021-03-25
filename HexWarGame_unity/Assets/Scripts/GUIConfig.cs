using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "WarGame/GUIConfig")]
public class GUIConfig : SingletonScriptableObject<GUIConfig> {

    [SerializeField] private float gridOutlineThickness = 0.35f; public static float GridOutlineThickness { get { return Inst.gridOutlineThickness; } }
    
} // End of TerrainConfig class.
