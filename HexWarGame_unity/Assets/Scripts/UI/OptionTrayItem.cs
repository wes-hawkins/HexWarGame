using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OptionTrayItem : MonoBehaviour {
    
    [SerializeField] private TextMeshProUGUI labelText;
    [SerializeField] private Image graphic;
    [SerializeField] private Transform indenter;


    public void Init(string label, Sprite icon){
        labelText.SetText(label);
        graphic.overrideSprite = icon;
        name = "Option: " + label;
    } // End of Init() method.

} // End of OptionTrayItem class.
