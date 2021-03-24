using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

public class GameManager : MonoBehaviour, IMouseClickable {

	public static GameManager Inst { get; private set; }

	[SerializeField] private TerrainConfig terrainConfig;
	[SerializeField] private TileBlendingMap tileBlendingMap;

	static Color[] allianceColors = new Color[]{Color.white, Color.cyan, Color.yellow};
	public const int actionsPerTurn = 5;

	public static int playerTurn { get; private set; } = -1;

	public static bool busy = false;
	public static bool ready = true;
	private int busyCooldown;

	private Unit[] teamSelectedUnit = new Unit[12];
	private int numTeams = 12;

	private bool mouse0Released = true;
	private bool mouse1Released = true;

	public HexTile hoveredTile { get; private set; } = null;
	public HexTile selectedTile { get; private set; } = null;
	private List<HexTile> path = null;
	private HexTile oldHoveredTile = null;
	private CancellationTokenSource pathCTS = new CancellationTokenSource();

	// Where on the map the cursor is.
	public Vector3 CursorMapPosition { get; private set; }

	[Space]
	[SerializeField] private MeshFilter hexFillMesh = null;
	[SerializeField] private MeshFilter hexOutlineMesh = null;
	[SerializeField] private MeshFilter arrowMesh = null;

	[SerializeField] private MeshFilter hoveredCellOutline = null;



	private void Awake(){
		Inst = this;

		terrainConfig.Init();
		tileBlendingMap.Init();
	} // End of Awake().


	private void Start() {
		World.Inst.Init();
		InputManager.Inst.Init();

		foreach(Unit unit in Unit.GetAllUnits)
			unit.ManualStart();
	} // End of Start().


	private void Update(){

		MainCameraController.Inst.Frame();


		busyCooldown--;
		if(busy){
			ready = false;
			busyCooldown = 3;
		}
		busy = false;
		ready = busyCooldown <= 0;
	
	
		// Reset input testers
		if(!Input.GetMouseButton(0))
			mouse0Released = true;
		
		if(!Input.GetMouseButton(1))
			mouse1Released = true;



		// Find hovered tile
		Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
		Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
		HexTile newHoveredTile = null;
		float rayDist;
		if(groundPlane.Raycast(mouseRay, out rayDist)){
			CursorMapPosition = mouseRay.origin + (mouseRay.direction * rayDist);
			Vector2Int roundedPos = HexMath.WorldToHexGrid(CursorMapPosition);
			newHoveredTile = World.GetTile(roundedPos);
		}
		if(newHoveredTile != hoveredTile)
			hoveredTile = newHoveredTile;
		
		// Pathfinding test
		if((selectedTile != null) && (hoveredTile != null) && (hoveredTile != selectedTile) && (hoveredTile != oldHoveredTile)){
			oldHoveredTile = hoveredTile;

			// Forget old path and clear any in-progress calcs.
			path = null;
			pathCTS.Cancel();
			pathCTS = new CancellationTokenSource();

			// Spin up new path generation.
			Task.Run(() => GeneratePathAsync());
		}

		if(hoveredTile != null){
			HexMath.GetTilesOutline(hoveredCellOutline.mesh, new Vector2Int[]{ hoveredTile.GridPos2 }, 0.05f, 0.1f + (Mathf.Cos(Time.time * Mathf.PI) * 0.05f));

			if(Input.GetKeyDown(KeyCode.Alpha1)){
				hoveredTile.SetTerrainType(TerrainType.oceanFloor);
				World.Inst.RebuildTerrain(hoveredTile.WorldPos.ToMap2D(), 1.5f * Vector2.one);
			} else if(Input.GetKeyDown(KeyCode.Alpha2)){
				hoveredTile.SetTerrainType(TerrainType.shallowWater);
				World.Inst.RebuildTerrain(hoveredTile.WorldPos.ToMap2D(), 1.5f * Vector2.one);
			} else if(Input.GetKeyDown(KeyCode.Alpha3)){
				hoveredTile.SetTerrainType(TerrainType.openGround);
				World.Inst.RebuildTerrain(hoveredTile.WorldPos.ToMap2D(), 1.5f * Vector2.one);
			} else if(Input.GetKeyDown(KeyCode.Alpha4)){
				hoveredTile.SetTerrainType(TerrainType.mountains);
				World.Inst.RebuildTerrain(hoveredTile.WorldPos.ToMap2D(), 1.5f * Vector2.one);
			}
		}

		UpdateTileColors();
	} // End of Update().


	// DEBUG. Not useful for eventual game, only proof of concept.
	private void GeneratePathAsync(){
		HexTile[] newPath = World.FindPath(selectedTile, hoveredTile, pathCTS.Token);
		if(newPath != null)
			path = new List<HexTile>(newPath);
	} // End of GeneratePathAsync.


	private void UpdateTileColors(){
		foreach(HexTile tile in World.allTiles){
			Color targetColor = Color.white;
			if(!tile.Navigable)
				targetColor = Color.Lerp(targetColor, Color.black, 0.4f);

			//World.GetVisualization(tile).GetComponent<Renderer>().material.color = targetColor;
		}


		if((selectedTile != null) && (path != null)){
			Vector2Int[] pathPositions = new Vector2Int[path.Count];
			for(int i = 0; i < path.Count; i++)
				pathPositions[i] = path[i].GridPos2;
			HexMath.GetTilesOutline(hexOutlineMesh.mesh, pathPositions, 0.05f, 0.15f);
			HexMath.GetTilesFill(hexFillMesh.mesh, pathPositions, 0f, true);
			HexMath.GetTilesArrow(arrowMesh.mesh, pathPositions, 0.2f);
		}

	} // End of UpdateActiveTileTints() method.


	public void OnClicked(int mouseButton){
		if(hoveredTile != selectedTile){
			selectedTile = hoveredTile;
			if(selectedTile.occupyingUnit != null)
				selectedTile.occupyingUnit.Select();
		}
	} // End of CheckForClickedTile().


	public void OnGUI() {
		if(hoveredTile != null){
			Vector3 screenPoint = Camera.main.WorldToScreenPoint(HexMath.HexGridToWorld(hoveredTile.GridPos2));
			GUI.Label(new Rect(screenPoint.x, Screen.height - screenPoint.y, 100f, 100f), hoveredTile.GridPos2.ToString());
		}
	} // End of OnGUI().

} // End of GameManager class.