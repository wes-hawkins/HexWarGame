#pragma warning disable CS4014 // Suppress warning about Task.Run() not being awaited... intended to run on separate thread.

using System;
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

	public int Hitpoints { get; private set; } = 10;
	public float MovePower { get; private set; } = 1f;

	public Action<int> HitpointsChanged;
	public Action<float> MovePowerChanged;

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

		UnitsManager.Inst.RegisterUnitBadge(this);

	} // End of ManualStart() method.


	public async Task OnClicked(int mouseButton){
		Select();
		// On right click...
		if(mouseButton == 1){
			HexTile lastHoveredTile = null;
			HexPath path = null;
			CancellationTokenSource pathCTS;

			// Draw valid move tiles
			CancellationTokenSource validMoveCTS = new CancellationTokenSource();

			Vector2Int[] validMoveTiles = HexNavigation.FindValidMoves(occupiedTile, this, out Vector2Int[] passThroughOnlyTiles, validMoveCTS.Token);
			InputManager.Inst.ValidMoveTilesFillMesh.gameObject.SetActive(true);
			InputManager.Inst.PassThroughOnlyTilesMesh.gameObject.SetActive(true);

			// Update path logic
			while(true){
				// Animate valid moves outline
				//HexMath.GetTilesOutline(InputManager.Inst.ValidMoveTilesOutlineMesh.mesh, validMoveTiles, 0.1f, 0.15f + (-Mathf.Cos(Time.time * Mathf.PI * 2f)) * 0.05f);
				HexMath.GetTilesFill(InputManager.Inst.ValidMoveTilesFillMesh.mesh, validMoveTiles, Mathf.Lerp(0f, -0.1f, GameManager.UIPulsar), true);
				HexMath.GetTilesFill(InputManager.Inst.PassThroughOnlyTilesMesh.mesh, passThroughOnlyTiles, Mathf.Lerp(-0.3f, -0.2f, GameManager.UIPulsar), false);

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
					HexMath.GetTilesArrow(InputManager.Inst.ArrowMesh.mesh, path.Cells, Mathf.Lerp(0.25f, 0.3f, GameManager.UIPulsar));
				InputManager.Inst.ArrowMesh.gameObject.SetActive(path != null);

				// TODO: Rewrite this, it's terrible.
				// Confirm move
				if(Input.GetMouseButton(0) && (path != null)){

					InputManager.Inst.ArrowMesh.gameObject.SetActive(false);
					InputManager.Inst.ValidMoveTilesFillMesh.gameObject.SetActive(false);
					InputManager.Inst.PassThroughOnlyTilesMesh.gameObject.SetActive(false);

					// Step through the move command
					for(int i = 0; i < (path.Tiles.Length - 1); i++){
						// Remove from previous tile.
						occupiedTile.SetOccupyingUnit(null);

						// Move to next
						float initialMovePower = MovePower;
						float finalMovePower = MovePower - path.GetStepCost(i);
						for(float t = 0; t < 1f; t = Mathf.MoveTowards(t, 1f, Time.deltaTime * 3f)){
							Vector3 position;
							Quaternion rotation;
							path.GetStepAnimation(i, t, out position, out rotation);

							transform.position = position;
							transform.rotation = rotation;

							MovePower = Mathf.Lerp(initialMovePower, finalMovePower, t);
							MovePowerChanged(MovePower);

							await Task.Yield();
						}

						occupiedTile = path.Tiles[i + 1];
						occupiedTile.SetOccupyingUnit(this);
						UpdateHeight();
					}
					return;
				}

				// Cancel move command
				if(Input.GetMouseButton(1)){
					DeselectAll();
					InputManager.Inst.ArrowMesh.gameObject.SetActive(false);
					InputManager.Inst.ValidMoveTilesFillMesh.gameObject.SetActive(false);
					InputManager.Inst.PassThroughOnlyTilesMesh.gameObject.SetActive(false);
					return;
				}

				await Task.Yield();
			}
		}
	} // End of OnClicked().


	private void GeneratePathAsync(HexTile startTile, HexTile endTile, out HexPath path, CancellationToken ct){
		path = HexNavigation.FindPath(startTile, endTile, this, ct);
	} // End of GeneratePathAsync().





	private void UpdateHeight(){
		float height = 0f;
		if(definition.MoveScheme.GetCanNavigate(occupiedTile.TerrainType)){
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
		if(!Application.isPlaying) return;

		foreach(Renderer renderer in renderers)
			renderer.material.SetColor("_Recolor", color);
	} // End of SetRecolor() method.

} // End of Unit class.