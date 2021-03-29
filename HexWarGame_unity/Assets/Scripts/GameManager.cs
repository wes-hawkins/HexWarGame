using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
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

	CancellationTokenSource cts = new CancellationTokenSource();

	public GameMode GameMode { get; private set; } = GameMode.edit;
	public Action<GameMode> GameModeChanged = null;


	private void Awake(){
		Inst = this;
	} // End of Awake().


	private void Start() {
		terrainConfig.Init();
		guiConfig.Init();
		tileBlendingMap.Init();
		MainCameraController.Inst.ManualStart();
		World.Inst.Init();
		EditorOptionsTray.Inst.ManualStart();
		ScenarioEditor.Inst.ManualStart();
		SaveLoadManager.Inst.ManualStart();

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


	public void SetGameMode(GameMode mode){
		if(mode != GameMode){
			GameMode = mode;
			GameModeChanged?.Invoke(mode);
		}
	} // End of SetGameMode().


	public void Button_Quit(){
		Application.Quit();
	} // End of Button_Quit().

} // End of GameManager class.


public enum GameMode {
	edit,
	play
}