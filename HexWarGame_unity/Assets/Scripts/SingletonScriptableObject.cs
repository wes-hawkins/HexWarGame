using UnityEngine;

// Abstract class for making reload-proof singletons out of ScriptableObjects
public abstract class SingletonScriptableObject<T> : ScriptableObject where T : ScriptableObject {  

    public static T Inst = null;

	public virtual void Init() {
		Inst = this as T;
	} // End of Init()

} // End of SingletonScriptableObject.