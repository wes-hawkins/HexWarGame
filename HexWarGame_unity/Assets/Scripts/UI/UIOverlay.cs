using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIOverlay : MonoBehaviour {

    public static UIOverlay Inst { get; private set; }
    private Image image;


    private void Awake() {
        Inst = this;
        image = GetComponent<Image>();
    } // End of Awake() method.


    public void Show(bool show){
        image.enabled = show;
    } // End of Show().

} // End of UIOverlay class.
