

using UnityEngine;

namespace MyCollection {
  public static partial class MyRandom {
    // https://answers.unity.com/questions/1353041/deltatime-dependent-random-wander-math-problem.html
    public static float DeltaAdjustedChance(float chance) => 1 - Mathf.Pow(1 - chance, Time.deltaTime);
  }
}
