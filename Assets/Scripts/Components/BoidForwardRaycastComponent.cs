    using Unity.Entities;
    using Unity.Jobs;
    using Unity.Mathematics;

    [GenerateAuthoringComponent]
    public struct BoidForwardRaycastComponent : IComponentData {
        public static JobHandle writerHandle;
        
        public float3 hitPosition;
        public float3 surfaceNormal;
    }
