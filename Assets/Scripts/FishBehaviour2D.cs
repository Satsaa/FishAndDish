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

  [Foldout("Movement Settings", true)]

  [Tooltip("Disables the effect of this script on RigidBody.velocity")]
  public bool allowVelocityChange = true;

  [Tooltip("The velocity the fish tries to reach")]
  public float ambientSpeed = 0.1f;


  [Foldout("Animation Settings", true)]

  [Tooltip("The amount of wiggle on dat behind")]
  public float tailMultiplier = 0.1f;

  [Range(0, 1)]
  [Tooltip("Reduce tail animation speed by this much per second")]
  public float tailDecay = 0.1f;
  public bool useTailDecay = true;

  [Tooltip("Maximum rotation of a single rotator (at " + nameof(Rotator.multiplier) + "= 1)")]
  [Range(0, 180)]
  public float maxAngle = 45f;

  [MinMaxRange(0, 5)]
  [Tooltip("Adjust maxTailAngle based on this range of velocity. MaxTailAngle is multiplied linearly between 0 and 1 based on the relationship of velocity between this range")]
  public Vector2 tailStraightenRange = new Vector2(0, 0.5f);

  [Range(0, 1)]
  public float tailDampening = 0.03f;
  public Rotator[] rotators;


  [Foldout("Burst Settings", true)]

  [Tooltip("Speed added per boost")]
  public float burstSpeed = 0.25f;
  [Tooltip("Speed added to tail movement per boost")]
  public float bustTailSpeed = 0.25f;
  [Tooltip("The maximum amount of bursts the fish can use")]
  public int2 burstsPerUse = 5;
  [Tooltip("The maximum amount of bursts the fish can \"store\" for use")]
  public int burstCapacity = 5;
  [Tooltip("The time for a burst to recharge (only 1 burst recharges at a time)")]
  public float2 burstRechargeTime = new float2(0.5f, 1f);
  [Tooltip("Delay before bursting again")]
  public float2 burstDelay = 0.2f;
  public bool allowBurst = true;

  [Range(0, 1)]
  [Tooltip("The chance of using bursts per second on average")]
  public float burstChance = 0.5f;


  [Foldout("Debug", true)]

  [SerializeField] private int bursts = 0;


  [SerializeField] private int queuedBursts;
  [SerializeField] public float nextBurstTime = float.PositiveInfinity;
  [SerializeField] private float nextBurstRecharge = float.PositiveInfinity;

  [SerializeField] public float tailSpeed;
  /// <summary> Value used to calculate rotation with math.sin </summary>
  [SerializeField] private float tailSineValue;
  [SerializeField] private float prevMaxAdjustedAngle;
  [SerializeField] private float speed;

  [SerializeField] private Collider2D col;
  [SerializeField] private Rigidbody2D rb;

  [System.Serializable]
  public class Rotator {
    public float multiplier;
    public GameObject go;
    [HideInInspector]
    public float defaultRotation;
    public float rotation { get => go.transform.localEulerAngles.z; set => go.transform.localEulerAngles = go.transform.localEulerAngles.SetZ(value); }
  }


  // Start is called before the first frame update
  void Start() {
    nextBurstRecharge = Time.time + Random.Range(burstRechargeTime.x, burstRechargeTime.y);
    foreach (var rotator in rotators)
      rotator.defaultRotation = rotator.rotation;
    rb = gameObject.GetComponent<Rigidbody2D>();
    col = gameObject.GetComponent<Collider2D>();
  }


  #region Update

  // Update is called once per frame
  void Update() {
    speed = rb.velocity.magnitude;
    if (DeltaAdjustedChance(burstChance))
      UseBurst();
    HandleVelocity();
    HandleBursting();
    AnimateTail();
  }

  void HandleVelocity() {
    if (speed < ambientSpeed) {
      rb.velocity = rb.velocity.AddLenSafe(ambientSpeed - speed * 0.05f, rb.transform.right);
    }
  }

  void HandleBursting() {
    if (Time.time > nextBurstRecharge) {
      if (bursts < burstCapacity) {
        bursts++;
        nextBurstRecharge = Time.time + Random.Range(burstRechargeTime.x, burstRechargeTime.y);
      } else nextBurstRecharge = float.PositiveInfinity;
    }
    if (Time.time > nextBurstTime) {
      Burst();
      nextBurstRecharge = Time.time + Random.Range(burstRechargeTime.x, burstRechargeTime.y);
      nextBurstTime = queuedBursts > 0 ? Time.time + Random.Range(burstDelay.x, burstDelay.y) : float.PositiveInfinity;
    }
  }

  void AnimateTail() {
    float decay = useTailDecay ? math.pow(1 - tailDecay, Time.deltaTime) : 1;
    tailSpeed *= decay;
    tailSineValue += tailSpeed * Time.deltaTime;
    var tailAngleMult = (Mathf.InverseLerp(tailStraightenRange.x, tailStraightenRange.y, speed));
    var maxAdjustedAngle = prevMaxAdjustedAngle + (maxAngle * tailAngleMult - prevMaxAdjustedAngle) * tailDampening;
    var tailAngle = math.sin(tailSineValue * tailMultiplier) * maxAdjustedAngle;
    Vector3 end = transform.position + transform.right.xy().SetAngle(tailAngle).xyo();
    prevMaxAdjustedAngle = maxAdjustedAngle;

    foreach (var rtr in rotators)
      rtr.rotation = rtr.defaultRotation + tailAngle * rtr.multiplier;
  }

  #endregion


  #region Methods

  public void UseBurst(int burstCount = -1) {
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
    if (allowBurst) {
      queuedBursts--;
      if (allowVelocityChange) rb.velocity = rb.velocity.AddLenSafe(burstSpeed, rb.transform.right);
      tailSpeed += bustTailSpeed;
    }
  }

  #endregion
}
