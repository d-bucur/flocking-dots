using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public struct BoidsData {
    public float senseRange;
    public float avoidanceFactor;
    public float cohesionFactor;
    public float alignmentFactor;
    public float flockingFactor;
    public float drag;
}

[CreateAssetMenu(menuName = "Scriptable Objects/SteeringBehaviour")]
public class SteeringBehaviour : ScriptableObject {
    public BoidsData data;
}
