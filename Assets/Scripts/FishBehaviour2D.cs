using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using Unity.Mathematics;
using MyBox;
using static MyCollection.MyRandom;

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

  [Foldout("Animation Settings", true)]

  [Tooltip("The amount of wiggle on dat behind")]
  public float tailMultiplier = 0.1f;
  [Tooltip("Reduce tail animation speed by this much per second")]
  public float tailDecay = 0.1f;
  [Tooltip("Move tail to straight angle this much per second")]
  public float tailReturnToStraightFraction = 0.1f;

  [Foldout("Burst Settings", true)]

  [Tooltip("The range of speed a boost gives (min burst speed, max burst speed)")]
  public float2 burstSpeed = new float2(0.6f, 1f);
  [Tooltip("The sample curve of burstSpeed")]
  public AnimationCurve burstSampleCurve = AnimationCurve.Linear(0, 0, 1, 1);
  [Delayed]
  [Tooltip("The maximum amount of bursts the fish can use")]
  public int2 burstsPerUse = 5;
  [Tooltip("The maximum amount of bursts the fish can have")]
  public int maxBurstsCapacity = 5;
  [Tooltip("The time for a burst to recharge (only 1 burst recharges at a time)")]
  public float2 burstRechargeTime = new float2(0.5f, 1f);

  [Range(0, 1)]
  [Tooltip("The chance of using bursts per second on average")]
  public float burstChance = 0.5f;


  [Foldout("Debug", true)]

  public int bursts = 0;

  public float[] speedList;




  private int remainingBursts;
  private float nextBurstTime = float.NegativeInfinity;
  private bool bursting;
  private float targetSpeed = 0.01f;

  private float tailSpeed;
  /// <summary> Value used to calculate rotation with math.sin </summary>
  private float tailAnimationValue;


  #region Init

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
      if (speed <= prevSpeed) {
        Debug.LogWarning("If the burst speed curve slopes down at any point, behaviour may be unpredictable!");
        break;
      }
      prevSpeed = speed;
    }
  }

  // Start is called before the first frame update
  void Start() {

  }

  #endregion


  #region Update

  // Update is called once per frame
  void Update() {
    if (Random.value <= DeltaAdjustedChance(burstChance))
      Use();
    HandleBursting();
    AnimateTail();
  }

  void HandleBursting() {
    if (!bursting) return;
  }

  void AnimateTail() {
    float speedMultiplier = math.pow(1 - tailDecay, Time.deltaTime);
    tailSpeed *= speedMultiplier;
    // !!! Wiggle wiggle anim

    float tailRotTowardsStraight = math.pow(1 - tailReturnToStraightFraction, Time.deltaTime);
  }

  #endregion


  #region Methods

  public void Use(int bursts = -1) {
    if (bursts < 0) bursts = UnityEngine.Random.Range(burstsPerUse.x, burstsPerUse.y);
    remainingBursts += bursts;
    if (remainingBursts > 0)
      bursting = true;
  }

  public void Burst() {
    remainingBursts--;
    targetSpeed = GetNextSpeed(targetSpeed);
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

  #endregion
}
