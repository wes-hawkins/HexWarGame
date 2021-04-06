using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UnitInfoBadge : MonoBehaviour {

    [SerializeField] private TextMeshProUGUI hitpointsText;
    [SerializeField] private TextMeshProUGUI movementPowerText;
	public RectTransform RectTransform { get; private set; }


	private void Awake() {
		RectTransform = GetComponent<RectTransform>();
	} // End of Awake().


	public void Init(Unit unit){
		unit.HitpointsChanged += OnHitpointsChanged;
		unit.MovePowerChanged += OnMovementPowerChanged;
	} // End of Init() method.



	private void OnHitpointsChanged(int newHitpoints){
        hitpointsText.SetText(newHitpoints.ToString());
    } // End of OnHitpointsChanged() method.


	private void OnMovementPowerChanged(float newMovementPower){
        movementPowerText.SetText(Mathf.FloorToInt(newMovementPower * 100f) + "%");
    } // End of OnMovementPowerChanged() method.
    
} // End of UnitInfoBadge class.
