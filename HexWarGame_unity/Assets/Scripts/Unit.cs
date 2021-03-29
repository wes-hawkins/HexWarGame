#pragma warning disable CS4014 // Suppress warning about Task.Run() not being awaited... intended to run on separate thread.

using UnityEngine;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class Unit : MonoBehaviour, IMouseClickable {

	private static List<Unit> allUnits = new List<Unit>(); public static Unit[] GetAllUnits { get { return allUnits.ToArray(); } }
	

	[SerializeField] private UnitDefinition definition = null; public UnitDefinition Definition { get { return definition; } }

	public int alliance { get; private set; } = 0;

	public HexTile occupiedTile { get; private set; } = null;
	public Vector2Int GridPos2 { get { return occupiedTile.GridPos2; } }

	public MapLayer mapLayer { get; private set; } = MapLayer.surface;
	
	private Renderer[] renderers;


	public static Unit SelectedUnit { get; private set; }
	public void Select(){ SelectedUnit = this; } // Select a specific unit.
	public static void DeselectAll(){ SelectedUnit = null; } // Deselect all units.

	public static List<Unit> invalidUnits = new List<Unit>();


	public static async void MainLoop(CancellationToken ct){
		while(!ct.IsCancellationRequested){
			foreach(Unit unit in invalidUnits){
				unit.SetRecolor(new Color(0.8f, 0f, 0f, 0.5f + (Mathf.Cos(Mathf.PI * Time.time * 4f) * 0.5f)));
			}

			await Task.Yield();
		}
	} // End of MainLoop().



	private void Awake() {
		allUnits.Add(this);
		renderers = GetComponentsInChildren<Renderer>();
	} // End of Awake() method.

	public void ManualStart() {
		// Lock unit to the nearest tile on startup.
		float lowestDist = float.MaxValue;
		HexTile closestTile = null;
		foreach(HexTile aHex in World.allTiles){
			var distToHex = Vector3.Distance(transform.position, aHex.WorldPos);
			if(distToHex < lowestDist){
				lowestDist = distToHex;
				closestTile = aHex;
			}
		}

		occupiedTile = closestTile;
		occupiedTile.SetOccupyingUnit(this);
		occupiedTile.TerrainTypeUpdated += UpdateHeight;
		UpdateHeight();

	} // End of ManualStart() method.


	public async Task OnClicked(int mouseButton){
		Select();
		// On right click...
		if(mouseButton == 1){
			HexTile lastHoveredTile = null;
			Vector2Int[] path = new Vector2Int[0];
			CancellationTokenSource pathCTS;
			while(true){
				InputManager.Inst.FindHoveredTile();
				if((InputManager.Inst.HoveredTile != null) && (InputManager.Inst.HoveredTile != lastHoveredTile)){
					lastHoveredTile = InputManager.Inst.HoveredTile;
					// Forget old path and clear any in-progress calcs.
					path = null;
					pathCTS = new CancellationTokenSource();

					// Spin up new path generation.
					Task.Run(() => GeneratePathAsync(occupiedTile, lastHoveredTile, out path, pathCTS.Token));
				}

				if(path != null)
					HexMath.GetTilesArrow(InputManager.Inst.ArrowMesh.mesh, path, 0.35f + (-Mathf.Cos(Time.time * Mathf.PI * 2f)) * 0.025f);
				InputManager.Inst.ArrowMesh.gameObject.SetActive(path != null);

				// TODO: Rewrite this, it's terrible.
				// Confirm move
				if(Input.GetMouseButton(0) && (path != null)){
					occupiedTile.SetOccupyingUnit(null);
					occupiedTile = lastHoveredTile;
					lastHoveredTile.SetOccupyingUnit(this);
					UpdateHeight();
					InputManager.Inst.ArrowMesh.gameObject.SetActive(false);
					return;
				}

				// Cancel move command
				if(Input.GetMouseButton(1)){
					DeselectAll();
					InputManager.Inst.ArrowMesh.gameObject.SetActive(false);
					return;
				}

				await Task.Yield();
			}
		}
	} // End of OnClicked().


	private void GeneratePathAsync(HexTile startTile, HexTile endTile, out Vector2Int[] path, CancellationToken ct){
		path = World.FindPath(startTile, endTile, this, ct);
	} // End of GeneratePathAsync().


	private void UpdateHeight(){
		float height;
		if(occupiedTile.GetIsNavigable(mapLayer, this, out height)){
			transform.position = occupiedTile.WorldPos + (Vector3.up * height);
			SetInvalid(false);
		} else {
			transform.position = occupiedTile.WorldPos;
			SetInvalid(true);
		}
	} // End of UpdateHeight().


	private void SetInvalid(bool invalid){
		if(invalid){
			if(!invalidUnits.Contains(this)){
				invalidUnits.Add(this);
			}
		}else{
			if(invalidUnits.Contains(this)){
				invalidUnits.Remove(this);
				SetRecolor(Color.clear);
			}
		}
	} // End of SetInvalid().


	private void SetRecolor(Color color){
		foreach(Renderer renderer in renderers)
			renderer.material.SetColor("_Recolor", color);
	} // End of SetRecolor() method.


	public float GetMoveCost(HexTile tile){
		return 1f;
	} // End of GetMoveCost() method.

} // End of Unit class.