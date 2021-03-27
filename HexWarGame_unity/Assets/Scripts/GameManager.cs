using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

public class GameManager : MonoBehaviour {

	public static GameManager Inst { get; private set; }

	[SerializeField] private TerrainConfig terrainConfig;
	[SerializeField] private GUIConfig guiConfig;
	[SerializeField] private TileBlendingMap tileBlendingMap;

	public const int actionsPerTurn = 5;

	public static int playerTurn { get; private set; } = -1;

	public static bool busy = false;
	public static bool ready = true;
	private int busyCooldown;

	CancellationTokenSource cts = new CancellationTokenSource();



	private void Awake(){
		Inst = this;
	} // End of Awake().


	private void Start() {
		terrainConfig.Init();
		guiConfig.Init();
		tileBlendingMap.Init();
		World.Inst.Init();
		EditorOptionsTray.Inst.ManualStart();

		foreach(Unit unit in Unit.GetAllUnits)
			unit.ManualStart();


		// Main thread
		Debug.Log("Starting main thread...");
		InputManager.Inst.MainLoop(cts.Token);
		MainCameraController.Inst.MainLoop(cts.Token);
		Unit.MainLoop(cts.Token);
	} // End of Start().


	private void OnApplicationQuit() {
		Debug.Log("Shutting down...");
		cts.Cancel();
	} // End of OnApplicationQuit().

} // End of GameManager class.