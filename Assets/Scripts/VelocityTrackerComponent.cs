using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

/** A copy of the last velocity that is read only when the velocity is updated */
[Serializable]
[GenerateAuthoringComponent]
public struct VelocityTrackerComponent : IComponentData
{
    public float3 velocity;
}
