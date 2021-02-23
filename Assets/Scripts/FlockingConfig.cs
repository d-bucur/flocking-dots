using System;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public struct FlockingData {
    public float senseRange;
    public float avoidanceFactor;
    public float cohesionFactor;
    public float alignmentFactor;
    public float flockingFactor;
    public float boundsFactor;
    public float3 target;
    public float targetFactor;
    public float maxAcceleration;
    public float minAcceleration;
    public float drag;
    public float worldSize;
    public bool isDebugEnabled;
    public float lookForwardDistance;
    public float obstacleAvoidanceFactor;
}

[CreateAssetMenu(menuName = "Scriptable Objects/FlockingConfig")]
public class FlockingConfig : ScriptableObject {
    public FlockingData data;
}
