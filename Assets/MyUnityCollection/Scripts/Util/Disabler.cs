using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Disabler {

  [Tooltip("Only Renderer, Behaviour and GameObject types are supported!")]
  public List<Object> objects;

  public void DisableComponents() {
    foreach (var obj in objects)
      Disable(obj);
  }

  public void EnableComponents() {
    foreach (var obj in objects)
      Enable(obj);
  }


  void Disable(Object obj) {
    var r = obj as Renderer;
    if (r != null) {
      r.enabled = false;
    } else {
      var b = obj as Behaviour;
      if (b != null) {
        b.enabled = false;
      } else {
        var g = obj as GameObject;
        if (g != null) {
          g.SetActive(false);
        }
      }
    }
  }

  void Enable(Object obj) {
    var r = obj as Renderer;
    if (r != null) {
      r.enabled = true;
    } else {
      var b = obj as Behaviour;
      if (b != null) {
        b.enabled = true;
      } else {
        var g = obj as GameObject;
        if (g != null) {
          g.SetActive(true);
        }
      }
    }
  }
}
