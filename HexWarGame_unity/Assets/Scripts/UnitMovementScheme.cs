using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "WarGame/UnitMovementScheme")]
public class UnitMovementScheme : ScriptableObject {

	[System.Serializable]
	public class TerrainMovementSchemeData {
		public TerrainType terrain = TerrainType.openGrass;
		[Tooltip("% of the unit's total move power required for crossing this type of terrain.")]
		public float moveCost = 0.2f;
	} // End of TerrainMovementSchemeData class.

	[SerializeField] private TerrainMovementSchemeData[] schemeData = new TerrainMovementSchemeData[0];
	private Dictionary<TerrainType, TerrainMovementSchemeData> schemeDict = new Dictionary<TerrainType, TerrainMovementSchemeData>();


	public void Init(){
		foreach(TerrainMovementSchemeData data in schemeData)
			schemeDict.Add(data.terrain, data);
	} // End of Init().



	public bool GetCanNavigate(TerrainType type){
		return schemeDict.ContainsKey(type);
	} // End of GetCanNavigate() method.

	// Returns true if the unit can cross the terrain, and outs the associated % cost if so.
	public bool GetMoveCost(TerrainType type, out float cost){
		if(schemeDict.ContainsKey(type)){
			cost = schemeDict[type].moveCost;
			return true;
		}
		cost = -1f;
		return false;
	} // End of GetMoveCost().

} // End of UnitMovementScheme.
