using UnityEngine;
using System;
using System.Collections.Generic;

public class HexMath {

	// Directions:
	// 0 = north
	// 1 = northeast
	// 2 = southeast
	// 3 = south
	// 4 = southwest
	// 5 = northwest

	// Hexes aligned with points up/down along Z axis. Flat sides aligned with X axis.

	public const float cellHeight = 0.86605f; // 'height' in the Z direction... or z offset between two cells in adjacent rows.
	public const float sideL = 0.5774f;
	public const float circumcircleRadius = 0.5774f;
	public const float longDiagL = 1.1547f;
	public const float c = 0.28865f; // (longDiag - sideLength) / 2... or height of diagonal top or bottom triangle 'caps'
	public const float m = c * 0.5f; // slope of diagonal.

	public const float sqrtOf3 = 1.73205080757f;



	// Converts axial to cubic coordinates.
	public static Vector3Int AxialToCubic(Vector2Int tile){
		return new Vector3Int(tile.x, tile.y, tile.x + tile.y);
	} // End of AxialToCubic() method.


	// Converts cubic to axial coordinates.
	public static Vector2Int CubicToAxial(Vector3Int tile){
		return new Vector2Int(tile.x, tile.y);
	} // End of CubicToAxial() method.


	// Returns 'z' component of tile given 2d axial position.
	public static int GetZ(Vector2Int tile){
		return -tile.x - tile.y;
	} // End of GetZ() method.


	// Find nearest integer hex.
	public static Vector2Int Round(Vector3 pos){
		int rx = Mathf.RoundToInt(pos.x);
		int ry = Mathf.RoundToInt(pos.y);
		int rz = Mathf.RoundToInt(pos.z);

		var x_err = Mathf.Abs(rx - pos.x);
		var y_err = Mathf.Abs(ry - pos.y);
		var z_err = Mathf.Abs(rz - pos.z);

		if((x_err > y_err) && (x_err > z_err))
			rx = -ry - rz;
		else if(y_err > z_err)
			ry = -rx - rz;
		else
			rz = -rx - ry;

		return new Vector2Int(rx, rz);
	} // End of Round() method.


	// Returns the total number of hexes within a certain range of a hex (including that hex).
	// Note: a single hex has a "radius" of 0.
	public static int VancouverArea(int radius){
		int result = 1;
		for(int i = 0; i <= radius; i++)
			result += 6 * i;
		return result;
	} // End of VancouverArea() method.


	// Returns number of tiles in a ring with radius.
	public static int VancouverCircumference(int radius){
		return 6 * radius;
	} // End of VancouverCircumference() method.


	// Cheers to 'aaz' of stackoverflow.com
	public static int VancouverDist(Vector2Int start, Vector2Int end){
		return (Mathf.Abs(start.x - end.x) + Mathf.Abs(start.y - end.y) + Mathf.Abs(start.x + start.y - end.x - end.y)) / 2;
	} // End of VancouverDist() method.


	// Cheers to http://www.drking.org.uk/
	public static float CrowDist(Vector2Int start, Vector2Int end){
		return Mathf.Sqrt(Mathf.Pow(end.x - start.x, 2) + Mathf.Pow(end.y - start.y, 2) + ((end.x - start.x) * (end.y - start.y)));
	} // End of CrowDist() method.


	// Returns the coordinate of the adjacent hex in the direction.
	public static Vector2Int GetAdjacentCell(Vector2Int start, int inputDir){
		return start + DirectionToHexVec(inputDir);
	} // End of AdjInDir() method.


	// Returns unit vector in direction.
	public static Vector2Int DirectionToHexVec(int inputDir){
		int dir = (inputDir + 6) % 6;
		switch(dir){
			case 0 : return new Vector2Int(0, 1);
			case 1 : return new Vector2Int(1, 0);
			case 2 : return new Vector2Int(1, -1);
			case 3 : return new Vector2Int(0, -1);
			case 4 : return new Vector2Int(-1, 0);
			case 5 : return new Vector2Int(-1, 1);
			default: return Vector2Int.zero;
		}
	} // End of DirVector() method.


	// How many turns between two directions (0 to 3).
	public static int DeltaTurns(int inDir, int outDir){
		return deltaTurnsLookup[inDir][outDir];
	} // End of DeltaTurns() method.

	private static Dictionary<int, Dictionary<int, int>> deltaTurnsLookup = new Dictionary<int, Dictionary<int, int>>(){
		{ 0, new Dictionary<int, int>(){ {0, 0}, {1, 1}, {2, 2}, {3, 3}, {4, -2}, {5, -1} } },
		{ 1, new Dictionary<int, int>(){ {0, -1}, {1, 0}, {2, 1}, {3, 2}, {4, 3}, {5, -2} } },
		{ 2, new Dictionary<int, int>(){ {0, -2}, {1, -1}, {2, 0}, {3, 1}, {4, 2}, {5, 3} } },
		{ 3, new Dictionary<int, int>(){ {0, 3}, {1, -2}, {2, -1}, {3, 0}, {4, 1}, {5, 2} } },
		{ 4, new Dictionary<int, int>(){ {0, 2}, {1, 3}, {2, -2}, {3, -1}, {4, 0}, {5, 1} } },
		{ 5, new Dictionary<int, int>(){ {0, 1}, {1, 2}, {2, 3}, {3, -2}, {4, -1}, {5, 0} } },
	};


	public static int HexVecToDirection(Vector2Int inputVec){
		switch(inputVec.x){
			case -1:
				if(inputVec.y == 0)
					return 4;
				return 5;
			case 0:
				if(inputVec.y == 1)
					return 0;
				return 3;
			case 1:
				if(inputVec.y == 0)
					return 1;
				return 2;
		}
		return -1;
	} // End of HexVecToDirection() method.


	// Returns angle in degrees from the center to a vertex.
	public static float VertAngle(int dir){
		dir = (dir + 6) % 6;
		switch(dir){
			case 0: return 0f;
			case 1: return 60f;
			case 2: return 120f;
			case 3: return 180f;
			case 4: return 240f;
			case 5: return 300f;
		}
		return 0;
	} // End of VertAngle() method.


	// Returns all the hexes in a vancouver square.
	public static Vector2Int[] GetVancouverSquare(Vector2Int start, int range){
		Vector2Int[] potential = new Vector2Int[VancouverArea(range)];
		int num = 0;
		for(int i = -range; i <= range; i++){
			for(int j = Mathf.Max(-range, -i - range); j <= Mathf.Min(range, -i + range); j++){
				potential[num] = start + new Vector2Int(i, j);
				num++;
			}
		}
		Vector2Int[] tiles = new Vector2Int[num];
		for(int k = 0; k < tiles.Length; k++)
			tiles[k] = potential[k];
		return tiles;
	} // End of GetVancouverSquare() method.


	// Returns all hexes exactly a distance from a tile.
	public static Vector2Int[] GetVancouverRing(Vector2Int start, int range){
		Vector2Int[] tiles = new Vector2Int[VancouverCircumference(range)];
		int num = 0;
		for(int i = 0; i < 6; i++){
			for(int j = 0; j < range; j++){
				tiles[num] = start + (DirectionToHexVec(i) * range) + (DirectionToHexVec(i + 2) * j);
				num++;
			}
		}
		return tiles;
	} // End of GetVancouverRing() method.


	// Returns a 'straight, traversable line' of hexes from start to end.
	public static Vector2Int[] GetLineBresenham(Vector2Int start, Vector2Int end){
		int dx = start.x - end.x;
		int dy = start.y - end.y;
		int dz = GetZ(start) - GetZ(end);
		int steps = Mathf.Max(Mathf.Abs(dx - dy), Mathf.Abs(dy - dz), Mathf.Abs(dz - dx)) + 1;
	
		Vector2Int[] potential = new Vector2Int[steps];
		int num = 0;
	
		Vector2Int prev = Vector2Int.zero;
		for(int i = 0; i < steps; i++){
			Vector2Int test = Round((AxialToCubic(start) * (1 - (i / steps))) + (AxialToCubic(end) * (i / steps)));
			if(test != prev){
				potential[ num ] = test;
				num++;
			
				prev = test;
			}
		}
	
		Vector2Int[] tileLine = new Vector2Int[num];
		for(int i = 0; i < tileLine.Length; i++ )
			tileLine[i] = potential[i];
	
		return tileLine;
	} // End of GetLineBresenham() method.



	// Generates a mesh for an outline that highlights the given cells, with the origin at the rootTile. 'Thickness' controls
	//   the thickness, obviously. 'Dilate' pushes in or out of the perimeter of the tile.
	public static void GetTilesOutline(Mesh targetMesh, Vector2Int[] tiles, float thickness, float dilate = 0f){
		// Grab all edges that don't lead to a neighbor.
		List<HexEdge> edges = new List<HexEdge>();
		List<Vector2Int> tileList = new List<Vector2Int>(tiles);
		foreach(Vector2Int tile in tiles){
			for(int i = 0; i < 6; i++){
				Vector2Int neighbor = GetAdjacentCell(tile, i);
				if(!tileList.Contains(neighbor))
					edges.Add(new HexEdge(tile, i));
			}
		}

		List<Vector3> verts = new List<Vector3>();
		List<int> tris = new List<int>();
		for(int e = 0; e < edges.Count; e++){
			HexEdge edge = edges[e];

			// Verts on the current tile from which to build the outline
			Vector3 leftVert = GetVertex(edge.Direction) * (1f + dilate); // 0
			Vector3 rightVert = GetVertex(edge.Direction + 1) * (1f + dilate); // 1

			// Determine if the corners are concave or convex (based on adjacent tiles), and extrude the point accordingly.
			Vector3 leftVertExtrude;
			bool leftVertConcave = tileList.Contains(GetAdjacentCell(edge.TilePos, edge.Direction - 1));
			if(leftVertConcave)
				leftVertExtrude = leftVert + (rightVert * thickness); // 2, concave
			else
				leftVertExtrude = leftVert * (1 + thickness); // 2, convex

			Vector3 rightVertExtrude;
			bool rightVertConcave = tileList.Contains(GetAdjacentCell(edge.TilePos, edge.Direction + 1));
			if(rightVertConcave)
				rightVertExtrude = rightVert + (leftVert * thickness); // 3, concave
			else
				rightVertExtrude = rightVert * (1 + thickness); // 3, convex

			// If we're dilated and concave, gotta push the verts around.
			if(leftVertConcave){
				Vector3 leftVertDilateOffset = GetVertex(edge.Direction - 1) * -dilate;
				leftVert += leftVertDilateOffset;
				leftVertExtrude += leftVertDilateOffset;
			}
			if(rightVertConcave){
				Vector3 rightVertDilateOffset = GetVertex(edge.Direction + 2) * -dilate;
				rightVert += rightVertDilateOffset;
				rightVertExtrude += rightVertDilateOffset;
			}

			verts.Add(HexGridToWorld(edge.TilePos) + leftVert);
			verts.Add(HexGridToWorld(edge.TilePos) + rightVert);
			verts.Add(HexGridToWorld(edge.TilePos) + leftVertExtrude);
			verts.Add(HexGridToWorld(edge.TilePos) + rightVertExtrude);

			// Winding needs to be reversed if extrusion is negative...
			int triStartIndex = e * 4;

			tris.Add(triStartIndex);
			tris.Add(triStartIndex + 2);
			tris.Add(triStartIndex + 1);

			tris.Add(triStartIndex + 1);
			tris.Add(triStartIndex + 2);
			tris.Add(triStartIndex + 3);
		}

		targetMesh.SetTriangles(new int[0], 0);
		targetMesh.SetVertices(verts.ToArray());
		targetMesh.SetTriangles(tris.ToArray(), 0);
		targetMesh.RecalculateBounds();
	} // End of GetTileOutline() method.

	private struct HexEdge {
		public Vector2Int TilePos;
		public int Direction;
		public HexEdge(Vector2Int tilePos, int direction){
			TilePos = tilePos;
			Direction = direction;
		} // End of constructor.
	} // End of Edge struct.


	// Creates a solid fil within the given tiles. 'Dilate' pushes the fill out or pulls it in from the perimeter.
	//   If 'contiguous', the fill will stay connected between adjacent cells while dilated.
	public static void GetTilesFill(Mesh targetMesh, Vector2Int[] tiles, float dilate = 0f, bool contiguous = false){
		List<Vector3> verts = new List<Vector3>();
		List<int> tris = new List<int>();
		List<Vector2Int> tilesList = new List<Vector2Int>(tiles);
		for(int t = 0; t < tiles.Length; t++){
			// Each hex gets seven verts: one in the middle, and one per corner
			Vector3 tileWorldPos = HexGridToWorld(tiles[t]);
			verts.Add(tileWorldPos);
			for(int v = 0; v < 6; v++){
				Vector3 vert;

				// If continuous, gotta push verts around based on neighbors/dilation.
				if(contiguous){
					bool neighborOnLeft = tilesList.Contains(GetAdjacentCell(tiles[t], v - 1));
					bool neighborOnRight = tilesList.Contains(GetAdjacentCell(tiles[t], v));

					if(neighborOnLeft && neighborOnRight)
						vert = tileWorldPos + GetVertex(v);
					else{
						vert = tileWorldPos + (GetVertex(v) * (1 + dilate));
						if(neighborOnLeft && !neighborOnRight)
							vert += GetVertex(v - 1) * -dilate;
						else if(!neighborOnLeft && neighborOnRight)
							vert += GetVertex(v + 1) * -dilate;
					}
					
				} else {
					vert = tileWorldPos + (GetVertex(v) * (1 + dilate));
				}

				verts.Add(vert);
			}

			// Wind one triangle per side.
			for(int s = 0; s < 5; s++){
				tris.Add((t * 7));
				tris.Add((t * 7) + s + 1);
				tris.Add((t * 7) + s + 2);
			}
			// Last triangle wrap
			tris.Add(t * 7);
			tris.Add((t * 7) + 6);
			tris.Add((t * 7) + 1);
		}

		targetMesh.SetTriangles(new int[0], 0);
		targetMesh.SetVertices(verts.ToArray());
		targetMesh.SetTriangles(tris.ToArray(), 0);
		targetMesh.RecalculateBounds();
	} // End of GetTilesFill() method.


	// Width is in percentage of circumcircle radius.
	public static void GetTilesArrow(Mesh targetMesh, Vector2Int[] tiles, float width){
		List<Vector3> verts = new List<Vector3>();
		List<int> tris = new List<int>();

		// The total number of verts we have before starting our current tile. (Add current tile's
		//   vert count to this when finished.)
		int totalPreviousVerts = 0;

		// How roundy the rounded parts are.
		int endCapSegments = 10; // start cap
		int curveSegments = 10; // how many segments in a 60 degree turn.

		for(int t = 0; t < tiles.Length; t++){
			Vector3 tileCenter = HexGridToWorld(tiles[t]);
			Vector3 vecToLast = (t > 0)? HexGridToWorld(tiles[t - 1]) - tileCenter : Vector3.zero;
			Vector3 vecToNext = (t < (tiles.Length - 1))? HexGridToWorld(tiles[t + 1]) - tileCenter : Vector3.zero;

			int dirToLast = (t > 0)? HexVecToDirection(tiles[t - 1] - tiles[t]) : -1;
			int dirToNext = (t < (tiles.Length - 1))? HexVecToDirection(tiles[t + 1] - tiles[t]) : -1;

			// First tile (rounded cap)
			if(t == 0){
				// Main trunk
				Vector3 leftSegmentVec = GetVertex(dirToNext - 1) * (width / 2f);
				Vector3 centerLeft = tileCenter + leftSegmentVec;
				Vector3 centerRight = tileCenter - leftSegmentVec;
				Vector3 outEdgeLeft = tileCenter + (vecToNext / 2f) + leftSegmentVec;
				Vector3 outEdgeRight = tileCenter + (vecToNext / 2f) - leftSegmentVec;

				verts.Add(centerLeft); // 0
				verts.Add(centerRight); // 1
				verts.Add(outEdgeLeft); // 2
				verts.Add(outEdgeRight); // 3

				tris.Add(0); tris.Add(2); tris.Add(1);
				tris.Add(1); tris.Add(2); tris.Add(3);

				// Rounded cap
				verts.Add(tileCenter); // 4

				// At this point we have 5 verts total.
				for(int i = 0; i < (endCapSegments + 1); i++){
					Vector3 thisRadiusPoint = tileCenter + RotateVectorAzimuth(leftSegmentVec, -180f * ((float)i / endCapSegments));
					verts.Add(thisRadiusPoint); // 5 + i, previous is 4 + i.

					// Skip first point; it just sets up the first point.
					if(i > 0){
						tris.Add(4); // Tile center
						tris.Add(5 + i);
						tris.Add(4 + i);
					}
				}

				totalPreviousVerts = 6 + endCapSegments;

			// Last tile (arrow head)
			} else if(t == (tiles.Length - 1)){

				float arrowSideLengthPercent = Mathf.Clamp(width * 2f, 0f, 1f);

				// Arrow
				verts.Add(tileCenter);
				verts.Add(tileCenter + (GetVertex(dirToLast) * arrowSideLengthPercent));
				verts.Add(tileCenter + (GetVertex(dirToLast + 1) * arrowSideLengthPercent));

				tris.Add(totalPreviousVerts);
				tris.Add(totalPreviousVerts + 1);
				tris.Add(totalPreviousVerts + 2);

				Vector3 leftSegmentVec = GetVertex(dirToLast - 1) * (width / 2f);
				Vector3 edgeLeft = tileCenter + (vecToLast / 2f) + leftSegmentVec;
				Vector3 edgeRight = tileCenter + (vecToLast / 2f) - leftSegmentVec;

				//Debug.Log("ccr-asl: " + (circumcircleRadius - arrowSideLength));
				//Debug.Log("final: " + Mathf.Pow((circumcircleRadius - arrowSideLength), 1f/3f));


				float x = (sideL - (arrowSideLengthPercent * sideL)) / 2f;

				float trunkHeight = 2f * x * cellHeight;

				Vector3 capTrunkLeft = edgeLeft + ((-vecToLast.normalized) * trunkHeight);
				Vector3 capTrunkRight = edgeRight + ((-vecToLast.normalized) * trunkHeight);

				//Vector3 capTrunkLeft = edgeLeft + ((-vecToLast / 2f).normalized * sideL * Mathf.Pow((circumcircleRadius - (arrowSideLength * circumcircleRadius)), 1f/3f));
				//Vector3 capTrunkRight = edgeRight + ((-vecToLast / 2f).normalized * sideL * Mathf.Pow((circumcircleRadius - (arrowSideLength * circumcircleRadius)), 1f/3f));

				// Arrow had 3 verts.
				verts.Add(capTrunkLeft);
				verts.Add(capTrunkRight);
				verts.Add(edgeLeft);
				verts.Add(edgeRight);

				tris.Add(totalPreviousVerts + 3);
				tris.Add(totalPreviousVerts + 3 + 2);
				tris.Add(totalPreviousVerts + 3 + 1);

				tris.Add(totalPreviousVerts + 3 + 1);
				tris.Add(totalPreviousVerts + 3 + 2);
				tris.Add(totalPreviousVerts + 3 + 3);


				Debug.DrawLine(edgeLeft, edgeRight, Color.red);
				Debug.DrawLine(capTrunkLeft, capTrunkRight, Color.green);

				
			// Body tiles
			} else {
				// Straight line?
				int deltaTurns = DeltaTurns(dirToLast, dirToNext);
				if(deltaTurns == 3){
					// Simple rectangle.
					Vector3 leftSegmentVec = (GetVertex(dirToLast - 1) * (width / 2f));
					verts.Add(tileCenter + (vecToLast / 2f) + leftSegmentVec); // 0
					verts.Add(tileCenter + (vecToLast / 2f) - leftSegmentVec); // 1
					verts.Add(tileCenter + (vecToNext / 2f) + leftSegmentVec); // 2
					verts.Add(tileCenter + (vecToNext / 2f) - leftSegmentVec); // 3

					tris.Add(totalPreviousVerts);
					tris.Add(totalPreviousVerts + 1);
					tris.Add(totalPreviousVerts + 2);

					tris.Add(totalPreviousVerts + 1);
					tris.Add(totalPreviousVerts + 3);
					tris.Add(totalPreviousVerts + 2);

					totalPreviousVerts += 4;

				// Curve?
				} else {
					// Wide curves will be centered on the adjacent neighbor hex inside the turn. Tight curves will be
					//   anchored on the inside vertex.
					bool tightTurn = Math.Abs(deltaTurns) == 1;
					bool leftTurn = deltaTurns > 0;

					int dirToNeighbor = leftTurn? (dirToLast + 1) : (dirToLast - 1);

					Vector3 circleCenter;
					float innerRadius;
					float outerRadius;

					if(tightTurn){
						circleCenter = HexGridToWorld(tiles[t]) + GetVertex(dirToNeighbor + (leftTurn? 0 : 1));
						innerRadius = (sideL / 2f) - (circumcircleRadius * (width / 2f));
						outerRadius = (sideL / 2f) + (circumcircleRadius * (width / 2f));
					// Wided turn
					} else {
						Vector2Int anchorNeighbor = GetAdjacentCell(tiles[t], dirToNeighbor);
						circleCenter = HexGridToWorld(anchorNeighbor);
						innerRadius = circumcircleRadius + ((sideL / 2f) - (circumcircleRadius * (width / 2f)));
						outerRadius = circumcircleRadius + ((sideL / 2f) + (circumcircleRadius * (width / 2f)));
					}

					for(int i = 0; i < (curveSegments * (tightTurn? 2f : 1f)) + 1; i++){
						float angle = VertAngle(dirToNeighbor + (leftTurn? -2 : 3)) + (60f * ((float)i / curveSegments) * (leftTurn? -1f : 1f));
						Vector3 innerPoint = circleCenter + RotateVectorAzimuth(Vector3.forward * innerRadius, angle);
						Vector3 outerPoint = circleCenter + RotateVectorAzimuth(Vector3.forward * outerRadius, angle);

						verts.Add(innerPoint);
						verts.Add(outerPoint);

						// Ignore first, since we don't have previous segments to mess with.
						if(i > 0){
							tris.Add(totalPreviousVerts + ((i - 1) * 2));
							tris.Add(totalPreviousVerts + ((i - 1) * 2) + (leftTurn? 2 : 1));
							tris.Add(totalPreviousVerts + ((i - 1) * 2) + (leftTurn? 1 : 2));

							tris.Add(totalPreviousVerts + ((i - 1) * 2) + 1);
							tris.Add(totalPreviousVerts + ((i - 1) * 2) + (leftTurn? 2 : 3));
							tris.Add(totalPreviousVerts + ((i - 1) * 2) + (leftTurn? 3 : 2));
						}
					}

					totalPreviousVerts += (curveSegments * (tightTurn? 2 : 1) + 1) * 2;
				}
			}
		}

		targetMesh.SetTriangles(new int[0], 0);
		targetMesh.SetVertices(verts.ToArray());
		targetMesh.SetTriangles(tris.ToArray(), 0);
		targetMesh.RecalculateNormals();
	} // End of GetTilesArrow().


	// Returns a local vertext offset direction for a vertex in a direction.
	public static Vector3 GetVertex(int inputDir){
		int dir = (inputDir + 6) % 6;
		switch(dir){
			case 0 : return new Vector3(0f, 0f, circumcircleRadius);
			case 1 : return new Vector3((sqrtOf3 * circumcircleRadius) / 2f, 0f, circumcircleRadius / 2f);
			case 2 : return new Vector3((sqrtOf3 * circumcircleRadius) / 2f, 0f, -circumcircleRadius / 2f);
			case 3 : return new Vector3(0f, 0f, -circumcircleRadius);
			case 4 : return new Vector3(-(sqrtOf3 * circumcircleRadius) / 2f, 0f, -circumcircleRadius / 2f);
			case 5 : return new Vector3(-(sqrtOf3 * circumcircleRadius) / 2f, 0f, circumcircleRadius / 2f);
		}
		return Vector3.zero;
	} // End of GetVertex() method.


	// Rotates a 2D vector (e.g. ground plane UI) around its origin.
	public static Vector3 RotateVectorAzimuth(Vector3 input, float angle){
		//x2=cosβx1−sinβy1
		//y2=sinβx1+cosβy1

		return Quaternion.Euler(0f, angle, 0f) * input;
		//return new Vector3((Mathf.Cos(angle) * input.x) - (Mathf.Sin(angle) * input.y), 0f, (Mathf.Sin(angle) * input.x) + (Mathf.Cos(angle) * input.y));
	} // End of RotateVectorAzimuth() method.



	public static void GetGrid(Mesh targetMesh, int mapRadius, float gridThickness){
		List<Vector3> verts = new List<Vector3>();
		List<int> tris = new List<int>();
		Vector2Int[] tiles = GetVancouverSquare(Vector2Int.zero, mapRadius);
		for(int t = 0; t < tiles.Length; t++){
			// Each hex gets seven verts: one in the middle, and one per corner
			Vector3 tileWorldPos = HexGridToWorld(tiles[t]);

			for(int v = 0; v < 7; v++){
				Vector3 vertex = tileWorldPos + GetVertex(v);
				Vector3 insideVertex = tileWorldPos + (GetVertex(v) * (1 - gridThickness));

				if(v < 6){
					verts.Add(insideVertex);
					verts.Add(vertex);
				}

				int tileBaseVert = 12 * t;
				if(v > 0){
					int edgeBaseVert = (v % 6) * 2;
								  
					tris.Add(tileBaseVert + edgeBaseVert);
					tris.Add(tileBaseVert + (edgeBaseVert + 1) % 12);
					tris.Add(tileBaseVert + (edgeBaseVert + 2) % 12);
							 
					tris.Add(tileBaseVert + (edgeBaseVert + 1) % 12);
					tris.Add(tileBaseVert + (edgeBaseVert + 3) % 12);
					tris.Add(tileBaseVert + (edgeBaseVert + 2) % 12);
				}
			}
		}

		targetMesh.SetTriangles(new int[0], 0);
		targetMesh.SetVertices(verts.ToArray());
		targetMesh.SetTriangles(tris.ToArray(), 0);
		targetMesh.RecalculateBounds();
	} // End of GetGrid().

	/*
	static function InverseGetVertex( tile : Vector2, vertex : Vector2 ) : int
	{
		if( vertex == ( tile + ( Vector2( 2.0, -1.0 ) * ( 1.0 / 3.0 ))))
			return 0;
	
		if( vertex == ( tile + ( Vector2( 1.0, 1.0 ) * ( 1.0 / 3.0 ))))
			return 1;
	
		if( vertex == ( tile + ( Vector2( -1.0, 2.0 ) * ( 1.0 / 3.0 ))))
			return 2;
	
		if( vertex == ( tile + ( Vector2( -2.0, 1.0 ) * ( 1.0 / 3.0 ))))
			return 3;
	
		if( vertex == ( tile + ( Vector2( -1.0, -1.0 ) * ( 1.0 / 3.0 ))))
			return 4;
	
		if( vertex == ( tile + ( Vector2( 1.0, -2.0 ) * ( 1.0 / 3.0 ))))
			return 5;
	}


	// Returns vertices that can be drawn to with a straight line from the CENTER of 'start' without crossing
	// adjacent hexes.
	static function ConcaveVertices( start : Vector2, end : Vector2 ) : int[]
	{
		var sextant = AxialToCubic( end ) - AxialToCubic( start );

		var x = sextant.x;
		var y = sextant.y;
		var z = sextant.z;
	
		var potentialAxes = new boolean[6];

		if(( y < x ) || ( -x < z ))
			potentialAxes[0] = true;
		
		if(( -z < y ) || ( -x < z ))
			potentialAxes[1] = true;
		
		if(( -z < y ) || ( y > x ))
			potentialAxes[2] = true;
	
		if(( -x > z ) || ( y > x ))
			potentialAxes[3] = true;
		
		if(( -x > z ) || ( -z > y ))
			potentialAxes[4] = true;
	
		if(( y < x ) || ( -z > y ))
			potentialAxes[5] = true;
	
	
		var axesCount : int;
		for( var j = 0; j < 6; j++ )
		{
			if( potentialAxes[j] )
				axesCount++;
		}
	
		var axes = new int[ axesCount ];
		var axesNum : int;
		for( var i = 0; i < 6; i++ )
		{
			if( potentialAxes[i] )
			{
				axes[axesNum] = i;
				axesNum++;
			}
		}
		return axes;
	}
	*/


	public static Vector3 HexGridToWorld(Vector2Int pos){
		return new Vector3(pos.x + (pos.y * 0.5f), 0f, pos.y * 0.86605f);
	} // End of HexPos().


	public static Vector2Int WorldToHexGrid(Vector3 worldPos){
		
		int row = Mathf.FloorToInt((worldPos.z + (sideL / 2f)) / cellHeight);
		int column = Mathf.RoundToInt(worldPos.x - (row * 0.5f));

		float relX = worldPos.x - column - (row * 0.5f);
		float relY = worldPos.z - (row * cellHeight);
		
		if(relY > ((c / 0.5f) * relX) + (longDiagL / 2f)){
			column--;
			row++;
		} else if(relY > (-(c / 0.5f) * relX) + (longDiagL / 2f)){
			row++;
		}

		return new Vector2Int(column, row);
	} // End of HexPos().



	// Tells whether a given point is “left of” or “right of” a given directed line.
	// returns LEFT if (x0,y0)-->(x1,y1)-->(x2,y2) turns to the left,
	//		  RIGHT if (x0,y0)-->(x1,y1)-->(x2,y2) turns to the right
	//	   STRAIGHT if (x2,y2) is colinear with (x0,y0)-->(x1,y1)
	public static int Turn(Vector2 lineStart, Vector2 lineEnd, Vector2 point){
		float cross = ((lineEnd.x - lineStart.x) * (point.y - lineStart.y)) - ((point.x - lineStart.x) * (lineEnd.y - lineStart.y));
	
		// Note: 0.0002 was determined by sampling a large number of small 'cross' values on a huge map.
		if( Mathf.Abs( cross ) < 0.00002 )
			return 0;
		else if( cross > 0.0 )
			return 1;
		else
			return -1;
	} // End of Turn() method.



	// Gets 'straight line' distance from a hex to a point in relative space, pointed along the cubic axese.
	public static float HexagonSpaceDistance(Vector2 samplePoint){

		// Determine which egde we're closest to.
		float angle = (Mathf.PI / 2f) - Mathf.Atan2(samplePoint.y, samplePoint.x);
		int edge = (Mathf.FloorToInt(angle / (Mathf.PI / 3f)) + 6) % 6;

		Vector3 leftVertex = GetVertex(edge);
		Vector3 rightVertex = GetVertex(edge + 1);

		float dist = Utilities.FindDistanceToSegment(samplePoint, leftVertex.ToMap2D(), rightVertex.ToMap2D());
		return dist;
	} // End of DistToEdge().




	/*
	// This function will return if a hexagon intersects or is tangent to a line.
	static function LineIntersectsTile( lineStart : Vector2, lineEnd : Vector2, tile : Vector2 ) : boolean
	{
		var side1 : int;
		side1 = Turn( lineStart, lineEnd, GetVertex( tile, 0 ));
	
		// If line exactly hits vertex, return true.
		if( side1 == 0 )
			return true;
		for( var i = 1; i < 6; i++ )
		{
			var j = Turn( lineStart, lineEnd, GetVertex( tile, i ));
			// If line exactly hits vertex or the tile 'envelopes' the line (has points on both sides), return true.
			if(( j == 0 ) || ( j != side1 ))
				return true;
		}
	
		return false;
	}



	static var next1 : HexTile;
	static var next2 : HexTile;
	static var cur1 : HexTile;
	static var cur2 : HexTile;
	static var last1 : HexTile;
	static var last2 : HexTile;  

	static function GetLineSupercover( start : Vector2, end : Vector2 )
	{
		var potential = new Vector2[ 99999 ];
		var num : int;

		cur1 = World.GetTile( start );
		cur2 = null;
		last1 = null;
		last2 = null;

		while( true )
		{
			// Break if hit obstacle
			if(( cur1.h != 0 ) || ( cur2 && ( cur2.h != 0 )))
				break;
		
	
			if( cur1 )
			{
				potential[ num ] = cur1.pos2;
				num++;
			}
			if( cur2 )
			{
				potential[ num ] = cur2.pos2;
				num++;
			}
		
			// If we've hit the target, break.
			if( !cur1 || ( cur1.pos2 == end ) || ( cur2 && ( cur2.pos2 == end )))
				break;
		
			next1 = null;
			next2 = null;
		
			NextHexes( cur1.pos2, start, end );
			if( cur2 )
				NextHexes( cur2.pos2, start, end );
		

			last1 = cur1;
			last2 = cur2;
			cur1 = next1;
			cur2 = next2;
		}
	
		var tiles = new Vector2[ num ];
		for( var k = 0; k < tiles.length; k++ )
			tiles[k] = potential[k];

		return tiles;
	}

	static function NextHexes( cur : Vector2, start : Vector2, end : Vector2 ) : int
	{
		var h : Vector2;
		var turn0 : int;
		var turn1 : int;
	
		for ( var i = 0; i < 6 ; i++ )
		{
			// These together define one side of the hexagon. The current neighbor is the neighbor who shares the side with
			//   the current hexagon.
			// Side of the line the first vertex is on.
			turn0 = Turn( start, end, GetVertex( cur, i ));
			// Side of the line the next (clockwise) vertex is on.
			turn1 = Turn( start, end, GetVertex( cur, i + 1 ));
		
		
			// Success if either vertex intersects the line exactly, or the line passes between them (turns are different.)
			if(( turn0 == 0 ) || ( turn1 == 0 ) || ( turn0 < turn1 ))
			{
				// Consider the hexagon that shares the same side.
				h = cur + DirVector(i);
				if(( h != cur1.pos2 ) && ( !cur2 || ( h != cur2.pos2 )) && ( !next1 || ( h != next1.pos2 )) && ( !next2 || ( h != next2.pos2 )) && ( !last1 || ( h != last1.pos2 )) && ( !last2 || ( h != last2.pos2 )))
				{
					if( next1 == null )
						next1 = World.GetTile(h);
					else if( next2 == null )
						next2 = World.GetTile(h);
				}
				// If the right vertex is on the line, consider the neighbor to the right.
				if( turn0 == 0 )
				{
					h = cur + DirVector( i - 1 );
					if(( h != cur1.pos2 ) && ( !cur2 || ( h != cur2.pos2 )) && ( !next1 || ( h != next1.pos2 )) && ( !next2 || ( h != next2.pos2 )) && ( !last1 || ( h != last1.pos2 )) && ( !last2 || ( h != last2.pos2 )))
					{
						if( next1 == null )
							next1 = World.GetTile(h);
						else if( next2 == null )
							next2 = World.GetTile(h);
					}
				}
				// If the left vertex is on the line, consider the neighbor to the left.
				if( turn1 == 0 )
				{
					h = cur + DirVector( i + 1 );
					if(( h != cur1.pos2 ) && ( !cur2 || ( h != cur2.pos2 )) && ( !next1 || ( h != next1.pos2 )) && ( !next2 || ( h != next2.pos2 )) && ( !last1 || ( h != last1.pos2 )) && ( !last2 || ( h != last2.pos2 )))
					{
						if( next1 == null )
							next1 = World.GetTile(h);
						else if( next2 == null )
							next2 = World.GetTile(h);
					}
				}
			}
		}
	
		if( next2 == null )
			return 1;
		else
			return 2;
	}





	// Returns 'true' if a straight line can be drawn between the start hex and the end hex.
	// NOTE: does NOT consider lines tangent to hexes to be overlapping.
	static function TestFOVToTile( start : Vector2, end : Vector2, targetVertex : int ) : boolean
	{
		cur1 = World.GetTile( start );
		cur2 = null;
		last1 = null;
		last2 = null;
	
		while( true )
		{
			if( cur1 && ( cur1.h == 0 ))
				World.visHit[ cur1.pos2.x + World.mapSize, cur1.pos2.y + World.mapSize ] = true;
		
			if( cur2 && ( cur2.h == 0 ))
				World.visHit[ cur2.pos2.x + World.mapSize, cur2.pos2.y + World.mapSize ] = true;


			// Break if hit obstacle
			if( !cur1 || (( cur1.h > 0 ) && ( !cur2 || ( cur2.h > 0 ))))
				return false;
		
			// If we've hit the target, break.
			if( !cur1 || ( cur1.pos2 == end ) || ( cur2 && ( cur2.pos2 == end )))
				return true;
		
		
			next1 = null;
			next2 = null;
		
			NextHexesFOV( cur1.pos2, start, end, targetVertex );
			if( cur2 )
				NextHexesFOV( cur2.pos2, start, end, targetVertex );
		
			last1 = cur1;
			last2 = cur2;
			cur1 = next1;
			cur2 = next2;
		}
	}

	static function TestFOVToTile( start : Vector2, end : Vector2 ) : boolean
	{
		return TestFOVToTile( start, end, -1 ); 
	}


	static function NextHexesFOV( cur : Vector2, start : Vector2, end : Vector2, targetVertex : int ) : int
	{
		var h : Vector2;
		var endActual : Vector2;
	
		if( targetVertex == -1 )
			endActual = end;
		else
			endActual = GetVertex( end, targetVertex );
	
		for ( var i = 0; i < 6 ; i++ )
		{
			// These together define one side of the hexagon. The current neighbor is the neighbor who shares the side with
			//   the current hexagon.
			// Side of the line the first vertex is on.
			var vert0 = GetVertex( cur, i );
			var turn0 = Turn( start, endActual, vert0 );
			// Side of the line the next (clockwise) vertex is on.
			var vert1 = GetVertex( cur, i + 1 );
			var turn1 = Turn( start, endActual, vert1 );
		
			if( vert0 == endActual )
				next1 = World.GetTile( end );
		
			// Success if either vertex intersects the line exactly, or the line passes between them (turns are different.)
			else if(( turn0 == 0 ) || (( turn0 != 0 ) && ( turn1 != 0 ) && ( turn0 < turn1 )))
			{
				// Consider the hexagon that shares the same side if we pass between the points (in the positive direction.)
				if(( turn0 != 0 ) && ( turn1 != 0 ) && ( turn0 < turn1 ))
				{
					h = cur + DirVector(i);
					if(( h != cur1.pos2 ) && ( !cur2 || ( h != cur2.pos2 )) && ( !next1 || ( h != next1.pos2 )) && ( !next2 || ( h != next2.pos2 )) && ( !last1 || ( h != last1.pos2 )) && ( !last2 || ( h != last2.pos2 )))
					{
						if( next1 == null )
							next1 = World.GetTile(h);
						else if( next2 == null )
							next2 = World.GetTile(h);
					}
				}
				// If vertex on the line, test if we overlap the left hex...
				h = cur + DirVector( i );
				if(( turn0 == 0 ) && ( turn1 == 1 ) && ( Turn( start, endActual, GetVertex( h, InverseGetVertex( h, vert0 ) + 1 )) <= 0 ))
				{
					if(( h != cur1.pos2 ) && ( !cur2 || ( h != cur2.pos2 )) && ( !next1 || ( h != next1.pos2 )) && ( !next2 || ( h != next2.pos2 )) && ( !last1 || ( h != last1.pos2 )) && ( !last2 || ( h != last2.pos2 )))
					{
						if( next1 == null )
							next1 = World.GetTile(h);
						else if( next2 == null )
							next2 = World.GetTile(h);
					}
				}
				// ...and the right hex...
				h = cur + DirVector( i - 1 );
				if(( turn0 == 0 ) && ( turn1 == 1 ) && ( Turn( start, endActual, GetVertex( h, InverseGetVertex( h, vert0 ) - 1 )) >= 0 ))
				{
					if(( h != cur1.pos2 ) && ( !cur2 || ( h != cur2.pos2 )) && ( !next1 || ( h != next1.pos2 )) && ( !next2 || ( h != next2.pos2 )) && ( !last1 || ( h != last1.pos2 )) && ( !last2 || ( h != last2.pos2 )))
					{
						if( next1 == null )
							next1 = World.GetTile(h);
						else if( next2 == null )
							next2 = World.GetTile(h);
					}
				}
			}
		}
	
	
		if( next2 == null )
			return 1;
		else
			return 2;
	}



	// Returns the difference between two directions.
	public static int DeltaDir(int dir0, int dir1){
		return Mathf.RoundToInt(Mathf.DeltaAngle(dir0 * 60, dir1 * 60) / 60);
	} // End of DeltaDir() method.
	*/

} // End of HexMath class.