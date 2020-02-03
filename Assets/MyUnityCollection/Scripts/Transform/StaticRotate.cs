using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class StaticRotate : MonoBehaviour {

  [Tooltip("The rotation applied per second")]
  public float3 rotation;
  [MyBox.ConditionalField(nameof(useRbRotate))]
  public bool local = false;
  public bool useRbRotate = false;

  // Update is called once per frame
  void Update() {
    if (useRbRotate) {
      var rot = gameObject.transform.rotation;
      var qt = (quaternion.EulerXYZ(math.radians(rotation) * Time.deltaTime) * rot);
      var rb = gameObject.GetComponent<Rigidbody>();
      if (rb) {

        rb.MoveRotation(qt);
      } else {
        var rb2D = gameObject.GetComponent<Rigidbody2D>();
        rb2D.MoveRotation(qt);
      }
    } else {
      var qt = quaternion.EulerXYZ(math.radians(rotation) * Time.deltaTime);
      if (local)
        transform.localRotation *= qt;
      else
        transform.rotation *= qt;
    }
  }
}
