using UnityEngine;
using System.Collections.Generic;

public class Unit : MonoBehaviour{

	private static List<Unit> allUnits = new List<Unit>(); public static Unit[] GetAllUnits { get { return allUnits.ToArray(); } }
	

	public int alliance { get; private set; } = 0;

	public HexTile occupiedHex { get; private set; } = null;
	
	public int moveSpeed { get; private set; } = 5; // Number of hexes this unit moves per action. Affects move animation
	public int moveSpeedActual { get; private set; } = 5; // 'Current' move speed, accounting for weight, etc.

	public static Unit SelectedUnit { get; private set; }
	public void Select(){
		SelectedUnit = this;
	} // End of Select() method.


	private void Awake() {
		allUnits.Add(this);
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
		occupiedHex = closestTile;
		transform.position = occupiedHex.WorldPos;

	
		/*
		// Moving
		if(moveOrder.Length > 0){
			GameManager.busy = true;
		
			occupiedHex.SetOccupyingUnit(null);
			occupiedHex = moveOrder[0];
	
			transform.position = Vector3.Lerp(previousHex.transform.position, moveOrder[0].transform.position, moveLerp);
			transform.eulerAngles = new Vector3(0f, Mathf.Atan2(( moveOrder[0].transform.position.x - previousHex.transform.position.x ), ( moveOrder[0].transform.position.z - previousHex.transform.position.z )) * Mathf.Rad2Deg, 0f);
			moveLerp = Mathf.MoveTowards(moveLerp, 1, Time.deltaTime * moveSpeedActual * 2);
		
			if(moveLerp == 1){
				// Shift the previous hex out of the moveOrder
				HexTile[] newMoveOrder = new HexTile[moveOrder.Length - 1];
				for(int i = 0; i < moveOrder.Length - 1; i++)
					newMoveOrder[i] = moveOrder[i + 1];
				
				previousHex = moveOrder[0];
				moveOrder = newMoveOrder;
			
				moveLerp = 0;
			}
		}
		*/

	} // End of ManualStart() method.
	
} // End of Unit class.