using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class Box {
  public float2 center;
  public float2 size;

  // Start is called before the first frame update
  void Start() {

  }

  // Update is called once per frame
  void Update() {

  }

  void OnDrawGizmosSelected() {
    Gizmos.color = Color.magenta;
    Gizmos.DrawWireCube(center, size);

    Gizmos.color = Color.magenta * 0.2f;
    Gizmos.DrawCube(center, size);
  }
}
