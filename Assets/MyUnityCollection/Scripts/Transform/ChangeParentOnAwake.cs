using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeParentOnAwake : MonoBehaviour {
  [Tooltip("Can be null for scene root")]
  public Transform parent;
  public bool OnStart;
  // Start is called before the first frame update
  void Awake() {
    if (OnStart) return;
    transform.parent = parent;
  }
  void Start() {
    if (!OnStart) return;
    transform.parent = parent;
  }
}
