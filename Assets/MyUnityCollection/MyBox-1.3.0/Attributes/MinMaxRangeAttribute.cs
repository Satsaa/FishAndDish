// ---------------------------------------------------------------------------- 
// Author: Richard Fine
// Source: https://bitbucket.org/richardfine/scriptableobjectdemo
// ----------------------------------------------------------------------------

using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

namespace MyBox {
  public class MinMaxRangeAttribute : PropertyAttribute {
    public MinMaxRangeAttribute(float x, float y) {
      this.x = x;
      this.y = y;
    }

    public readonly float x;
    public readonly float y;
  }

  [Serializable]
  public struct RangedFloat {
    public float x;
    public float y;

    public RangedFloat(float x, float y) {
      this.x = x;
      this.y = y;
    }
  }

  [Serializable]
  public struct RangedInt {
    public int x;
    public int y;

    public RangedInt(int x, int y) {
      this.x = x;
      this.y = y;
    }
  }

  public static class RangedExtensions {
    public static float LerpFromRange(this RangedFloat ranged, float t) {
      return Mathf.Lerp(ranged.x, ranged.y, t);
    }

    public static float LerpFromRangeUnclamped(this RangedFloat ranged, float t) {
      return Mathf.LerpUnclamped(ranged.x, ranged.y, t);
    }

    public static float LerpFromRange(this RangedInt ranged, float t) {
      return Mathf.Lerp(ranged.x, ranged.y, t);
    }

    public static float LerpFromRangeUnclamped(this RangedInt ranged, float t) {
      return Mathf.LerpUnclamped(ranged.x, ranged.y, t);
    }
  }
}

#if UNITY_EDITOR
namespace MyBox.Internal {
  [CustomPropertyDrawer(typeof(MinMaxRangeAttribute))]
  public class MinMaxRangeIntAttributeDrawer : PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
      SerializedProperty xProp = property.FindPropertyRelative("x");
      SerializedProperty yProp = property.FindPropertyRelative("y");
      if (xProp == null || yProp == null) {
        WarningsPool.Log("MinMaxRangeAttribute used on <color=brown>" +
                         property.name +
                         "</color>. Must be used on types with x and y fields",
          property.serializedObject.targetObject);

        return;
      }

      var minValid = xProp.propertyType == SerializedPropertyType.Integer || xProp.propertyType == SerializedPropertyType.Float;
      var maxValid = yProp.propertyType == SerializedPropertyType.Integer || yProp.propertyType == SerializedPropertyType.Float;
      if (!maxValid || !minValid || xProp.propertyType != yProp.propertyType) {
        WarningsPool.Log("MinMaxRangeAttribute used on <color=brown>" +
                         property.name +
                         "</color>. Min and Max fields must be of int or float type",
          property.serializedObject.targetObject);

        return;
      }

      MinMaxRangeAttribute rangeAttribute = (MinMaxRangeAttribute)attribute;

      label = EditorGUI.BeginProperty(position, label, property);
      position = EditorGUI.PrefixLabel(position, label);

      bool isInt = xProp.propertyType == SerializedPropertyType.Integer;

      float xValue = isInt ? xProp.intValue : xProp.floatValue;
      float yValue = isInt ? yProp.intValue : yProp.floatValue;
      float rangeMin = rangeAttribute.x;
      float rangeMax = rangeAttribute.y;


      const float rangeBoundsLabelWidth = 40f;

      var rangeBoundsLabel1Rect = new Rect(position);
      rangeBoundsLabel1Rect.width = rangeBoundsLabelWidth;
      GUI.Label(rangeBoundsLabel1Rect, new GUIContent(xValue.ToString(isInt ? "F0" : "F2")));
      position.xMin += rangeBoundsLabelWidth;

      var rangeBoundsLabel2Rect = new Rect(position);
      rangeBoundsLabel2Rect.xMin = rangeBoundsLabel2Rect.xMax - rangeBoundsLabelWidth;
      GUI.Label(rangeBoundsLabel2Rect, new GUIContent(yValue.ToString(isInt ? "F0" : "F2")));
      position.xMax -= rangeBoundsLabelWidth;

      EditorGUI.BeginChangeCheck();
      EditorGUI.MinMaxSlider(position, ref xValue, ref yValue, rangeMin, rangeMax);

      if (EditorGUI.EndChangeCheck()) {
        if (isInt) {
          xProp.intValue = Mathf.RoundToInt(xValue);
          yProp.intValue = Mathf.RoundToInt(yValue);
        } else {
          xProp.floatValue = xValue;
          yProp.floatValue = yValue;
        }
      }

      EditorGUI.EndProperty();
    }
  }
}
#endif