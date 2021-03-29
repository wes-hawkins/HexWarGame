using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FileBrowserEntry : MonoBehaviour {

    [SerializeField] private TextMeshProUGUI label = null;
    [SerializeField] private Image icon = null;
    [SerializeField] private Button button = null; public Button Button { get { return button; } }


    public void SetLabel(string text){
        label.SetText(text);
    } // End of SetLabel() method.

    public void SetIcon(Sprite sprite){
        icon.overrideSprite = sprite;
    } // End of SetIcon() method.
    
} // End of FileBrowserEntry class.
