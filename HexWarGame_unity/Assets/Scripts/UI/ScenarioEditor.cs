using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading;
using System.Threading.Tasks;

public class ScenarioEditor : MonoBehaviour {
	
	public static ScenarioEditor Inst = null;

	[Header("Playtest Buttons")]
    [SerializeField] private Button playtestButton = null;
	[Space]
    [SerializeField] private Button endPlaytestButton = null; // Stop editing, revert to previous state
    [SerializeField] private Button commitPlaytestButton = null; // Stop editing, but keep current state

	[Header("Editing UI")]
    [SerializeField] private Button terrainButton = null;
    [SerializeField] private Button structuresButton = null;
    [SerializeField] private Button unitsButton = null;
	[Space]
    [SerializeField] private RectTransform optionsTray = null;

	[Header("File Buttons")]
    [SerializeField] private Button newButton = null;
    [SerializeField] private Button saveButton = null;
    [SerializeField] private Button loadButton = null;

	[Space]
	[SerializeField] private RectTransform inset = null;
	[SerializeField] private RectTransform topCautionBar = null;
	[SerializeField] private RectTransform bottomCautionBar = null;

	// TODO: Notifications system
	[SerializeField] private RectTransform holdShiftReminder = null;
	private float cautionBarSize;

	private float editModeTransitionTime = 0.5f;

	public bool HotControl { get { return Input.GetKey(KeyCode.LeftShift) && (GameManager.Inst.GameMode == GameMode.edit); } }


	private void Awake() {
		Inst = this;
	} // End of Awake().


	public void ManualStart(){
		cautionBarSize = bottomCautionBar.anchorMax.y;
		UpdateEditTransitionEffects(1f);
	} // End of ManualStart() method.


	public void Button_Playtest(){
		ChangeGameMode(GameMode.play);
	} // End of Button_Playtest().


	public void Button_BackToEditing(){
		ChangeGameMode(GameMode.edit);
	} // End of Button_BackToEditing().


	private async void ChangeGameMode(GameMode mode){
		Unit.DeselectAll();
		float t = 0f;
		do{
			t = Mathf.MoveTowards(t, 1f, Time.deltaTime / editModeTransitionTime);
			UpdateEditTransitionEffects((mode == GameMode.play)? 1f - t : t);
			await Task.Yield();
		} while (t < 1f);

		GameManager.Inst.SetGameMode(mode);
	} // End of Button_Test().


	private void UpdateEditTransitionEffects(float editModeLerp){
		float smoothedLerp = Utilities.LinToSmoothLerp(Utilities.LinToSmoothLerp(editModeLerp));

		topCautionBar.anchorMin = new Vector2(0f, 1f - (cautionBarSize * smoothedLerp));
		bottomCautionBar.anchorMax = new Vector2(1f, cautionBarSize * smoothedLerp);

		inset.anchorMin = new Vector2(0f, cautionBarSize * smoothedLerp);
		inset.anchorMax = new Vector2(1f, 1f - (cautionBarSize * smoothedLerp));

		terrainButton.transform.localScale = Vector3.one * smoothedLerp;
		structuresButton.transform.localScale = Vector3.one * smoothedLerp;
		unitsButton.transform.localScale = Vector3.one * smoothedLerp;

		optionsTray.localScale = new Vector3(smoothedLerp, 1f, 1f);

		playtestButton.transform.localScale = Vector3.one * smoothedLerp;

		newButton.transform.localScale = Vector3.one * smoothedLerp;
		saveButton.transform.localScale = Vector3.one * smoothedLerp;
		loadButton.transform.localScale = Vector3.one * smoothedLerp;

		// Playtest in-progress
		endPlaytestButton.transform.localScale = Vector3.one * (1f - smoothedLerp);
		commitPlaytestButton.transform.localScale = Vector3.one * (1f - smoothedLerp);

		holdShiftReminder.transform.localScale = new Vector3(1f, smoothedLerp, 1f);

		MainCameraController.Inst.EditTransition(smoothedLerp);
	} // End of UpdateEditTransitionEffects().


	public async Task DoEditTile(){
		Vector2 lastCursorPos = InputManager.Inst.CursorMapPosition.ToMap2D();

		switch(EditorOptionsTray.Inst.SelectedCategory){

			// Paint terrain
			case EditorOptionCategory.terrain:
				while(Input.GetMouseButton(0)){
					// Get all tiles between the previous position and our current position, so that tiles aren't skipped
					//   over with framerate issues.
					InputManager.Inst.UpdateCursorMapPosition();
					InputManager.Inst.FindHoveredTile();
					Vector2Int[] tilesToPaint = HexMath.GetLineSupercover(lastCursorPos, InputManager.Inst.CursorMapPosition.ToMap2D());
					lastCursorPos = InputManager.Inst.CursorMapPosition.ToMap2D();

					foreach(Vector2Int cell in tilesToPaint){
						HexTile tile = World.GetTile(cell);
						if(tile != null){
							tile.SetTerrainType(EditorOptionsTray.Inst.SelectedTerrainType);
						}
					}
					// TODO: Cache changing tiles together, then run a single update pass on the terrain to avoid unnecessary terrain updates.
					await Task.Yield();
				}
				break;
		}
	} // End of DoEditTile().


	public void Button_New(){

	} // End of Button_New().

	public void Button_Save(){
		SaveLoadManager.Inst.SaveGameMenu();
	} // End of Button_Save().

	public void Button_Load(){
		SaveLoadManager.Inst.LoadGameMenu();
	} // End of Button_Load().

} // End of ScenarioEditor().
