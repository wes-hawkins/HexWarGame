using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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




    private void Awake(){
        Inst = this;
    } // End of Start() method.


    // Main thread
    public async void Init(){
        arrowMesh.gameObject.SetActive(false);
        hoveredCellMesh.gameObject.SetActive(false);
        selectedUnitMesh.gameObject.SetActive(false);

        while(true){

            // Check for input-interrupting actions here (movement, combat, etc.)
            // ...

            FindHoveredTile();

            // Read player input.
            if(Input.GetMouseButton(0) || Input.GetMouseButton(1)){
                int mouseButton = 0;
                if(Input.GetMouseButton(1))
                    mouseButton = 1;

                IMouseDraggable mouseDraggable = null;
                IMouseClickable mouseClickable = null;

                Vector3 initialMousePos = Input.mousePosition;
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
			                if(HoveredTile.occupyingUnit != null){
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

            await Task.Yield();
        }
    } // End of Init().


    public void FindHoveredTile(){
        // Find hovered tile
		Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
		Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
		float rayDist;
		if(groundPlane.Raycast(mouseRay, out rayDist)){
			CursorMapPosition = mouseRay.origin + (mouseRay.direction * rayDist);
			Vector2Int roundedPos = HexMath.WorldToHexGrid(CursorMapPosition);
			HoveredTile = World.GetTile(roundedPos);
		}
    } // End of FindHoveredTile().


	private void Update() {
        // Animate tiles (away from input thread; this is fine.)
        if(HoveredTile != null)
    		HexMath.GetTilesFill(hoveredCellMesh.mesh, new Vector2Int[]{ InputManager.Inst.HoveredTile.GridPos2 }, (GUIConfig.GridOutlineThickness * -0.5f));
        hoveredCellMesh.gameObject.SetActive(HoveredTile != null);

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