using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour
{

	private void Update() {

		/*
		// Line intersect test
		List<Vector2Int> tiles = new List<Vector2Int>();
		foreach(HexTile tile in World.allTiles){
			if(HexMath.LineIntersectsTile(Vector2.zero, InputManager.Inst.CursorMapPosition.ToMap2D(), tile.GridPos2)){
				tiles.Add(tile.GridPos2);
			}
		}
		HexMath.GetTilesFill(GetComponent<MeshFilter>().mesh, tiles.ToArray(), -0.2f);
		*/


		/*
		// Get next tile in direction
		int direction = HexMath.WorldVectorToHexDir(InputManager.Inst.CursorMapPosition.ToMap2D());
		Vector2Int nextHex = HexMath.GetAdjacentCell(Vector2Int.zero, direction);

		HexMath.GetTilesFill(GetComponent<MeshFilter>().mesh, new Vector2Int[]{ nextHex }, -0.2f);
		*/


		/*
		// 3 tiles in direction
		Debug.Log("Goin");
		List<Vector2Int> tiles = new List<Vector2Int>();
		int dirTowardsEnd = HexMath.WorldVectorToHexDir(InputManager.Inst.CursorMapPosition.ToMap2D() - HexMath.HexGridToWorld(Vector2Int.zero).ToMap2D());
		for(int d = -1; d < 2; d++){
			Debug.Log("     " + d);
			tiles.Add(HexMath.GetAdjacentCell(Vector2Int.zero, dirTowardsEnd + d));
		}
		HexMath.GetTilesFill(GetComponent<MeshFilter>().mesh, tiles.ToArray(), -0.2f);
		*/

	} // End of Update().



}
