using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

[ExecuteInEditMode]
public class TestDist : MonoBehaviour {
  public Transform a1;
  public Transform a2;
  public Transform b1;
  public Transform b2;

  public Line preLine;
  public Line res;

  // Update is called once per frame
  void Update() {
    Line first = new Line(a1.position, a2.position);
    Line second = new Line(b1.position, b2.position);

    first.Debug(Color.green);
    second.Debug(Color.red);

    first.ShortestConnectingLine(second).Debug(Color.cyan);
  }

}
