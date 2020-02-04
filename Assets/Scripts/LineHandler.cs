using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

[ExecuteAlways]
[RequireComponent(typeof(LineRenderer))]
public class LineHandler : MonoBehaviour {

  public Transform start;
  public Transform end;

  public Vector3 curviness = new Vector3(0, -1, 0);
  public AnimationCurve lineCurve;

  [MyBox.PositiveValueOnly]
  public int points = 10;
  // Update is called once per frame
  void Update() {
    if (start == null || end == null) return;
    points = math.max(points, 2);

    var lh = GetComponent<LineRenderer>();
    var startPos = start.position;
    var endPos = end.position;

    var curveMult = Vector3.Distance(startPos, endPos);
    var positions = new Vector3[points];
    for (int i = 0; i < positions.Length; i++) {
      var fraction = (float)i / (positions.Length - 1);
      var fractionedCurve = (curveMult * curviness) * lineCurve.Evaluate(fraction);
      positions[i] = (Vector3)math.lerp(startPos, endPos, fraction) + fractionedCurve;
    }
    lh.positionCount = points;
    lh.SetPositions(positions);
  }
}
