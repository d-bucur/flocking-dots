using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidSpawner : MonoBehaviour {
    public GameObject boid;
    public int batchSize;
    public Transform area;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Space)) {
            var areaSize = area.localScale.x / 2.0f;
            for (var i = 0; i < batchSize; i++) {
                var pos = new Vector3(
                    Random.Range(-areaSize, areaSize),
                    Random.Range(-areaSize, areaSize),
                    Random.Range(-areaSize, areaSize)
                );
                Instantiate(boid, pos, Quaternion.identity);
            }
        }
    }
}
