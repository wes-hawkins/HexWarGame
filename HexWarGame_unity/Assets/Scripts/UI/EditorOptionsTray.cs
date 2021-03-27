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


    // 0 = terrain, 1 = structures, 2 = units
    private int selectedCategory = 0;
    private int[] selectedOption = new int[3]; // Keep track of selected option in each list.
    private List<OptionTrayItem>[] optionLists = new List<OptionTrayItem>[3];

    [Space]
    [SerializeField] private Material normalTextMaterial = null; public Material NormalTextMaterial { get { return normalTextMaterial; } }
    [SerializeField] private Material selectedTextMaterial = null; public Material SelectedTextMaterial { get { return selectedTextMaterial; } }


	private void Awake() {
        Inst = this;
        rectTransform = GetComponent<RectTransform>();

        scrollbarEnableDisable.OnEnabled += SetWidthWithScrollbar;
        scrollbarEnableDisable.OnDisabled += SetWidthWithScrollbar;

        foreach(int i in selectedOption){
            selectedOption[i] = -1; // -1 indicated nothing selected.
            optionLists[i] = new List<OptionTrayItem>();
        }
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
        for(int i = 0; i < TerrainConfig.Inst.TerrainData.Length; i++){
            TerrainConfig.TerrainTypeUIData terrainData = TerrainConfig.Inst.TerrainData[i];

            OptionTrayItem newItem = Instantiate(optionTrayItemSource, terrainOptionsContainer).GetComponent<OptionTrayItem>();
            newItem.Init(terrainData.Name, terrainData.Sprite);
            optionLists[0].Add(newItem);
            int j = i;
            newItem.Button.onClick.AddListener(delegate{ OptionSelected(0, j); });
            Debug.Log("Button set up " + j);
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


    private void OptionSelected(int category, int optionNum){
        Debug.Log("OptionSelected(" + category + ", " + optionNum + ")");

        // Clear existing selection.
        int previousOption = selectedOption[selectedCategory];
        if(previousOption != -1)
            optionLists[selectedCategory][previousOption].Deselect();

        selectedCategory = category;
        selectedOption[category] = optionNum;
        optionLists[category][optionNum].Select();

    } // End of OptionSelected().

} // End of EditorOptionsTray class.
