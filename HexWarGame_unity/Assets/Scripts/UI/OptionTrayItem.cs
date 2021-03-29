using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OptionTrayItem : MonoBehaviour {
    
    [SerializeField] private TextMeshProUGUI labelText;
    [SerializeField] private Image background;
    [SerializeField] private Image graphic;
    [SerializeField] private Transform indenter;
    [SerializeField] private Button button; public Button Button { get { return button; } }

    private const float indent = 15f;


    public void Init(string label, Sprite icon){
        labelText.SetText(label);
        graphic.overrideSprite = icon;
        name = "Option: " + label;
    } // End of Init() method.


    public void Deselect(){
        indenter.localPosition = Vector3.zero;
        labelText.color = Color.white;
        labelText.fontSharedMaterial = EditorOptionsTray.Inst.NormalTextMaterial;
        background.color = Color.black;
    } // End of Deselect().


    public void Select(){
        indenter.localPosition = Vector3.right * indent;
        labelText.color = Color.black;
        labelText.fontSharedMaterial = EditorOptionsTray.Inst.SelectedTextMaterial;
        background.color = Color.white;
    } // End of Deselect().

} // End of OptionTrayItem class.
