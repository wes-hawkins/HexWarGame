using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utilities {

	public static Vector2 ToMap2D(this Vector3 vec){
		return new Vector2(vec.x, vec.z);
	} // End of ToMap2D extensions method.

	public static Vector3 MapTo3D(this Vector2 vec){
		return new Vector3(vec.x, 0f, vec.y);
	} // End of ToMap2D extensions method.


	// Calculate the distance between point pt and the segment p1 --> p2.
    public static float FindDistanceToSegment(Vector2 pt, Vector2 p1, Vector2 p2){
        Vector2 closestPoint = new Vector2();

        float dx = p2.x - p1.x;
        float dy = p2.y - p1.y;
        if((dx == 0) && (dy == 0)){
            // It's a point not a line segment.
            closestPoint = p1;
            dx = pt.x - p1.x;
            dy = pt.y - p1.y;
            return Mathf.Sqrt(dx * dx + dy * dy);
        }

        // Calculate the t that minimizes the distance.
        float t = ((pt.x - p1.x) * dx + (pt.y - p1.y) * dy) /
            (dx * dx + dy * dy);

        // See if this represents one of the segment's
        // end points or a point in the middle.
        if (t < 0){
            closestPoint = new Vector2(p1.x, p1.y);
            dx = pt.x - p1.x;
            dy = pt.y - p1.y;
        }else if (t > 1){
            closestPoint = new Vector2(p2.x, p2.y);
            dx = pt.x - p2.x;
            dy = pt.y - p2.y;
        }else{
            closestPoint = new Vector2(p1.x + t * dx, p1.y + t * dy);
            dx = pt.x - closestPoint.x;
            dy = pt.y - closestPoint.y;
        }

        return Mathf.Sqrt(dx * dx + dy * dy);
    } // End of FindDistanceToSegment() method.

} // End of Utilities class.
