    using Unity.Entities;

    [GenerateAuthoringComponent]
    public struct BoidSpawnerComponent : IComponentData {
        public Entity prefab;
        public int batchSize;
    }
