using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class TestDist : MonoBehaviour {
  public Transform a1;
  public Transform a2;
  public Transform b1;
  public Transform b2;

  // Update is called once per frame
  void Update() {
    Line a = new Line(a1.position, a2.position);
    Line b = new Line(b1.position, b2.position);
    a.Debug(Color.red);
    b.Debug(Color.green);

    var res = a.DistanceLine(b);
    res.Debug(Color.blue);
  }
}
