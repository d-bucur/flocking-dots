    using Unity.Entities;
    using UnityEngine;

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
                for (int i = 0; i < spawner.batchSize; i++)
                    ecb.Instantiate(spawner.prefab);
            }).Run();
            _ecbSystem.AddJobHandleForProducer(Dependency);
        }
    }
