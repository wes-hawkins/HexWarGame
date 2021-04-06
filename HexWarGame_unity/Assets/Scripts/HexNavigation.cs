using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;


public static class HexNavigation {

	// TODO: handle 'frontier' and 'explored' tiles as individual lists to be managed, instead of looping through
	//   all tiles every loop.
	


	// If the heuristic always = 0, this is Dijkstra's Algorithm. If heuristic changes, it's A*.
	public static HexPath FindPath(HexTile startTile, HexTile goalTile, Unit unit, CancellationToken ct){
		// Immediately check for issues with the goal tile.
		if(!unit.Definition.MoveScheme.GetCanNavigate(goalTile.TerrainType) || goalTile.occupyingUnit)
			return null;

		DijkstraSetup(startTile, out Dictionary<HexTile, TileMetadata> data);
		bool openHexExists = true;
		while(openHexExists && !ct.IsCancellationRequested){
			DijkstraCurrentTileSearch(ref openHexExists, ref data, out HexTile currentTile);
			if(openHexExists){
				DijkstraStep(unit, currentTile, ref data);
	
				// If node is goal, we're done!
				if(currentTile == goalTile){
					// 'Breadcrumb' our way back to the start tile.
					HexTile currentTraceHex = goalTile;
					List<HexTile> path = new List<HexTile>();
					while(currentTraceHex != startTile){
						path.Add(currentTraceHex);
						currentTraceHex = data[currentTraceHex].parentTile;
					}
					path.Add(startTile);
					path.Reverse();
					return new HexPath(unit, path.ToArray());
				}
			}
		}
		
		return null; // Not able to create a path!
	} // End of FindPath().



	// Finds all tiles this unit could move to this turn. 'invalidDestinations' are the tiles that can be passed through,
	//   but not landed on.
	public static Vector2Int[] FindValidMoves(HexTile startTile, Unit unit, out Vector2Int[] passThroughOnly, CancellationToken ct){
		DijkstraSetup(startTile, out Dictionary<HexTile, TileMetadata> data);
		bool openHexExists = true;
		while(openHexExists && !ct.IsCancellationRequested){
			DijkstraCurrentTileSearch(ref openHexExists, ref data, out HexTile currentTile);
			if(openHexExists)
				DijkstraStep(unit, currentTile, ref data);
		}

		// Build valid movement tiles.
		List<HexTile> exploredTiles = new List<HexTile>(); // We'll store all found tiles in here.
		List<HexTile> passThroughOnlyTiles = new List<HexTile>(); // We'll store tiles we can pass through, but not land on, here.
		foreach(KeyValuePair<HexTile, TileMetadata> kvp in data){
			if(kvp.Value.isExplored && (kvp.Key != startTile)){
				if(kvp.Key.occupyingUnit) // If a friendly unit is in the tile, it's a 'pass through' tile; we can't land on it.
					passThroughOnlyTiles.Add(kvp.Key);
				else
					exploredTiles.Add(kvp.Key);
			}
		}

		Vector2Int[] returnTiles = new Vector2Int[exploredTiles.Count];
		for(int i = 0; i < exploredTiles.Count; i++)
			returnTiles[i] = exploredTiles[i].GridPos2;

		passThroughOnly = new Vector2Int[passThroughOnlyTiles.Count];
		for(int i = 0; i < passThroughOnlyTiles.Count; i++)
			passThroughOnly[i] = passThroughOnlyTiles[i].GridPos2;
		
		return returnTiles;
	} // End of FindValidMoves().



	// Sets up the initial conditions for a Dijkstra engine.
	private static void DijkstraSetup(HexTile startTile, out Dictionary<HexTile, TileMetadata> data){
		// Create pathfinding data for all tiles.
		data = new Dictionary<HexTile, TileMetadata>();
		foreach(HexTile tile in World.allTiles)
			data.Add(tile, new TileMetadata());
	
		// Seed first tile.
		data[startTile].isFrontier = true;
	} // End of DijkstraSetup() method.



	// Finds the best frontier tile to test next in a Dijkstra engine.
	private static void DijkstraCurrentTileSearch(ref bool openHexExists, ref Dictionary<HexTile, TileMetadata> data, out HexTile currentTile){
		// Search for node on Open that has best estimate.
		float bestFScore = float.MaxValue;
		currentTile = null;
		openHexExists = false;
		foreach(HexTile tile in World.allTiles){
			if(data[tile].isFrontier){
				openHexExists = true;
				if(data[tile].fScore < bestFScore){
					currentTile = tile;
					bestFScore = data[tile].fScore;
				}
			}
		}
	} // End of DijkstraCurrentTileSearch() method.



	// Core Dijkstra engine loop for pathfinding; computes neighbors for current tile.
	private static void DijkstraStep(Unit unit, HexTile currentTile, ref Dictionary<HexTile, TileMetadata> data){
		unit.Definition.MoveScheme.GetMoveCost(currentTile.TerrainType, out float currentTileMoveCost);

		// Move our current node to Closed list
		data[currentTile].isFrontier = false;
		data[currentTile].isExplored = true;
			
		// Test all neighboring nodes that can be reached from there
		HexTile[] adjacentTiles = currentTile.GetAdjacentTiles();
		foreach(HexTile adjacentTile in adjacentTiles){
			// Get neighbors
			float nextTileMoveCost;
			if(unit.Definition.MoveScheme.GetMoveCost(adjacentTile.TerrainType, out nextTileMoveCost)){
				// The cost to move between the current tile and the next one.
				float blendedMoveCost = (currentTileMoveCost + nextTileMoveCost) / 2f;

				// The heuristic 'coaxes' the result a certain way. Allows algorithm to choose between multiple
				//   'optimal' paths, e.g. the 'straightest path' (even if a diagonal path is just as efficient.)
				//   Make sure the heuristic is much smaller than the value of a whole cell.
				float heuristic = 0f;

				// New tile we're attempting to move into?
				if(!data[adjacentTile].isExplored && !data[adjacentTile].isFrontier){
					data[adjacentTile].gScore = data[currentTile].gScore + blendedMoveCost;
					//heuristic = HexMath.CrowDist(adjacentTile.GridPos2, goalTile.GridPos2) * 0.1f;
					data[adjacentTile].fScore = data[adjacentTile].gScore + heuristic;

					// If we have enough move power to get to this tile, add it to our frontier.
					if(data[adjacentTile].gScore <= unit.MovePower){
						data[adjacentTile].isFrontier = true;
						data[adjacentTile].parentTile = currentTile;
					}
				// Old tile we found a better path to?
				} else if(data[adjacentTile].isFrontier && (data[adjacentTile].gScore < data[currentTile].gScore)){
					data[adjacentTile].parentTile = currentTile;
						
					data[adjacentTile].gScore = data[currentTile].gScore + blendedMoveCost;
					//heuristic = HexMath.CrowDist(adjacentTile.GridPos2, goalTile.GridPos2) * 0.1f;
					data[adjacentTile].fScore = data[adjacentTile].gScore + heuristic;
				}
			}
		}
	} // End of PathfindingEngineStep() method.


	// Metadata attached to a single tile.
	private class TileMetadata {
		public bool isFrontier = false;
		public bool isExplored = false;
		public float gScore = 0f; // Total cost to move along the path up to this cell.
		public float fScore = 0f;
		public HexTile parentTile = null; // The tile we came from (in the pathfinding), used to retrace our steps.
	} // End of HexTilePathingData class.

} // End of HexNavigation class.


// A traversible path from 'start' to 'finish' for a unit, across a number of 'steps' between two tiles. Note that there
//   are one fewer steps than tiles, as a step is between two tiles.
public class HexPath {
	public Unit Unit { get; private set; }
	public HexTile[] Tiles { get; private set; }
	public Vector2Int[] Cells { get; private set; }


	public HexPath(Unit unit, HexTile[] tiles){
		Unit = unit;
		Tiles = tiles;
		Cells = new Vector2Int[tiles.Length];
		for(int i = 0; i < tiles.Length; i++)
			Cells[i] = tiles[i].GridPos2;
	} // End of constructor.


	// How much move power is needed to make the step.
	public float GetStepCost(int step){
		float currentTileMoveCost;
		float nextTileMoveCost;
		Unit.Definition.MoveScheme.GetMoveCost(Tiles[step].TerrainType, out currentTileMoveCost);
		Unit.Definition.MoveScheme.GetMoveCost(Tiles[step + 1].TerrainType, out nextTileMoveCost);
		return (currentTileMoveCost + nextTileMoveCost) / 2f;
	} // End of GetStepCost() method.


	// Interpolates over a step, spitting out a position and rotation for animating the unit.
	public void GetStepAnimation(int step, float t, out Vector3 position, out Quaternion rotation){
		// TODO: Step will be broken up into two halves; first tile and second tile.
		// TODO: Modulate 'rate' depending on 'length' of half-step? (Curves take longer to traverse than straights?)
		// ...

		// DEBUG: Simple boilerplate
		HexTile thisTile = Tiles[step];
		HexTile nextTile = Tiles[step + 1];

		position = Vector3.Lerp(thisTile.WorldPos, nextTile.WorldPos, t);
		rotation = Quaternion.Euler(0f, HexMath.DirAngle(HexMath.HexVecToDirection(nextTile.GridPos2 - thisTile.GridPos2)), 0f);

	} // End of GetStepAnimation() method.

} // End of HexPath class.