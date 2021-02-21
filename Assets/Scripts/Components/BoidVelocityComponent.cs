using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
[GenerateAuthoringComponent]
public struct BoidVelocityComponent : IComponentData
{
    public float3 Value;
}
