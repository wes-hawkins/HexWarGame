using UnityEngine;
using System.Collections.Generic;

public class HexTile {

	public Vector2Int GridPos2 { get; private set; }
	public int x { get { return GridPos2.x; } }
	public int y { get { return GridPos2.y; } }
	public int z { get; private set; }
	public Vector3Int GridPos3 { get { return new Vector3Int(GridPos2.x, GridPos2.y, z); } }

	public Vector3 WorldPos { get { return HexMath.HexGridToWorld(GridPos2); } }

	public bool Navigable { get; private set; }
	public float moveCost = 1f;

	public Unit occupyingUnit { get; private set; } = null;
	public void SetOccupyingUnit(Unit unit){ occupyingUnit = unit; }

	public TerrainType TerrainType { get; private set; }
	public void SetTerrainType(TerrainType newType){
		TerrainType = newType;
	} // End of SetTerrainType().


	public HexTile(Vector2Int pos){
		GridPos2 = pos;
		z = pos.x + pos.y;
		Navigable = Random.Range(0f, 1f) > 0.4f;
		//Navigable = true;

		//TerrainType = (TerrainType)Random.Range(0, System.Enum.GetValues(typeof(TerrainType)).Length);

		int distFromCenter = HexMath.VancouverDist(Vector2Int.zero, GridPos2);
		int numTerrainTypes = System.Enum.GetValues(typeof(TerrainType)).Length - 1;
		TerrainType = (TerrainType)Mathf.Clamp(numTerrainTypes - (distFromCenter / 3) + ((Random.Range(0f, 1f) < 0.2f)? Random.Range(-1, 1) : 0), 0, numTerrainTypes);

	} // End of constructor.


	public HexTile[] GetAdjacentTiles(){
		List<HexTile> neighbors = new List<HexTile>();
		for(int i = 0; i < 6; i++){
			HexTile neighbor = World.GetTile(GridPos2 + HexMath.DirectionToHexVec(i));
			if(neighbor != null)
				neighbors.Add(neighbor);
		}
		return neighbors.ToArray();
	} // End of GetNeighbors() method.



	private float mountainPerlinRate = 5f;

	private float mountainPeakHeight = 0.4f; // How high the center of the mountain tile 'peaks'.
	private float mountainRolloffDist = 0.5f; // Distance over which we roll off the bulge.

	// Returns height (in world space y, not actual normalized heighmap)
	public float GetHeightmap(Vector3 worldPos){
		float distFromCenter = Vector2.Distance(new Vector2(worldPos.x, worldPos.z), new Vector2(WorldPos.x, WorldPos.z));

		// Apply influence of each tile.
		float targetHeight = 0f;
		switch(TerrainType){
			case TerrainType.oceanFloor: 
				targetHeight = TerrainConfig.OceanDepth;
				break;
			case TerrainType.shallowWater: 
				targetHeight = TerrainConfig.ShallowsDepth; 
				break;
			//case TerrainType.beach: 
				//targetHeight = TerrainConfig.BeachDepth; 
				//break;
			case TerrainType.openGround: 
				targetHeight = 0f; 
				break;
			//case TerrainType.lowHills: 
				//targetHeight = 0.5f + (0.15f * Mathf.PerlinNoise(worldPos.x * lowHillsPerlinRate, worldPos.z * lowHillsPerlinRate)); 
				//break;
			case TerrainType.mountains: 
				float peakLerp = 1f - Mathf.Clamp01(distFromCenter / mountainRolloffDist);
				targetHeight = (peakLerp * mountainPeakHeight); // Center peak
				targetHeight += (TerrainConfig.MountainHeight - mountainPeakHeight) * Mathf.PerlinNoise((worldPos.x + 1000f) * mountainPerlinRate, (worldPos.z + 1000f) * mountainPerlinRate); // Perlin noise
				break;
		}
		return targetHeight;
	} // End of GetHeightmapForTerrainType() method..


	public float[] GetAlphamaps(Vector3 worldPos){
		float[] alphaMaps = new float[5];
		// 0 = Grass
		// 1 = Mountain
		// 2 = Snowcap
		// 3 = Beach Sand
		// 4 = Shallows

		switch(TerrainType){
			case TerrainType.openGround:
				alphaMaps[0] = 1f;
				break;
			case TerrainType.shallowWater:
				alphaMaps[4] = 1f;
				break;
			//case TerrainType.beach:
				//alphaMaps[3] = 1f;
				//break;
			case TerrainType.mountains:
				// TODO: Make snow based on actual heightmap, post-cell-blending.
				float snow = Mathf.Clamp01(Mathf.InverseLerp(TerrainConfig.SnowcapHeight - TerrainConfig.SnowcapBlend, TerrainConfig.SnowcapHeight, GetHeightmap(worldPos)));
				alphaMaps[1] = 1f - snow;
				alphaMaps[2] = snow;
				break;
		}

		return alphaMaps;
	} // End of GetAlphamap() method.

} // End of HexTile class.


public enum TerrainType {
	oceanFloor,
	shallowWater,
	//beach,
	openGround,
	//lowHills,
	mountains,
} // End of TerrainType enum.