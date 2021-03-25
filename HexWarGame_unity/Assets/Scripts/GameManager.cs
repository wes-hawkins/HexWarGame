using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

public class GameManager : MonoBehaviour {

	public static GameManager Inst { get; private set; }

	[SerializeField] private TerrainConfig terrainConfig;
	[SerializeField] private GUIConfig guiConfig;
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

	private List<HexTile> path = null;
	private CancellationTokenSource pathCTS = new CancellationTokenSource();


	float outlineThickness = 0.08f;



	private void Awake(){
		Inst = this;

		terrainConfig.Init();
		guiConfig.Init();
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
		Unit.StaticUpdate();


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

		// Terrain editing
		if(InputManager.Inst.HoveredTile != null){
			if(Input.GetKey(KeyCode.Alpha1))
				InputManager.Inst.HoveredTile.SetTerrainType(TerrainType.deepWater);
			else if(Input.GetKey(KeyCode.Alpha2))
				InputManager.Inst.HoveredTile.SetTerrainType(TerrainType.shallowWater);
			else if(Input.GetKey(KeyCode.Alpha3))
				InputManager.Inst.HoveredTile.SetTerrainType(TerrainType.openGround);
			else if(Input.GetKey(KeyCode.Alpha4))
				InputManager.Inst.HoveredTile.SetTerrainType(TerrainType.mountains);
		}

	} // End of Update().


	/*
	public void OnGUI() {
		if(hoveredTile != null){
			Vector3 screenPoint = Camera.main.WorldToScreenPoint(HexMath.HexGridToWorld(hoveredTile.GridPos2));
			GUI.Label(new Rect(screenPoint.x, Screen.height - screenPoint.y, 100f, 100f), hoveredTile.GridPos2.ToString());
		}
	} // End of OnGUI().
	*/

} // End of GameManager class.