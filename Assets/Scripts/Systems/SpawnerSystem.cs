    using Unity.Entities;
    using Unity.Mathematics;
    using UnityEngine;
    using Random = UnityEngine.Random;

    public class SpawnerSystem : SystemBase {
        private EndSimulationEntityCommandBufferSystem _ecbSystem;

        protected override void OnCreate() {
            base.OnCreate();
            Application.targetFrameRate = 30;
            _ecbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate() {
            var ecb = _ecbSystem.CreateCommandBuffer();
            Entities.ForEach((in BoidSpawnerComponent spawner) => {
                if (!Input.GetKey(KeyCode.Space)) return;
                for (int i = 0; i < spawner.batchSize; i++) {
                    var entity = ecb.Instantiate(spawner.prefab);
                    var randomDir = new float3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
                    ecb.SetComponent(entity, new BoidComponent() {velocity = randomDir});
                }
            }).Run();
            _ecbSystem.AddJobHandleForProducer(Dependency);
        }
    }
