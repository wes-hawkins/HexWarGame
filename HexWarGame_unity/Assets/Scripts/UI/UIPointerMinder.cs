using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// While the intention is to use the UGUI system as much as possible, this script detects when the cursor
//   if over a UGUI element so that map-based user inputs know when to back off.
public class UIPointerMinder : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    
    public static UIPointerMinder HoveredElement { get; private set; }


    public void OnPointerEnter(PointerEventData data){
        HoveredElement = this;
    } // End of IPointerEnter().

    public void OnPointerExit(PointerEventData data){
        if(HoveredElement == this)
            HoveredElement = null;
    } // End of IPointerEnter().

} // End of UIPointerMinder class.
