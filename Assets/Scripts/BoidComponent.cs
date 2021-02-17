using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
[GenerateAuthoringComponent]
public struct BoidComponent : IComponentData
{
    public float3 acceleration;
    public float3 velocity;
}
