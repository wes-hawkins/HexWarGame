using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "WarGame/UnitDefinition")]
public class UnitDefinition : ScriptableObject {

	[SerializeField] private string unitName = "New Unit"; public string UnitName { get { return unitName; } }
	[SerializeField] private int movePower = 10; public int MovePower { get { return movePower; } }

	[Space]
	[Tooltip("Unit can traverse on mountains.")]
	[SerializeField] private bool canTraverseMountains = false; public bool CanTraverseMountains { get { return canTraverseMountains; } }

	[Tooltip("Unit can traverse on open ground.")]
	[SerializeField] private bool canTraverseOpenGround = false; public bool CanTraverseOpenGround { get { return canTraverseOpenGround; } }

	[Tooltip("Unit can traverse the surface of shallow water.")]
	[SerializeField] private bool canFloatShallowSurface = false; public bool CanFloatShallowSurface { get { return canFloatShallowSurface; } }

	[Tooltip("Unit can traverse on the bed under shallow water.")]
	[SerializeField] private bool canTraverseShallowBed = false; public bool CanTraverseShallowBed { get { return canTraverseShallowBed; } }

	[Tooltip("Unit can traverse the surface of deep water.")]
	[SerializeField] private bool canFloatDeepSurface = false; public bool CanFloatDeepSurface { get { return canFloatDeepSurface; } }

	[Tooltip("Unit can traverse on the sea floor.")] // Applies to subs as well.
	[SerializeField] private bool canTraverseDeepBed = false; public bool CanTraverseDeepBed { get { return canTraverseDeepBed; } }

} // End of UnitDefinition.
