using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class FishDespawner : MonoBehaviour {
  private FishSpawner fs;
  void Start() {
    fs = GameObject.FindObjectOfType<FishSpawner>();
  }
  void OnTriggerEnter2D(Collider2D col) {
    var fb = col.GetComponent<FishBehaviour2D>();
    if (fb == null) return;
    Destroy(fb.gameObject);
    fs.fishCount--;
  }
}
