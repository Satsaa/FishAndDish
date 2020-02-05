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
  public DistanceJoint2D joint;
  public Transform lineStart;
  public Transform lineEnd;

  public bool pressing = false;
  public float chargeStartTime = float.PositiveInfinity;

  public enum Action {
    charge,
    reel,
    fishReel,
    confirm, // Confirm you caught fish
  }
  // Start is called before the first frame update
  void Start() {

  }

  // Update is called once per frame
  void Update() {
    if (pressing) {
      switch (action) {
        case Action.charge:
          print(1);
          joint.enabled = true;
          if (chargeStartTime == float.PositiveInfinity) {
            if (chargeStartTime + maxChargeDuration > Time.time) {
              chargeStartTime = Time.time;
            }
          }
          if (chargeStartTime + maxChargeDuration < Time.time) {
            pressing = false;
          }
          break;
        case Action.reel:
          print(2);
          if (joint.enabled == false) {
            joint.enabled = true;
            joint.distance = Vector2.Distance(lineStart.transform.position, lineEnd.transform.position);
          }
          animator.Play("Reel");
          distance -= reelSpeed * Time.deltaTime;
          joint.distance = defaultDistance;
          lure.TryAttach();
          if (distance < catchDistance) {
            pressing = false;
            action = Action.charge;
          }
          break;
        case Action.fishReel:
          print(3);
          if (lure.joint.attachedRigidbody == null) {
            action = Action.reel;
            return;
          }
          animator.Play("HardReel");
          distance -= fishReelSpeed * Time.deltaTime;
          joint.distance = defaultDistance;
          if (distance < catchDistance) {
            pressing = false;
            action = Action.confirm;
            ShowFishInList(lure.attached.GetComponent<SkinnedMeshRenderer>().material.name);
          }
          break;
        case Action.confirm:
          print(4);
          joint.distance = defaultDistance;
          animator.Play("FishingIdle");
          ShowFishInList(lure.attached.GetComponentInChildren<SkinnedMeshRenderer>().material.name);
          Destroy(lure.attached.gameObject);
          break;
      }
    } else {
      switch (action) {
        case Action.charge:
          print(5);
          joint.enabled = true;
          if (chargeStartTime != float.PositiveInfinity) {
            animator.Play("Throw");
            joint.enabled = false;
            action = Action.reel;
            lure.GetComponent<Rigidbody2D>().velocity += throwVelocity * Mathf.Clamp01((Time.time - chargeStartTime) / maxChargeDuration);

            chargeStartTime = float.PositiveInfinity;
          }
          break;
        case Action.reel:
          print(6);
          animator.Play("FishingIdle");
          break;
        case Action.fishReel:
          print(7);
          if (lure.joint.attachedRigidbody == null) {
            action = Action.reel;
            return;
          }
          animator.Play("Struggle");
          break;
        case Action.confirm:
          print(8);
          joint.distance = defaultDistance;
          animator.Play("FishingIdle");
          ShowFishInList(lure.attached.GetComponentInChildren<SkinnedMeshRenderer>().material.name);
          Destroy(lure.attached.gameObject);
          break;
      }
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
