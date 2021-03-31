using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitsManager : MonoBehaviour {

    private void Start() {
        GameObject[] units = Resources.LoadAll<GameObject>("Units");
        Debug.Log(units.Length);

        // TODO: 'Init' each unit, take a mugshot, populate selection list.

    } // End of Start().

} // End of UnitsManager class.
