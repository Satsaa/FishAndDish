using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RandomDirectionChange : MonoBehaviour {

  private Rigidbody rb;

  [Tooltip("Interval of velocity direction rotation")]
  public float rotationInterval = 1;
  [Tooltip("Maximum degrees of velocity direction rotation per interval")]
  public float rotationJitter = 90;

  private float lastRotationChange = float.NegativeInfinity;
  private Vector3 rotation;

  // Start is called before the first frame update
  void Start() {
    rb = GetComponent<Rigidbody>();
    rotation = new Vector3(Random.Range(-rotationJitter, rotationJitter), Random.Range(-rotationJitter, rotationJitter), Random.Range(-rotationJitter, rotationJitter));
    // Prevent synchronization with others sharing same values
    lastRotationChange = Time.time - Random.Range(0, rotationInterval);
  }

  // Update is called once per frame
  void FixedUpdate() {
    if (lastRotationChange < Time.time - rotationInterval) {
      rotation = new Vector3(Random.Range(-rotationJitter, rotationJitter), Random.Range(-rotationJitter, rotationJitter), Random.Range(-rotationJitter, rotationJitter));
      lastRotationChange = Time.time;
    }
    var deltaRotation = Quaternion.Euler(rotation.x * Time.deltaTime, rotation.y * Time.deltaTime, rotation.z * Time.deltaTime);
    rb.velocity = deltaRotation * rb.velocity;
  }
}
