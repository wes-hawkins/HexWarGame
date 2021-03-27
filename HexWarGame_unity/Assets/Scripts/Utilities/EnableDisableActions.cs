using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Utility for alerting other scripts to when this component's GameObject is enabled or disabled.
public class EnableDisableActions : MonoBehaviour {

    public Action OnEnabled;
    public Action OnDisabled;


	private void OnEnable() {
		OnEnabled?.Invoke();
	} // End of OnEnabled() method.

	private void OnDisable() {
		OnDisabled?.Invoke();
	} // End of OnDisabled() method.

} // End of EnableDisableActions class.
