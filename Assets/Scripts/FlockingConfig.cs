using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public struct FlockingData {
    public float senseRange;
    public float avoidanceFactor;
    public float cohesionFactor;
    public float alignmentFactor;
    public float flockingFactor;
    public float3 target;
    public float targetFactor;
    public float drag;
    public float worldSize;
    public bool isDebugEnabled;
}

[CreateAssetMenu(menuName = "Scriptable Objects/FlockingConfig")]
public class FlockingConfig : ScriptableObject {
    public FlockingData data;
}
