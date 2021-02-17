using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct FlockingComponent : IComponentData {
    public float3 acceleration;
}
