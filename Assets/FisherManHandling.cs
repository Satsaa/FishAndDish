using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class FisherManHandling : MonoBehaviour, IPointerDownHandler {
  public GameObject[] fishUIElement;
  public float defaultDistance = 1;
  public float distance = 1;
  public float reelSpeed = 1;
  public float fishReelSpeed = 0.5f;
  public float catchDistance = 1;
  public Action action = Action.charge;
  public Animator animator;
  public Vector2 throwVelocity = new Vector2(-1, 1);
  public float maxChargeDuration = 2;
  public Lure lure;
  public DistanceJoint2D lineJoint;
  public Transform lineStart;
  public Transform lineEnd;
  public float maxDensity = 50;

  public bool pressing = false;
  public float chargeStartTime = float.PositiveInfinity;

  public enum Action {
    charge,
    reel,
    fishReel,
    confirm, // Confirm you caught fish
  }

  private float defaultDensity;

  // Start is called before the first frame update
  void Start() {
    defaultDensity = lineEnd.GetComponent<Collider2D>().density;
  }

  // Update is called once per frame
  void Update() {
    switch (action) {
      case Action.charge:
        lineEnd.GetComponent<Collider2D>().density = Mathf.Min(maxDensity, lineEnd.GetComponent<Collider2D>().density * (1 + 1 * Time.deltaTime));
        lineJoint.distance = defaultDistance;
        distance = defaultDistance;
        if (pressing) {
          animator.Play("Throw");
          print(action + " " + pressing);
          if (chargeStartTime == float.PositiveInfinity) {
            if (chargeStartTime + maxChargeDuration > Time.time) {
              chargeStartTime = Time.time;
            }
          }
          if (chargeStartTime + maxChargeDuration < Time.time) {
            pressing = false;
          }
        } else {

          print(action + " " + pressing);
          if (chargeStartTime == float.PositiveInfinity) {
            lineJoint.enabled = true;
            animator.Play("FishingIdle");
            chargeStartTime = float.PositiveInfinity;
          } else {
            lineJoint.enabled = false;
            action = Action.reel;
            lineEnd.GetComponent<Collider2D>().density = defaultDensity;
            lure.GetComponent<Rigidbody2D>().velocity += throwVelocity * Mathf.Clamp01((Time.time - chargeStartTime) / maxChargeDuration);
            chargeStartTime = float.PositiveInfinity;
          }
        }
        break;

      case Action.reel:
        if (pressing) {
          lineEnd.GetComponent<Collider2D>().density = Mathf.Min(maxDensity, defaultDensity * 10);
          lineJoint.enabled = true;
          print(action + " " + pressing);
          if (lineJoint.enabled == false) {
            lineJoint.enabled = true;
            lineJoint.distance = Vector2.Distance(lineStart.transform.position, lineEnd.transform.position);
            distance = lineJoint.distance;
          }
          animator.Play("Reel");
          distance -= reelSpeed * Time.deltaTime;
          lineJoint.distance = distance;
          if (lure.TryAttach()) {
            action = Action.fishReel;
          } else if (distance <= catchDistance) {
            pressing = false;
            action = Action.charge;
          }
        } else {
          lineEnd.GetComponent<Collider2D>().density = defaultDensity;
          lineJoint.enabled = false;
          lineJoint.distance = Vector2.Distance(lineStart.transform.position, lineEnd.transform.position);
          distance = lineJoint.distance;
          print(action + " " + pressing);
          animator.Play("FishingIdle");
        }
        break;

      case Action.fishReel:
        if (pressing) {

          print(action + " " + pressing);
          if (lure.joint.attachedRigidbody == null) {
            action = Action.reel;
            return;
          }
          animator.Play("HardReel");
          distance -= fishReelSpeed * Time.deltaTime;
          lineJoint.distance = distance;
          if (distance <= catchDistance) {
            distance = catchDistance;
            lineEnd.GetComponent<Collider2D>().density = Mathf.Min(maxDensity, lineEnd.GetComponent<Collider2D>().density + 1 * Time.deltaTime);
            if (lure.attached == null || Vector2.Distance(lineStart.transform.position, lineEnd.transform.position) <= catchDistance) {
              pressing = false;
              action = Action.confirm;
              animator.Play("FishingIdle");
            }
          }
        } else {

          print(action + " " + pressing);
          if (lure.joint.attachedRigidbody == null) {
            action = Action.reel;
            return;
          }
          animator.Play("Struggle");
        }
        break;

      case Action.confirm:
        if (lure.attached == null) {
          action = Action.charge;
          return;
        }
        if (pressing) {
          print(action + " " + pressing);
          print(lure.attached.GetComponentInChildren<SkinnedMeshRenderer>().material.name.Replace(" (Instance)", ""));
          ShowFishInList((lure.attached.GetComponentInChildren<SkinnedMeshRenderer>().material.name.Replace(" (Instance)", "")));
          GameObject.FindObjectOfType<FishSpawner>().fishCount--;
          Destroy(lure.attached.gameObject);
          action = Action.charge;
          chargeStartTime = float.PositiveInfinity;
          lineJoint.distance = defaultDistance;
          distance = defaultDistance;
          pressing = false;
        } else {
          print(action + " " + pressing);
        }
        break;
    }
  }

  public void OnPointerDown(PointerEventData eventData) {
    pressing = true;
  }

  public void TakeInput() {
    pressing = false;
  }

  void ShowFishInList(string name) {
    foreach (var fish in fishUIElement) {
      var images = fish.GetComponentsInChildren<UnityEngine.UI.Image>();
      foreach (var image in images) {
        if (image.sprite.name == name) {
          fish.SetActive(true);
          break;
        }
      }
    }
  }
}
