using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct BoidAccelerationComponent : IComponentData {
    public float3 Value;
}
