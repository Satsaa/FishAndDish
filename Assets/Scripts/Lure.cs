using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lure : MonoBehaviour {
  [HideInInspector]
  public FishBehaviour2D attached;
  public DistanceJoint2D jointBase;
  [HideInInspector]
  public DistanceJoint2D joint;
  public ContactFilter2D filter;
  // Start is called before the first frame update
  void Start() {

  }

  // Update is called once per frame
  void Update() {
    if (joint == null) {
      joint = joint.CopyComponent(gameObject);
      if (attached != null)
        attached.GetComponent<FaceVelocity2D>().enabled = true;
      attached = null;
      joint.enabled = false;
    }
  }

  public bool TryAttach() {
    if (attached == null) {
      var res = new List<Collider2D>();
      GetComponent<Rigidbody2D>().OverlapCollider(filter, res);
      foreach (var item in res) {
        if (item.GetComponent<FishBehaviour2D>() != null) {
          attached = item.GetComponent<FishBehaviour2D>();
          attached.GetComponent<FaceVelocity2D>().enabled = false;
          // joint.connectedAnchor = attached.transform.position;
          joint.connectedBody = attached.GetComponent<Rigidbody2D>();
          joint.enabled = true;
          return true;
        }
      }
    }
    return false;
  }
}
