using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class WaterTrigger : MonoBehaviour {

  void OnTriggerEnter2D(Collider2D col) {
    var fb = col.gameObject.GetComponent<FishBehaviour2D>();
    if (fb == null) return;
    var rb = col.gameObject.GetComponent<Rigidbody2D>();
    if (rb == null) return;

    fb.allowBurst = true;
    fb.useTailDecay = true;

    var rd = col.gameObject.GetComponent<RandomDirectionChange2D>();
    if (rd != null) rd.enabled = true;

  }

  void OnTriggerExit2D(Collider2D col) {
    var fb = col.gameObject.GetComponent<FishBehaviour2D>();
    if (fb == null) return;
    var rb = col.gameObject.GetComponent<Rigidbody2D>();
    if (rb == null) return;

    fb.allowBurst = false;
    fb.useTailDecay = false;

    var rd = col.gameObject.GetComponent<RandomDirectionChange2D>();
    if (rd != null) rd.enabled = true;

  }
}
