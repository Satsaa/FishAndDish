using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lure : MonoBehaviour {
  [HideInInspector]
  public FishBehaviour2D inRange;
  [HideInInspector]
  public FishBehaviour2D attached;
  public DistanceJoint2D joint;
  // Start is called before the first frame update
  void Start() {

  }

  // Update is called once per frame
  void Update() {
    if (attached != null && joint.attachedRigidbody == null) {
      attached = null;
      inRange = null;
    }
  }

  void OnTriggerEnter2D(Collider2D col) {
    var fb = col.gameObject.GetComponent<FishBehaviour2D>();
    if (fb == null) return;
    inRange = fb;
  }

  public void Attach() {
    if (inRange != null && attached == null) {
      attached = inRange;
    }
  }
}
