using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "WarGame/UnitDefinition")]
public class UnitDefinition : ScriptableObject {

	[SerializeField] private string unitName = "New Unit"; public string UnitName { get { return unitName; } }
	[SerializeField] private UnitMovementScheme moveScheme = null; public UnitMovementScheme MoveScheme { get { return moveScheme; } }
} // End of UnitDefinition.
