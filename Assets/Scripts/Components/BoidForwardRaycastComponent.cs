    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Physics;

    [GenerateAuthoringComponent]
    public struct BoidForwardRaycastComponent : IComponentData {
        public float3 hitPosition;
        public float3 surfaceNormal;
    }
