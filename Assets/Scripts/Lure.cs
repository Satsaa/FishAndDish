using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lure : MonoBehaviour {
  [HideInInspector]
  public FishBehaviour2D attached;
  public DistanceJoint2D joint;
  public ContactFilter2D filter;
  // Start is called before the first frame update
  void Start() {

  }

  // Update is called once per frame
  void Update() {
    if (joint.attachedRigidbody == null) {
      attached = null;
    }
  }

  public void TryAttach() {
    if (attached == null) {
      var res = new List<Collider2D>();
      GetComponent<Rigidbody2D>().OverlapCollider(filter, res);
      foreach (var item in res) {
        if (item.GetComponent<FishBehaviour2D>() != null) {
          attached = item.GetComponent<FishBehaviour2D>();
          joint.connectedAnchor = attached.transform.position;
          joint.connectedBody = attached.GetComponent<Rigidbody2D>();
          break;
        }
      }
    }
  }
}
