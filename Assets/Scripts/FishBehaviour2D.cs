using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using Unity.Mathematics;
using MyBox;
using static MyCollection.MyRandom;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class FishBehaviour2D : MonoBehaviour {


  [Foldout("Components", true)]

  public Collider2D col;
  public Rigidbody2D rb;


  [Foldout("Movement Settings", true)]

  [Tooltip("The velocity the fish tries to reach")]
  public float ambientSpeed = 0.1f;
  [Range(0, 1)]
  [Tooltip("Aproach target speed by this fraction per second (when slowing down)")]
  public float speedReachFractionDown = 0.1f;
  [Range(0, 1)]
  [Tooltip("Aproach target speed by this fraction per second (when speeding up)")]
  public float speedReachFractionUp = 0.1f;


  [Foldout("Animation Settings", true)]

  [Tooltip("The amount of wiggle on dat behind")]
  public float tailMultiplier = 0.1f;
  [Range(0, 1)]
  [Tooltip("Reduce tail animation speed by this much per second")]
  public float tailDecay = 0.1f;
  [MinMaxRange(0, 5)]
  [Tooltip("Adjust maxTailAngle based on this range of velocity. MaxTailAngle is multiplied linearly between 0 and 1 based on the relationship of velocity between this range")]
  public Vector2 tailStraightenRange = new Vector2(0, 0.5f);
  [Tooltip("The maximum tail rotation in one direction")]
  [Range(0, 180)]
  public float maxTailAngle = 45f;
  [Range(0, 1)]
  public float tailDampening = 0.03f;
  public Rotator[] rotators;


  [Foldout("Burst Settings", true)]

  [Tooltip("The range of speed a boost gives (min burst speed, max burst speed)")]
  public float2 burstSpeed = new float2(0.6f, 1f);
  [Tooltip("The sample curve of burstSpeed")]
  public AnimationCurve burstSampleCurve = AnimationCurve.Linear(0, 0, 1, 1);
  [Tooltip("The maximum amount of bursts the fish can use")]
  public int2 burstsPerUse = 5;
  [Tooltip("The maximum amount of bursts the fish can \"store\" for use")]
  public int burstCapacity = 5;
  [Tooltip("The time for a burst to recharge (only 1 burst recharges at a time)")]
  public float2 burstRechargeTime = new float2(0.5f, 1f);
  [Tooltip("Delay before bursting again")]
  public float2 burstDelay = 0.2f;

  [Range(0, 1)]
  [Tooltip("The chance of using bursts per second on average")]
  public float burstChance = 0.5f;


  [Foldout("Debug", true)]

  [SerializeField] private int bursts = 0;

  [SerializeField] private float[] speedList;




  [SerializeField] private int queuedBursts;
  [SerializeField] private float nextBurstTime = float.PositiveInfinity;
  [SerializeField] private float nextBurstRecharge = float.PositiveInfinity;
  [SerializeField] private float targetSpeed = 0.01f;

  [SerializeField] private float tailSpeed;
  /// <summary> Value used to calculate rotation with math.sin </summary>
  [SerializeField] private float tailSineValue;
  [SerializeField] private float prevMaxAdjustedAngle;
  [SerializeField] private float speed;

  [System.Serializable]
  public class Rotator {
    public float multiplier;
    public GameObject go;
    [HideInInspector]
    public float defaultRotation;
    public float rotation { get => go.transform.localEulerAngles.z; set => go.transform.localEulerAngles = go.transform.localEulerAngles.SetZ(value); }
  }


  #region Init

#if UNITY_EDITOR
  void OnValidate() {
    if (PrefabUtility.GetPrefabAssetType(gameObject) != PrefabAssetType.NotAPrefab) return;
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
    foreach (var listedSpeed in speedList) {
      if (listedSpeed <= prevSpeed) {
        Debug.LogWarning("If the burst speed curve slopes down at any point, behaviour may be unpredictable!");
        break;
      }
      prevSpeed = listedSpeed;
    }
    if (rb == null) rb = gameObject.GetComponent<Rigidbody2D>();
    if (col == null) col = gameObject.GetComponent<Collider2D>();
  }
#endif

  // Start is called before the first frame update
  void Start() {
    nextBurstRecharge = Time.time + Random.Range(burstRechargeTime.x, burstRechargeTime.y);
    foreach (var rotator in rotators)
      rotator.defaultRotation = rotator.rotation;
  }

  #endregion


  #region Update

  // Update is called once per frame
  void Update() {
    speed = rb.velocity.magnitude;
    if (DeltaAdjustedChance(burstChance))
      Use();
    HandleVelocity();
    HandleBursting();
    AnimateTail();
  }

  void HandleVelocity() {
    var diff = targetSpeed - speed;

    float fract = 1 - math.pow(1 - (diff > 0 ? speedReachFractionUp : speedReachFractionDown), Time.deltaTime);
    var newSpeed = speed + diff * fract;
    rb.velocity = rb.velocity.SetLenSafe(newSpeed, new Vector2(1, 0));
    if (speed > targetSpeed || math.abs(diff) < 0.05f) {
      targetSpeed = ambientSpeed;
    } else {

    }
  }

  void HandleBursting() {
    if (Time.time > nextBurstRecharge) {
      if (bursts < burstCapacity)
        bursts++;
      if (bursts < burstCapacity)
        nextBurstRecharge = Time.time + Random.Range(burstRechargeTime.x, burstRechargeTime.y);
      else
        nextBurstRecharge = float.PositiveInfinity;
    }
    if (Time.time > nextBurstTime) {
      Burst();
      nextBurstRecharge = Time.time + Random.Range(burstRechargeTime.x, burstRechargeTime.y);
      if (queuedBursts > 0)
        nextBurstTime = Time.time + Random.Range(burstDelay.x, burstDelay.y);
      else
        nextBurstTime = float.PositiveInfinity;
    }
  }

  void AnimateTail() {
    float decay = math.pow(1 - tailDecay, Time.deltaTime);
    tailSpeed *= decay;
    tailSineValue += tailSpeed * Time.deltaTime;
    var tailAngleMult = (Mathf.InverseLerp(tailStraightenRange.x, tailStraightenRange.y, speed));
    var maxAdjustedAngle = prevMaxAdjustedAngle + (maxTailAngle * tailAngleMult - prevMaxAdjustedAngle) * tailDampening;
    var tailAngle = math.sin(tailSineValue * tailMultiplier) * maxAdjustedAngle;
    Vector3 end = transform.position + transform.right.xy().SetAngle(tailAngle).xyo();
    prevMaxAdjustedAngle = maxAdjustedAngle;
    Debug.DrawLine(transform.position, end);

    foreach (var rtr in rotators)
      rtr.rotation = rtr.defaultRotation + tailAngle * rtr.multiplier;
  }

  #endregion


  #region Methods

  public void Use(int burstCount = -1) {
    if (burstCount == -1) burstCount = math.clamp(Random.Range(burstsPerUse.x, burstsPerUse.y), 0, bursts);
    if (burstCount <= 0) return;
    if (queuedBursts < 1 && queuedBursts + burstCount > 0) {
      nextBurstTime = Time.time;
    }
    bursts -= burstCount;
    queuedBursts += burstCount;
    if (queuedBursts > 0) {
      nextBurstTime = Time.time + Random.Range(burstDelay.x, burstDelay.y);
    }
  }

  public void Burst() {
    queuedBursts--;
    var prevTargetSpeed = targetSpeed;
    targetSpeed = GetNextSpeed(targetSpeed);
    tailSpeed += targetSpeed - prevTargetSpeed;
  }

  public float GetNextSpeed(float speed) {
    var i = GetNearestSpeedIndex(speed);
    return speedList[math.clamp(i + 1, 0, speedList.Length - 1)];
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
