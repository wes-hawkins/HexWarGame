using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using TMPro;

public class InputManager : MonoBehaviour {

    public static InputManager Inst { get; private set; }


    public Action PrimaryClick; // On mouse up, without dragging.
    public Action PrimaryDragStart;
    public Action PrimaryDragStop;

    public Action SecondaryClick; // // On mouse up, without dragging.
    public Action SecondaryDragStart;
    public Action SecondaryDragStop;

    private float mouseDragThreshold = 3f;

    // Where on the map the cursor is.
	public Vector3 CursorMapPosition { get; private set; }

	public HexTile HoveredTile { get; private set; } = null;
	[SerializeField] private MeshFilter hoveredCellMesh = null;
	[SerializeField] private MeshFilter selectedUnitMesh = null;

	[SerializeField] private MeshFilter arrowMesh = null; public MeshFilter ArrowMesh { get { return arrowMesh; } }

	[SerializeField] private TextMeshProUGUI hoveredTileInfo = null;




    private void Awake(){
        Inst = this;

        arrowMesh.gameObject.SetActive(false);
        hoveredCellMesh.gameObject.SetActive(false);
        selectedUnitMesh.gameObject.SetActive(false);
    } // End of Start() method.


    // Main thread
    public async void MainLoop(CancellationToken ct){
        while(!ct.IsCancellationRequested){

            FindHoveredTile();

            // Read player input.
            if(!UIPointerMinder.HoveredElement && (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))){
                int mouseButton = 0;
                if(Input.GetMouseButton(1))
                    mouseButton = 1;

                IMouseDraggable mouseDraggable = null;
                IMouseClickable mouseClickable = null;

                Vector3 initialMousePos = Input.mousePosition;

                // If edit mode is hot, do that.
                if(ScenarioEditor.Inst.HotControl){
                    await ScenarioEditor.Inst.DoEditTile();
                } else {
                    while(true){

                        // Start drag if dragging.
                        if(Vector3.Distance(initialMousePos, Input.mousePosition) > mouseDragThreshold){
                            if(mouseDraggable == null)
                                mouseDraggable = MainCameraController.Inst;
                            await mouseDraggable.Drag(mouseButton);
                            break;
                        }
            
                        // Confirm click (on release)
                        if(!Input.GetMouseButton(mouseButton)){
                            if(mouseClickable != null)
                                await mouseClickable.OnClicked(mouseButton);
                            else{
			                    if((HoveredTile.occupyingUnit != null) && (GameManager.Inst.GameMode == GameMode.play)){
				                    await HoveredTile.occupyingUnit.OnClicked(mouseButton);
			                    } else {
				                    selectedUnitMesh.gameObject.SetActive(false);
				                    Unit.DeselectAll();
			                    }
                            }
                            break;
                        }

                        await Task.Yield();
                    }
                }
            }

            await Task.Yield();
        }
    } // End of Init().


    public void UpdateCursorMapPosition(){
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
		Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
		float rayDist;
		if(groundPlane.Raycast(mouseRay, out rayDist))
			CursorMapPosition = mouseRay.origin + (mouseRay.direction * rayDist);
    } // End of UpdateCursorMapPosition().


    public void FindHoveredTile(){
        // Find hovered tile
        if(!UIPointerMinder.HoveredElement){
		    UpdateCursorMapPosition();
			Vector2Int roundedPos = HexMath.WorldToHexGrid(CursorMapPosition);
			HoveredTile = World.GetTile(roundedPos);
        } else {
            HoveredTile = null;
        }
    } // End of FindHoveredTile().


    public void ClearHoveredTile(){
        HoveredTile = null;
    } // End of ClearHoveredTile().


	private void Update() {
        // Animate tiles (away from input thread; this is fine.)
        if(HoveredTile != null){
    		HexMath.GetTilesFill(hoveredCellMesh.mesh, new Vector2Int[]{ InputManager.Inst.HoveredTile.GridPos2 }, (GUIConfig.GridOutlineThickness * -0.5f));

            hoveredTileInfo.rectTransform.position = Camera.main.WorldToScreenPoint(HexMath.HexGridToWorld(HoveredTile.GridPos2));
            //hoveredTileInfo.SetText(HoveredTile.GridPos3.x + ", " + HoveredTile.GridPos3.y + ", " + HoveredTile.GridPos3.z);
            hoveredTileInfo.SetText(HoveredTile.GridPos2.x + ", " + HoveredTile.GridPos2.y);
        }
        hoveredCellMesh.gameObject.SetActive(HoveredTile != null);
        hoveredTileInfo.gameObject.SetActive(HoveredTile != null);

		// Selected unit outline effect
		if(Unit.SelectedUnit != null)
			HexMath.GetTilesOutline(selectedUnitMesh.mesh, new Vector2Int[]{ Unit.SelectedUnit.GridPos2 }, 0.15f + (Mathf.Cos(Time.time * Mathf.PI * 2f) * 0.05f), 0.035f);
		selectedUnitMesh.gameObject.SetActive(Unit.SelectedUnit != null);

	} // End of Update().

} // End of InputManager class.


public interface IMouseClickable {
    string name { get; }
    Task OnClicked(int mouseButton);
} // End of IMouseClickable interface.


public interface IMouseDraggable {
    string name { get; }
    Task Drag(int mouseButton);
} // End of IMouseDraggable interface.