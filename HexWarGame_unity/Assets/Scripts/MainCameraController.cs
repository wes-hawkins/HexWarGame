using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCameraController : MonoBehaviour, IMouseDraggable {

    public static MainCameraController Inst { get; private set; }

    private abstract class CameraBehavior : IMouseDraggable {
        public abstract Vector3 CameraPosition { get; }
        public abstract Quaternion CameraRotation { get; }

        public abstract void Frame();
        public abstract Task Drag(int mouseButton);

        public abstract string name { get; }
    } // End of CameraBehavior.


    // Bread n' butter camera that can pan around the map, zoom, tilt, etc.
    private class StrategicCamera : CameraBehavior {
        private float easing = 0.1f;

        private float eulersRate = 3f;
        private Vector2 eulers = new Vector2(45f, 20f);
        private Vector2 targetEulers;
        private Vector2 eulersVel = Vector2.zero;
        float minTilt = 20;
        float maxTilt = 80;

        private float zoomRate = 0.2f;
        private float targetZoomDist = 10f;
        private float zoomDist = 10f;
        private float zoomDistVel = 0f;
        private float minZoomDist = 2f;
        private float maxZoomDist = 20f;

        private float slewRate = 1f; // Multiplied by zoom!
        private Vector3 focusPosition = Vector3.zero;
        private Vector3 targetFocusPosition = Vector3.zero;
        private Vector3 focusPositionVel = Vector3.zero;

        public override string name { get { return "Strategic Camera"; } }

        public StrategicCamera(){
            targetEulers = eulers;
        } // End of constructor.


		public override Vector3 CameraPosition { get { 
            return focusPosition + (Quaternion.Euler(eulers) * -Vector3.forward * zoomDist);
        } } // End of CameraPosition getter.

		public override Quaternion CameraRotation { get {
            return Quaternion.Euler(eulers);
        } } // End of CameraRotation getter.


		public override void Frame() {
            // Translation
			Vector3 groundForward = Vector3.ProjectOnPlane(Quaternion.Euler(eulers) * Vector3.forward, Vector3.up).normalized;
            Vector2 moveThrottle = Vector2.zero;
            if(Input.GetKey(KeyCode.W))
                moveThrottle.y += 1f;
            if(Input.GetKey(KeyCode.S))
                moveThrottle.y -= 1f;
            if(Input.GetKey(KeyCode.A))
                moveThrottle.x -= 1f;
            if(Input.GetKey(KeyCode.D))
                moveThrottle.x += 1f;

            moveThrottle = Vector2.ClampMagnitude(moveThrottle, 1f);
            if(Input.GetKey(KeyCode.LeftShift))
                moveThrottle *= 3f;
            targetFocusPosition += ((groundForward * moveThrottle.y) + (-Vector3.Cross(groundForward, Vector3.up).normalized * moveThrottle.x)) * slewRate * Time.deltaTime * zoomDist;

            // Zoom
            float zoomInput = Input.mouseScrollDelta.y * -zoomRate;
            float zoomDistDelta = Mathf.Clamp(zoomInput * targetZoomDist, minZoomDist - targetZoomDist, maxZoomDist - targetZoomDist);

            float initialTargetZoomDist = targetZoomDist;
            // Scoot view towards/away from cursor position while zooming to maintain cursor map position.
            targetFocusPosition = Vector3.MoveTowards(targetFocusPosition, InputManager.Inst.CursorMapPosition, -Vector3.Distance(targetFocusPosition, InputManager.Inst.CursorMapPosition) * (zoomDistDelta / targetZoomDist));

            targetZoomDist = Mathf.Clamp(targetZoomDist + zoomDistDelta, minZoomDist, maxZoomDist);
            zoomDist = Mathf.SmoothDamp(zoomDist, targetZoomDist, ref zoomDistVel, easing);

            // Rotation
            eulers = Vector2.SmoothDamp(eulers, targetEulers, ref eulersVel, easing);
            focusPosition = Vector3.SmoothDamp(focusPosition, targetFocusPosition, ref focusPositionVel, easing);

		} // End of Frame() method.


		public override async Task Drag(int mouseButton) {
            switch(mouseButton){
                case 0: await DragSlew(); break;
                case 1: await DragRotate(); break;
            }
        } // End of Drag().

        private async Task DragSlew(){
		    Vector3 cameraLocalCursorStart = Camera.main.transform.InverseTransformPoint(InputManager.Inst.CursorMapPosition);
		    Vector3 cameraLocalUpNormal = Camera.main.transform.InverseTransformVector(Vector3.up);
            Vector3 initialTargetPosition = focusPosition;
		    Vector3 lastMousePosition = Vector2.zero;
            while(Input.GetMouseButton(0)){
			    Plane localStartPlane = new Plane(Camera.main.transform.TransformVector(cameraLocalUpNormal), Camera.main.transform.TransformPoint(cameraLocalCursorStart));
			    float mouseRayDist;
			    Ray newMouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
			    if((Input.mousePosition != lastMousePosition) && localStartPlane.Raycast(newMouseRay, out mouseRayDist)){
				    Vector3 newStartPlaneHit = newMouseRay.origin + (newMouseRay.direction * mouseRayDist);
				    targetFocusPosition = initialTargetPosition + (Camera.main.transform.TransformPoint(cameraLocalCursorStart) - newStartPlaneHit);
				    lastMousePosition = Input.mousePosition;
			    }
                await Task.Yield();
            }
        } // End of DragSlew().

        private async Task DragRotate(){
            while(Input.GetMouseButton(1)){
			    targetEulers += new Vector2(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X")) * eulersRate;
                targetEulers.x = Mathf.Clamp(targetEulers.x, minTilt, maxTilt);
                await Task.Yield();
            }
        } // End of DragRotate().

	} // End of StrategicCamera class.


    private StrategicCamera strategicCamera = new StrategicCamera();


	private void Awake() {
		Inst = this;
	} // End of Awake().


	public void Frame(){
        strategicCamera.Frame();
        transform.position = strategicCamera.CameraPosition;
        transform.rotation = strategicCamera.CameraRotation;
    } // End of ManualUpdate() method.



    public async Task Drag(int mouseButton){
        await strategicCamera.Drag(mouseButton);
    } // End of BeginDrag() method.

    
} // End of MainCameraController class.
