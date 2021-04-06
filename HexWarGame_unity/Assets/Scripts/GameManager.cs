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

	[Space]
	[SerializeField] private Canvas mainCanvas; public Canvas MainCanvas { get { return mainCanvas; } }


	public const int actionsPerTurn = 5;

	public static int playerTurn { get; private set; } = -1;

	public static bool busy = false;
	public static bool ready = true;

	CancellationTokenSource cts = new CancellationTokenSource();

	public GameMode GameMode { get; private set; } = GameMode.edit;
	public Action<GameMode> GameModeChanged = null;
	public bool IsEditor { get { return GameMode == GameMode.edit; } }

	public static float UIPulsar { get { return 0.5f + (0.5f * Mathf.Cos(3f * Time.time * Mathf.PI)); } }


	private void Awake(){
		Inst = this;
	} // End of Awake().


	private void Start() {
		terrainConfig.Init();
		guiConfig.Init();
		tileBlendingMap.Init();
		MainCameraController.Inst.ManualStart();
		EditorOptionsTray.Inst.ManualStart();
		ScenarioEditor.Inst.ManualStart();
		SaveLoadManager.Inst.ManualStart();

		UnitsManager.Inst.ManualStart();
		foreach(Unit unit in Unit.GetAllUnits)
			unit.ManualStart();

		// Main thread
		Debug.Log("Starting main thread...");
		InputManager.Inst.MainLoop(cts.Token);
		MainCameraController.Inst.MainLoop(cts.Token);
		Unit.MainLoop(cts.Token);
	} // End of Start().


	private void OnApplicationQuit() {
		cts.Cancel();
		Debug.Log("Shutting down...");
	} // End of OnApplicationQuit().


	public void SetGameMode(GameMode mode){
		if(mode != GameMode){
			GameMode = mode;
			GameModeChanged?.Invoke(mode);
		}
	} // End of SetGameMode().


	public void Button_Quit(){
		cts.Cancel();
		Application.Quit();
	} // End of Button_Quit().

} // End of GameManager class.


public enum GameMode {
	edit,
	play
}