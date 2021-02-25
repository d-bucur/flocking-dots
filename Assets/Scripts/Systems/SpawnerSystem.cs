    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Physics;
    using Unity.Transforms;
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
            if (!Input.GetKey(KeyCode.Space)) return;
            var spawner = GetSingleton<BoidSpawnerComponent>();
            var ecb = _ecbSystem.CreateCommandBuffer();
            for (int i = 0; i < spawner.batchSize; i++) {
                var entity = ecb.Instantiate(spawner.prefab);
                var randFloat3 = new float3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
                ecb.SetComponent(entity, new PhysicsVelocity() {Linear = randFloat3});
                ecb.SetComponent(entity, new Translation { Value = randFloat3 * 5});
            }
            _ecbSystem.AddJobHandleForProducer(Dependency);
        }
    }
