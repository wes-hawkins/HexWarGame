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

	[SerializeField] private GameObject hexTilePrefab = null;
	private static Dictionary<HexTile, GameObject> visualizations = new Dictionary<HexTile, GameObject>();
	public static GameObject GetVisualization(HexTile tile){
		return visualizations[tile];
	} // End of GetVisualization().

	private bool showAxes = true;

	private HexTile findPath_startHex = null;
	private HexTile findPath_goalHex = null;
	private HexTile[] findPath_thePath = null;
	private bool findPath_new = false;

	[SerializeField] private MeshFilter physicalGroundGridMesh = null;
	[SerializeField] private MeshFilter virtualGroundGridMesh = null;



	private void Awake(){
		Inst = this;
	} // End of Awake() method.


	public void Init(){
		// Clear old data in case of reset.
		findPath_startHex = null;
		findPath_goalHex = null;
		findPath_thePath = new HexTile[0];
		findPath_new = false;
	
		// Generate maps
		// Temporary container to fill with tiles... will be cropped into allTiles.
		HexTile[] potentialallTiles = new HexTile[HexMath.VancouverArea(mapRadius)];
		int hexTileCount = 0;
		for(int x = -mapRadius; x <= mapRadius; x++){
			for(int y = -mapRadius; y <= mapRadius; y++){
				if(HexMath.VancouverDist(Vector2Int.zero, new Vector2Int( x, y )) <= mapRadius){
					HexTile newHex = new HexTile(new Vector2Int(x, y));
				
					// Cache new hex for straight hex list.
					potentialallTiles[hexTileCount] = newHex;
					hexTileCount++;
				
					// Add new hex to hex map.
					tileMap.Add(new Vector2Int(x, y), newHex);
				}
			}
		}
	
		allTiles = new HexTile[hexTileCount];
		for(int i = 0; i < hexTileCount; i++)
			allTiles[i] = potentialallTiles[i];

		HexMath.GetGrid(physicalGroundGridMesh.mesh, mapRadius, GUIConfig.GridOutlineThickness);
		virtualGroundGridMesh.mesh = physicalGroundGridMesh.mesh;

		RebuildTerrain();
	} // End of Start().


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
		terr.transform.position = new Vector3(-terr.terrainData.size.x / 2f, TerrainConfig.OceanFloor, -terr.terrainData.size.z / 2f);
		terr.terrainData.size = new Vector3(50f, TerrainConfig.MountainHeight - TerrainConfig.OceanFloor, 50f);

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

		// TODO: Separate heightmap and alphamap passes, such that alphamap calcs can source from final heightmap?

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


	private class HexTilePathingData {
		public HexTile Tile { get; private set; }
		public bool isOpen = false;
		public bool isClosed = false;
		public float gScore = 0f; // Total cost to move along the path up to this cell.
		public float fScore = 0f;
		public HexTile parentHex = null; // The tile we came from (in the pathfinding).

		public HexTilePathingData(HexTile tile){
			Tile = tile;
		} // End of constructor.

	} // End of HexTilePathingData class.


	// TODO: Limit search by maximum number of tiles the unit can move. Only sample the hexes within
	//   the Vancouver distance to the startTile. Should still be able to go after a goalTile that is
	//   outside of the possible distance, but it will 'stop short.'
	public static Vector2Int[] FindPath(HexTile startTile, HexTile goalTile, Unit unit, CancellationToken ct){
		// Create pathfinding data for all tiles.
		Dictionary<HexTile, HexTilePathingData> data = new Dictionary<HexTile, HexTilePathingData>();
		foreach(HexTile tile in allTiles)
			data.Add(tile, new HexTilePathingData(tile));
	
		// Seed first tile.
		data[startTile].isOpen = true;
		bool openHexExists = true;
		while(openHexExists && !ct.IsCancellationRequested){
			// Search for node on Open that has best estimate.
			float bestFScore = float.MaxValue;
			HexTile currentTile = null;
		
			openHexExists = false;
			foreach(HexTile tile in allTiles){
				if(data[tile].isOpen){
					openHexExists = true;
					if(data[tile].fScore < bestFScore){
						currentTile = tile;
						bestFScore = data[tile].fScore;
					}
				}
			}

			if(openHexExists){
				// Move our current node to Closed list
				data[currentTile].isOpen = false;
				data[currentTile].isClosed = true;
			
				// Test all neighboring nodes that can be reached from there
				HexTile[] adjacentTiles = currentTile.GetAdjacentTiles();
				foreach(HexTile adjacentTile in adjacentTiles){
					// Get neighbors
					if(adjacentTile.GetIsNavigable(unit.mapLayer, unit)){
						// The heuristic 'coaxes' the result a certain way. Allows algorithm to choose between multiple
						//   'optimal' paths, e.g. the 'straightest path' (even if a diagonal path is just as efficient.)
						//   Make sure the heuristic is much smaller than the value of a whole cell.
						float heuristic = 0f;
						if(!data[adjacentTile].isClosed && !data[adjacentTile].isOpen){
							data[adjacentTile].isOpen = true;
							data[adjacentTile].parentHex = currentTile;
						
							data[adjacentTile].gScore = data[currentTile].gScore + unit.GetMoveCost(adjacentTile);
							//heuristic = HexMath.CrowDist(adjacentTile.GridPos2, goalTile.GridPos2);
							data[adjacentTile].fScore = data[adjacentTile].gScore + heuristic;
						}else if(data[adjacentTile].isOpen && (data[adjacentTile].gScore < data[currentTile].gScore)){
							data[adjacentTile].parentHex = currentTile;
						
							data[adjacentTile].gScore = data[currentTile].gScore + unit.GetMoveCost(adjacentTile);
							//heuristic = HexMath.CrowDist(adjacentTile.GridPos2, goalTile.GridPos2);
							data[adjacentTile].fScore = data[adjacentTile].gScore + heuristic;
						}
					}
				}
	
				// If node is goal, break with success
				if(currentTile == goalTile){
					HexTile currentTraceHex = goalTile;
					List<Vector2Int> path = new List<Vector2Int>();
					while(currentTraceHex != startTile){
						path.Add(currentTraceHex.GridPos2);
						currentTraceHex = data[currentTraceHex].parentHex;
					}
					path.Add(startTile.GridPos2);

					path.Reverse();
					return path.ToArray();
				}
			}
		}
		
		// Not able to create a path!
		return null;
	} // End of FindPath().






	/*
	static function GetTile( tile : Vector2 ) : HexTile
	{
		if(( tile.x >= -mapSize ) && ( tile.x <= mapSize ) && ( tile.y >= -mapSize ) && ( tile.y <= mapSize ))
			return tileMap[ tile.x + mapSize, tile.y + mapSize ];
		else
			return null;
	}

	static function GetAdjacent( hex : HexTile ) : HexTile[]
	{
		var potentialAdjacents = new HexTile[6];
		var numAdjacent : int;
	
		for( var i = 0; i < 6; i++ )
		{
			var adjacentCoord = HexMath.AdjInDir( hex.pos2, i );
			var adjacent = GetTile( adjacentCoord );
			if( adjacent )
			{
				potentialAdjacents[ numAdjacent ] = adjacent;
				numAdjacent++;
			}
		}

		var adjacentTilees = new HexTile[ numAdjacent ];
		for( var j = 0; j < numAdjacent; j++ )
			adjacentTilees[j] = potentialAdjacents[j];
	
		return adjacentTilees;
	}

	static function FindPath( startHex : HexTile, goalHex : HexTile, myTeam : int ) : HexTile[]
	{
		findPath_new = false;
		// Barrista
		if(( startHex == findPath_startHex ) && ( findPath_goalHex == goalHex ))
			return findPath_thePath;
	
		// print( "FindPath" );

		// Clear open and closed lists
		for( var anyHex : HexTile in allTiles )
		{
			anyHex.isOpen = false;
			anyHex.isClosed = false;
			anyHex.gScore = 0;
			anyHex.fScore = 0;
		}
	
		startHex.isOpen = true;
		var openHexExists = true;
	
		while( openHexExists )
		{
			// Search for node on Open that has best estimate.
			var bestFScore : float = 999999;
			var currentTile : HexTile;
		
			openHexExists = false;
			for( var aHex : HexTile in allTiles )
			{
				if( aHex.isOpen )
				{
					openHexExists = true;
					if( aHex.fScore < bestFScore )
					{
						currentTile = aHex;
						bestFScore = aHex.fScore;
					}
				}
			}

			if( openHexExists )
			{
				// Move that node to Closed list
				currentTile.isOpen = false;
				currentTile.isClosed = true;
			
				// Test all neighboring nodes that can be reached from there
				var adjacentTiles = GetAdjacent( currentTile );
				for( var adjacentTile : HexTile in adjacentTiles )
				{
					// Get neighbors
					if(( HexMath.VancouverDist( currentTile.pos2, adjacentTile.pos2 ) == 1 )
					   && ( Mathf.Abs( currentTile.h - adjacentTile.h ) <= 1 )
					   && ( !adjacentTile.occupyingUnit || ( adjacentTile.occupyingUnit.alliance == myTeam )))
					{
						var heuristic : float;
						if( !adjacentTile.isClosed && !adjacentTile.isOpen )
						{
							adjacentTile.isOpen = true;
							adjacentTile.parentHex = currentTile;
						
							adjacentTile.gScore = currentTile.gScore + adjacentTile.moveCost;
							heuristic = HexMath.CrowDist( adjacentTile.pos2, goalHex.pos2 );
							adjacentTile.fScore = adjacentTile.gScore + heuristic;
						}
						else if( adjacentTile.isOpen && ( adjacentTile.gScore < currentTile.gScore ))
						{
							adjacentTile.parentHex = currentTile;
						
							adjacentTile.gScore = currentTile.gScore + adjacentTile.moveCost;
							heuristic = HexMath.CrowDist( adjacentTile.pos2, goalHex.pos2 );
							adjacentTile.fScore = adjacentTile.gScore + heuristic;
						}
					}
				}
	
				// If node is goal, break with success
				if( currentTile == goalHex )
				{
					var currentTraceHex = goalHex;
					var pathLength : int;
				
					var pathHolder : HexTile[];
					pathHolder = new HexTile[ allTiles.length ];
				
					while( currentTraceHex != startHex )
					{
						pathLength++;
						pathHolder[ pathLength ] = currentTraceHex;
						currentTraceHex = currentTraceHex.parentHex;
					}
				
					// Build path to return
					var thePath : HexTile[];
					thePath = new HexTile[ pathLength ];
					for( var i = 0; i < pathLength; i++ )
						thePath[ i ] = pathHolder[ pathLength - i ];
				
					findPath_startHex = startHex;
					findPath_goalHex = goalHex;
					findPath_thePath = thePath;
					findPath_new = true;
					return thePath;
				}
			}
		}
	
		findPath_startHex = startHex;
		findPath_goalHex = goalHex;
		findPath_thePath = null;
		findPath_new = true;
		return null;
	}


	static function FindRange( startHex : HexTile, range : int ) : Vector2[]
	{
		findRange_new = false;
		// Barrista
		if(( findRange_startHex == startHex ) && ( findRange_range == range ))
		{
			tileRange = findRange_tileRange;
			return findRange_tilesInRange;
		}

		// Reset range tile values...
		tileRange = new float[ ( mapSize * 2 ) + 1, ( mapSize * 2 ) + 1 ];

		var tilesInRange = HexMath.GetVancouverSquare( startHex.pos2, range );
		for( var i = 0; i < tilesInRange.length; i++ )
		{
			if( InWorldMapRange( tilesInRange[i] + ( Vector2.one * mapSize )))
				tileRange[ tilesInRange[i].x + mapSize,
						   tilesInRange[i].y + mapSize ] = HexMath.VancouverDist( startHex.pos2, tilesInRange[i] );
		}

		findRange_startHex = startHex;
		findRange_range = range;
		findRange_tilesInRange = tilesInRange;
		findRange_new = true;
		findRange_tileRange = tileRange;
		return tilesInRange;
	}





	static var visHit : boolean[,];

	// Finds all tiles to the center of which a straight line can be drawn from 'start.'
	// Excludes tiles that are 'cut off' from 'start' by non-visible tiles.
	static function FindTilesInLOS( start : Vector2, range : int ) : Vector2[]
	{
		findTilesInLOS_new = false;
		// Barrista
		if(( start == findTilesInLOS_start ) && ( findTilesInLOS_range == range ))
		{
			tileRange = findTilesInLOS_tileRange;
			return findTilesInLOS_tiles;
		}
	
		// This array tells if a tile was intersected by any single thereto-successful visibility test ray.
		visHit = new boolean[ ( World.mapSize * 2 ) + 1, ( World.mapSize * 2 ) + 1 ];

		var potential = new Vector2[ HexMath.VancouverArea( range )];
		var num : int;
	
		// Confirmed visible tiles.
		var testHexes = HexMath.GetVancouverSquare( start, range );
	
		// Initialize float tiles... previous tile must be in FOV for a tile to be in FOV.
		visibility = new float[ ( mapSize * 2 ) + 1, ( mapSize * 2 ) + 1 ];
		visibility[ start.x + mapSize, start.y + mapSize ] = 1.0;
	
		// Reset range tile values...
		tileRange = new float[ ( mapSize * 2 ) + 1, ( mapSize * 2 ) + 1 ];
	
		for( var r = range; r > 0; r-- )
		{
			var currentRing = HexMath.GetVancouverRing( start, r );
			for( var i = 0; i < currentRing.length; i++ )
			{
				var currentTile = currentRing[i];
		
				// If the tile is not visible...
				var worldMapCoord = currentTile + ( Vector2.one * World.mapSize );
				if( InWorldMapRange( worldMapCoord ))
				{
					// var sampleVerts = HexMath.ConcaveVertices( start, currentTile);
					// var sampleVerts = [ -1 ];
					var sampleVerts = [ -1, 0, 1, 2, 3, 4, 5 ];
				
					var numVisibleVerts : int;
					for( var j = 0; j < sampleVerts.length; j++ )
					{
						if( HexMath.TestFOVToTile( start, currentTile, sampleVerts[j]))
							numVisibleVerts++;
					}
				
					var tileVisibility : float = ( numVisibleVerts * 1.0 ) / ( sampleVerts.length * 1.0 );
				
					if(( tileVisibility == 0 ) && visHit[ worldMapCoord.x, worldMapCoord.y ])
						tileVisibility = 1.0 / ( sampleVerts.Length * 1.0 );
				
					visibility[ worldMapCoord.x, worldMapCoord.y ] = tileVisibility;
					tileRange[ worldMapCoord.x, worldMapCoord.y ] = HexMath.VancouverDist( start, currentTile );
				
					if( tileVisibility > 0.0 )
					{
						potential[ num ] = currentTile;
						num++;
					}
				}
			}
		}


	
		var tiles = new Vector2[ num ];
		for( var k = 0; k < tiles.length; k++ )
			tiles[k] = potential[k];
	
		findTilesInLOS_start = start;
		findTilesInLOS_range = range;
		findTilesInLOS_tiles = tiles;
		findTilesInLOS_new = true;
		findTilesInLOS_tileRange = tileRange;
		return tiles;
	}

	static function InWorldMapRange( worldMapCoord : Vector2 ) : boolean
	{
		return ((( worldMapCoord.x >= 0 ) && ( worldMapCoord.y >= 0 ) &&
				( worldMapCoord.x < (( World.mapSize * 2 ) + 1 )) && ( worldMapCoord.y < (( World.mapSize * 2 ) + 1 ))));
	}
	*/

} // End of World class.