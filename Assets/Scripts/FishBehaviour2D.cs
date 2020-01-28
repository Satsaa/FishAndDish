using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using MyBox;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class FishBehaviour2D : MonoBehaviour {

  [Foldout("Components", true)]

  [AutoProperty]
  public Collider2D col;
  [AutoProperty]
  public Rigidbody2D rb;

  [Foldout("Movement Settings", true)]

  [Tooltip("The velocity the fish tries to reach")]
  public float ambientSpeed = 0.1f;

  [Foldout("Burst Settings", true)]

  [Tooltip("The range of speed a boost gives (min burst speed, max burst speed)")]
  public float2 burstSpeed = new float2(0.6f, 1f);

  [Tooltip("The sample curve of burstSpeed")]
  public AnimationCurve burstSampleCurve = AnimationCurve.Linear(0, 0, 1, 1);

  [Tooltip("The maximum amount of bursts the fish can use")]
  public int2 burstsPerUse = 5;

  [Tooltip("The maximum amount of bursts the fish can have")]
  public int maxBurstsCapacity = 5;

  [Tooltip("The time for a burst to recharge (only 1 burst recharges at a time)")]
  public float2 burstRechargeTime = new float2(0.5f, 1f);

  [Range(0, 1)]
  [Tooltip("The chance that bursts are used per second on average")]
  public float burstChance = 0.5f;

  [Foldout("Debug", true)]
  public int bursts = 0;

  public float[] speedList;



  private int remainingBursts = 0;
  private float nextBurstTime = float.NegativeInfinity;

  void OnValidate() {
    if (burstsPerUse.y == 1) {
      speedList = new float[1] { burstSpeed.y };
    } else {
      speedList = new float[burstsPerUse.y];
      float t = 1f / (burstsPerUse.y - 1);
      for (int i = 0; i < burstsPerUse.y; i++) {
        speedList[i] = burstSpeed.x + burstSampleCurve.Evaluate(i / ((float)burstsPerUse.y - 1)) * (burstSpeed.y - burstSpeed.x);
      }
    }
    var prevSpeed = float.NegativeInfinity;
    foreach (var speed in speedList) {
      if (speed > prevSpeed) {
        Debug.LogWarning("Burst speed curve must be sloping up at every sampled points!");
        break;
      }
    }
  }

  // Start is called before the first frame update
  void Start() {

  }

  // Update is called once per frame
  void Update() {

  }


  public void AddBurst(int bursts = -1) {
    if (bursts < 0) bursts = UnityEngine.Random.Range(burstsPerUse.x, burstsPerUse.y);
    remainingBursts += bursts;
  }

  void Burst() {
    remainingBursts--;
  }


  public float GetNextSpeed(float speed) {
    var i = GetNearestSpeedIndex(speed);
    return speedList[math.clamp(i, 0, speedList.Length - 1)];
  }

  public int GetNearestSpeedIndex(float speed) {
    var minDist = float.PositiveInfinity;
    var minI = 0;
    int i = -1;
    foreach (var compSpeed in speedList) {
      i++;
      var dist = math.distance(speed, compSpeed);
      if (dist < minDist) {
        minDist = dist;
        minI = i;
      } else if (dist == 0) {
        return i;
      }
    }
    return minI;
  }
}
