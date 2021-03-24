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



    private void Awake(){
        Inst = this;
    } // End of Start() method.


    // Main thread
    public async void Init(){
        while(true){

            // Check for input-interrupting actions here (movement, combat, etc.)
            // ...

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
                        if(mouseClickable == null)
                            mouseClickable = GameManager.Inst;
                        Debug.Log("Clicked on " + ((mouseClickable != null)? mouseClickable.name : "nothing") + ".");
                        if(mouseClickable != null)
                            mouseClickable.OnClicked(mouseButton);
                        break;
                    }

                    await Task.Yield();
                }
            }

            await Task.Yield();
        }
    } // End of Start().


    private struct MouseEvent {
        public GameObject Object;
        public MouseEventType EventType;

        public MouseEvent(GameObject target, MouseEventType eventType){
            Object = target;
            EventType = eventType;
        } // End of constructor.

    } // End of MouseEvent struct.

} // End of InputManager class.


public enum MouseEventType {
    click,
    beginDrag,
    drag,
    endDrag,
}



public interface IMouseClickable {
    string name { get; }
    void OnClicked(int mouseButton);
} // End of IMouseClickable interface.

public interface IMouseDraggable {
    string name { get; }
    Task Drag(int mouseButton);
} // End of IMouseDraggable interface.