using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using System;
using Random = UnityEngine.Random;

[RequireComponent(typeof(BoxCollider2D))]
public class FishSpawner : MonoBehaviour {
  public Spawnable[] spawnables;
  public int maxSpawns = 6;
  public float2 spawnDelay = new float2(3, 10);

  public int fishCount { get; set; }

  private float nextSpawn = float.PositiveInfinity;

  private BoxCollider2D bc;

  [System.Serializable]
  public class Spawnable {
    public GameObject prefab;
    public float weight = 1;
  }

  // Start is called before the first frame update
  void Start() {
    bc = GetComponent<BoxCollider2D>();
    fishCount = GameObject.FindObjectsOfType<FishBehaviour2D>().Length;
    foreach (var fb in GameObject.FindObjectsOfType<FishBehaviour2D>()) {
      Destroy(fb);
    }
  }

  // Update is called once per frame
  void Update() {
    if (nextSpawn == float.PositiveInfinity) {
      if (fishCount < maxSpawns)
        nextSpawn = Random.Range(spawnDelay.x, spawnDelay.y);
    } else if (Time.time >= nextSpawn) {
      SpawnFish();
    }
  }

  void SpawnFish() {
    nextSpawn = float.PositiveInfinity;
    var totalWeight = 0f;
    foreach (var spwn in spawnables) {
      totalWeight += spwn.weight;
    }
    var rand = Random.Range(0, totalWeight);
    var currentWeight = 0f;
    foreach (var spwn in spawnables) {
      if (rand > currentWeight && rand <= currentWeight + spwn.weight) {
        var fish = Instantiate(spwn.prefab, GetRandomPos(bc.bounds), Quaternion.identity);
        fishCount++;
        fish.GetComponent<FishBehaviour2D>().nextBurstTime = Time.time;
        fish.GetComponent<Rigidbody2D>().velocity = Vector3.right;
        return;
      }
      totalWeight += currentWeight;
    }
  }

  Vector3 GetRandomPos(Bounds bounds) {
    return new Vector3(
        Random.Range(bounds.min.x, bounds.max.x),
        Random.Range(bounds.min.y, bounds.max.y),
        Random.Range(bounds.min.z, bounds.max.z)
    );
  }
}
