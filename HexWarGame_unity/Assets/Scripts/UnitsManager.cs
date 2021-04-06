using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitsManager : MonoBehaviour {

    public static UnitsManager Inst = null;

    [SerializeField] private GameObject unitInfoBadgeSource = null;
    
    private Dictionary<Unit, UnitInfoBadge> unitBadgeDict = new Dictionary<Unit, UnitInfoBadge>();


	private void Awake() {
        Inst = this;
        unitInfoBadgeSource.gameObject.SetActive(false);
    } // End of Awake() method.


    public void ManualStart(){
	    // Initialize movement schemes
	    UnitMovementScheme[] moveSchemes = Resources.LoadAll<UnitMovementScheme>("Movement Schemes");
        foreach(UnitMovementScheme moveScheme in moveSchemes)
            moveScheme.Init();

        // Initialize unit selection/info
        GameObject[] units = Resources.LoadAll<GameObject>("Units");
        // TODO: 'Init' each unit prefab, take a mugshot, populate selection list.
        // ...

    } // End of Awake().


    public void RegisterUnitBadge(Unit unit){
        UnitInfoBadge newBadge = Instantiate(unitInfoBadgeSource, unitInfoBadgeSource.transform.parent).GetComponent<UnitInfoBadge>();
        newBadge.gameObject.SetActive(true);
        newBadge.Init(unit);
        unitBadgeDict.Add(unit, newBadge);
    } // End of RegisterUnitBadge().


    private void Update(){
        foreach(KeyValuePair<Unit, UnitInfoBadge> kvp in unitBadgeDict){
            Unit thisUnit = kvp.Key;
            UnitInfoBadge thisBadge = kvp.Value;

            Vector3 cameraGroundVector = Vector3.ProjectOnPlane(-Camera.main.transform.forward, Vector3.up).normalized;

            Vector3 unitViewportPoint = Camera.main.WorldToViewportPoint(thisUnit.transform.position + (cameraGroundVector * 0.5f));
            thisBadge.RectTransform.anchoredPosition = unitViewportPoint * GameManager.Inst.MainCanvas.renderingDisplaySize;
            thisBadge.RectTransform.localScale = Vector3.one * (1f / unitViewportPoint.z) * GUIConfig.UnitBadgeScale;
        }
    } // End of Update().

} // End of UnitsManager class.
