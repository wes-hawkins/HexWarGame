using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "WarGame/TileBlendingMap")]
[System.Serializable]
public class TileBlendingMap : SingletonScriptableObject<TileBlendingMap> {

	[System.Serializable]
	public class TileBlendingData {

		[SerializeField] private TerrainType fromType = TerrainType.openGround; public TerrainType FromType { get { return fromType; } }
		
		[SerializeField] private AnimationCurve heightmapBlend = new AnimationCurve(); public AnimationCurve HeightmapBlend { get { return heightmapBlend; } }
		[SerializeField] private AnimationCurve alphamapBlend = new AnimationCurve(); public AnimationCurve AlphamapBlend { get { return alphamapBlend; } }

	} // End of CellBlendingDefinition class.

	[SerializeField] private TileBlendingData[] blendDefinitions = new TileBlendingData[0];

	private Dictionary<TerrainType, TileBlendingData> blendMap = new Dictionary<TerrainType, TileBlendingData>();


	public override void Init(){
		base.Init();

		// Create map
		foreach(TileBlendingData data in blendDefinitions)
			blendMap.Add(data.FromType, data);
	} // End of Init().


	public static float GetHeightmapBlend(TerrainType fromType, float t){
		return Inst.blendMap[fromType].HeightmapBlend.Evaluate(t);
	} // End of GetHeightmapBlend() method.


	public static float GetAlphamapBlend(TerrainType fromType, float t){
		return Inst.blendMap[fromType].AlphamapBlend.Evaluate(t);
	} // End of GetAlphamapBlend() method.

} // End of TileBlendingMap class.
