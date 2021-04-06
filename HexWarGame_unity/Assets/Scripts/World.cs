using UnityEngine;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;


public class World : MonoBehaviour {

	public static World Inst { get; private set; }

	public static int mapRadius { get; private set; } = 20;

	public static HexTile[] allTiles { get; private set; } // Straight list of all hexes in the world.
	private static Dictionary<Vector2Int, HexTile> tileMap = new Dictionary<Vector2Int, HexTile>(); // 2D map of all hexes.
	public static HexTile GetTile(Vector2Int pos){
		if(tileMap.ContainsKey(pos))
			return tileMap[pos];
		return null;
	} // End of GetTile().

	[SerializeField] private MeshFilter physicalGroundGridMesh = null;
	[SerializeField] private MeshFilter virtualGroundGridMesh = null;
	[Space]
	[SerializeField] private Transform waterPlane = null;
	[SerializeField] private MeshFilter gridBrimMesh = null;




	private void Awake(){
		Inst = this;
	} // End of Awake() method.


	public void LoadMap(SerializableTileInfo[] map){
		tileMap.Clear();
		mapRadius = HexMath.VancouverAreaToRadius(map.Length);
		if(mapRadius == -1){
			Debug.LogError("Map has invalid area!");
			return;
		}
		Vector2Int[] vancSquare = HexMath.GetVancouverSquare(Vector2Int.zero, World.mapRadius);
		allTiles = new HexTile[map.Length];
		for(int i = 0; i < map.Length; i++){
			HexTile newTile = new HexTile(vancSquare[i]);
			newTile.SetTerrainType(map[i].TerrainType, false);
			allTiles[i] = newTile;
			tileMap.Add(vancSquare[i], newTile);
		}
		RebuildTerrain();
		RebuildGridAndBrim();
	} // End of LoadMap().


	public void NewMap(TerrainType terrainType, int radius){
		tileMap.Clear();
		mapRadius = radius;
		Vector2Int[] vancSquare = HexMath.GetVancouverSquare(Vector2Int.zero, World.mapRadius);
		allTiles = new HexTile[vancSquare.Length];
		for(int i = 0; i < vancSquare.Length; i++){
			HexTile newTile = new HexTile(vancSquare[i]);
			newTile.SetTerrainType(terrainType, false);
			allTiles[i] = newTile;
			tileMap.Add(vancSquare[i], newTile);
		}
		RebuildTerrain();
		RebuildGridAndBrim();
	} // End of NewMap() method.


	private void RebuildGridAndBrim(){
		HexMath.GetGrid(physicalGroundGridMesh.mesh, mapRadius, GUIConfig.GridOutlineThickness);
		virtualGroundGridMesh.mesh = physicalGroundGridMesh.mesh;

		HexMath.GetMapBrim(gridBrimMesh.mesh, mapRadius);
	} // End of RebuildGridAndBrim() method.


	// Rebuilds the entire terrain.
	public void RebuildTerrain(){
		RebuildTerrain(Vector2.zero, Terrain.activeTerrain.terrainData.size.ToMap2D());
	}
	// Rebuilds a single hex.
	public void RebuildTerrain(HexTile hexTile){
		RebuildTerrain(HexMath.HexGridToWorld(hexTile.GridPos2).ToMap2D(), Vector2.one * 1.5f);
	}
	// Rebuilds a path of terrain.
	public void RebuildTerrain(Vector2 center, Vector2 size){

		// The terrain sits at the ocean depth height, and raises up to create terrain.
		Terrain terr = Terrain.activeTerrain;
		//float terrainSize = mapRadius * 2f;
		float terrainSize = (mapRadius + 1f) * 2f;
		waterPlane.localScale = new Vector3(terrainSize, terrainSize, 1f);
		terr.terrainData.size = new Vector3(terrainSize, TerrainConfig.MountainHeight - TerrainConfig.OceanFloor, terrainSize);
		terr.transform.position = new Vector3(-terr.terrainData.size.x / 2f, TerrainConfig.OceanFloor, -terr.terrainData.size.z / 2f);

		//float terrainResolution = terr.terrainData.heightmapResolution / terr.terrainData.size.x; // heightmap-units per world-unit
		Vector2 centerTerrainLocalPos = center - terr.transform.position.ToMap2D(); // center position relative to heightmap
		Vector2 centerNormalized = centerTerrainLocalPos / terr.terrainData.size.x;

		// Now in heightmap units
		Vector2 patchCenter = new Vector2(centerNormalized.x * terr.terrainData.heightmapResolution, centerNormalized.y * terr.terrainData.heightmapResolution);
		Vector2 patchSize = new Vector2(size.x / terr.terrainData.size.x, size.y / terr.terrainData.size.z) * terr.terrainData.heightmapResolution;

		Vector2Int patchStartCorner = new Vector2Int(
			Mathf.Clamp(Mathf.RoundToInt(patchCenter.x - (patchSize.x / 2f)), 0, terr.terrainData.heightmapResolution),
			Mathf.Clamp(Mathf.RoundToInt(patchCenter.y - (patchSize.y / 2f)), 0, terr.terrainData.heightmapResolution)
		);
		Vector2Int patchEndCorner = new Vector2Int(
			Mathf.Clamp(Mathf.RoundToInt(patchCenter.x + (patchSize.x / 2f)), 0, terr.terrainData.heightmapResolution),
			Mathf.Clamp(Mathf.RoundToInt(patchCenter.y + (patchSize.y / 2f)), 0, terr.terrainData.heightmapResolution)
		);

		Vector2Int patchRealSize = new Vector2Int(patchEndCorner.x - patchStartCorner.x, patchEndCorner.y - patchStartCorner.y);


		// We 'jiggle' the sampling position a bit to make things a little less uniform.
		float influencePerlinRate = 1f; // how rapid the jiggle is.
		float influencePerlinIntensity = 0.2f; // how intense the jiggle is.

		// TODO: Address slight alphamap offset (vs. heightmap)

		float[,] heights = new float[patchRealSize.y, patchRealSize.x];

		// Alphamap is one unit shy of the heightmap, so strip an index off if we have to.
		bool stripAlphaX = patchEndCorner.x > terr.terrainData.alphamapResolution;
		bool stripAlphaY = patchEndCorner.y > terr.terrainData.alphamapResolution;
		float[,,] alphaMap = new float[patchRealSize.y - (stripAlphaY? 1 : 0), patchRealSize.x - (stripAlphaX? 1 : 0), 5];

		for (int x = patchStartCorner.x; x < patchEndCorner.x; x++){
			for (int y = patchStartCorner.y; y < patchEndCorner.y; y++){

				Vector3 worldPos = new Vector3(
					terr.transform.position.x + terr.terrainData.size.x * ((float)x / terr.terrainData.heightmapResolution),
					0f,
					terr.transform.position.z + terr.terrainData.size.z * ((float)y / terr.terrainData.heightmapResolution)
				);

				Vector2 samplePerlinOffset = influencePerlinIntensity * (new Vector2(-0.5f, -0.5f) + new Vector2(
					Mathf.PerlinNoise(worldPos.x * influencePerlinRate, worldPos.z * influencePerlinRate),
					Mathf.PerlinNoise((worldPos.x + 1000f) * influencePerlinRate, (worldPos.z + 1000f) * influencePerlinRate)
				));

				//Vector3 perlinSampledWorldPos = worldPos + samplePerlinOffset.MapTo3D();
				Vector3 perlinSampledWorldPos = worldPos;


				// The grid tile this point lies on.
				Vector2Int myCell = HexMath.WorldToHexGrid(perlinSampledWorldPos);
				Vector3 myTileWorldPos = HexMath.HexGridToWorld(myCell);

				float targetHeight = 0f;
				float[] targetAlphamaps = new float[5];

				// If we aren't on a tile, zero it out.
				HexTile myTile = GetTile(myCell);
				if(myTile == null){
					targetHeight = TerrainConfig.OceanFloor;

				// Use this block for just raw tiles without blending.
				/*} else {
					targetHeight = myTile.GetHeightmap(worldPos);
					targetAlphamaps = myTile.GetAlphamaps(worldPos);
				}*/

				// If we are on a tile, build influence map from neighbors...
				} else {
					targetHeight += myTile.GetHeightmap(worldPos);
					targetAlphamaps = myTile.GetAlphamaps(worldPos);

					// Get surrounding tiles.
					float totalHeightmapInfluence = 1f;
					float totalAlphamapInfluence = 1f;
					for(int d = 0; d < 6; d++){
						Vector2Int nearbyCell = HexMath.GetAdjacentCell(myCell, d);

						Vector3 nearbyCellWorldPos = HexMath.HexGridToWorld(nearbyCell);
						HexTile nearbyTile = GetTile(nearbyCell);
						if(nearbyTile != null){

							Vector2 leftVertex = (myTileWorldPos + HexMath.GetVertex(d)).ToMap2D();
							Vector2 rightVertex = (myTileWorldPos + HexMath.GetVertex(d + 1)).ToMap2D();
							float distToEdge = Utilities.FindDistanceToSegment(perlinSampledWorldPos.ToMap2D(), leftVertex, rightVertex);
							float influenceRange = 0.2f;
							float influence = 1f - Mathf.Clamp01(distToEdge / influenceRange);

							influence = 0.5f - (0.5f * Mathf.Cos(Mathf.PI * influence));

							totalHeightmapInfluence += influence;
							targetHeight += influence * nearbyTile.GetHeightmap(worldPos);

							
							float alphaInfluence = influence;
							if(nearbyTile.TerrainType == TerrainType.shallowWater)
								alphaInfluence *= 5f;

							totalAlphamapInfluence += alphaInfluence;
							float[] tileAlphamaps = nearbyTile.GetAlphamaps(worldPos);
							for(int i = 0; i < tileAlphamaps.Length; i++)
								targetAlphamaps[i] += tileAlphamaps[i] * alphaInfluence;
						}
					}
					targetHeight /= totalHeightmapInfluence;

					for(int i = 0; i < targetAlphamaps.Length; i++)
						targetAlphamaps[i] /= totalAlphamapInfluence;
				}
				
				heights[y - patchStartCorner.y, x - patchStartCorner.x] = Mathf.InverseLerp(TerrainConfig.OceanFloor, TerrainConfig.MountainHeight, targetHeight);

				// Alphamap
				if(x < terr.terrainData.alphamapResolution && y < terr.terrainData.alphamapResolution){

					for(int i = 0; i < targetAlphamaps.Length; i++){
						alphaMap[y - patchStartCorner.y, x - patchStartCorner.x, i] = targetAlphamaps[i];
					}
				}
			}
		}

		// Set the new heightmap values
		terr.terrainData.SetHeights(patchStartCorner.x, patchStartCorner.y, heights);
		terr.terrainData.SetAlphamaps(patchStartCorner.x, patchStartCorner.y, alphaMap);
		//terr.heightmapMaximumLOD = 0; // Effectively prevents terrain from showing lower LODs.

	} // End of RebuildTerrain().


} // End of World class.