using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Serves compartmentalized buttons for user-editable features and entities in the editor screen.
public class EditorOptionsTray : MonoBehaviour {

    public static EditorOptionsTray Inst { get; private set; }

    private RectTransform rectTransform;
    
    [SerializeField] private GameObject optionTrayItemSource = null;
    [SerializeField] private GameObject optionTrayCategorySource = null;
    [SerializeField] private EnableDisableActions scrollbarEnableDisable = null;

    private Transform terrainOptionsContainer = null;
    private Transform unitsOptionsContainer = null;
    private Transform structuresOptionsContainer = null;

    [Space]
    [SerializeField] private float noScrollWidth = 134f; // How wide the options tray is without scrollbar enabled.
    [SerializeField] private float withScrollWidth = 134f; // How wide the options tray is with scrollbar enabled.


	private void Awake() {
        Inst = this;
        rectTransform = GetComponent<RectTransform>();

        scrollbarEnableDisable.OnEnabled += SetWidthWithScrollbar;
        scrollbarEnableDisable.OnDisabled += SetWidthWithScrollbar;
    } // End of Awake().


    public void ManualStart(){

        optionTrayItemSource.transform.SetParent(null);

        terrainOptionsContainer = optionTrayCategorySource.transform;
        structuresOptionsContainer = Instantiate(optionTrayCategorySource, optionTrayCategorySource.transform.parent).transform;
        unitsOptionsContainer = Instantiate(optionTrayCategorySource, optionTrayCategorySource.transform.parent).transform;

        terrainOptionsContainer.name = "Group: Terrain";
        structuresOptionsContainer.name = "Group: Structures";
        unitsOptionsContainer.name = "Group: Units";

        // Set up terrain options
        foreach(TerrainConfig.TerrainTypeUIData terrainData in TerrainConfig.Inst.TerrainData){
            OptionTrayItem newItem = Instantiate(optionTrayItemSource, terrainOptionsContainer).GetComponent<OptionTrayItem>();
            newItem.Init(terrainData.Name, terrainData.Sprite);
        }

        // Set up structure options
        // ...

        // Set up unit options
        // ...
        
        // Cleanup
        optionTrayItemSource.transform.SetParent(optionTrayCategorySource.transform);
        optionTrayItemSource.gameObject.SetActive(false);

	} // End of Awake().


    private void SetWidthWithScrollbar(){
        rectTransform.sizeDelta = new Vector2(scrollbarEnableDisable.gameObject.activeSelf? withScrollWidth : noScrollWidth, 0f);
        rectTransform.sizeDelta = new Vector2(scrollbarEnableDisable.gameObject.activeSelf? withScrollWidth : noScrollWidth, 0f);
    } // End of SetWidthWithScrollbar().

} // End of EditorOptionsTray class.
