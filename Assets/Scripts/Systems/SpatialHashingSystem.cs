    using Unity.Collections;
    using Unity.Entities;
    using Unity.Jobs;
    using Unity.Mathematics;
    using Unity.Transforms;
    using UnityEngine;

    public class SpatialHashingSystem : SystemBase {
        // TODO move to singleton
        public NativeMultiHashMap<int3, Entity> spatialGrid;
        public JobHandle gridWriterHandle;
        private FlockingConfig steeringData;

        protected override void OnCreate() {
            base.OnCreate();
            spatialGrid = new NativeMultiHashMap<int3, Entity>(5, Allocator.Persistent);
            steeringData = Resources.Load<FlockingConfig>("SteeringConfig");
        }

        protected override void OnUpdate() {
            spatialGrid.Clear();
            var steeringDataCaptured = steeringData.data;
            var boidQuery = GetEntityQuery(
                ComponentType.ReadOnly(typeof(BoidComponent)),
                ComponentType.ReadOnly(typeof(Translation)));
            int valuesCount = boidQuery.CalculateEntityCount() * 27; // each value gets written in 27 cells
            if (spatialGrid.Capacity < valuesCount) {
                spatialGrid.Capacity = valuesCount * 2;
                // Debug.Log($"Resizing map to {spatialGrid.Capacity}");
            }
            var spatialGridWriter = spatialGrid.AsParallelWriter();
            // TODO parallel writer not reallocating size correctly
            // Debug.Log("==============New hashmap============");

            Entities.ForEach((in BoidComponent boidData, in Translation translation, in Entity entity) => {
                var key = GetSpatialHash(translation.Value, steeringDataCaptured.senseRange);
                // Debug.Log($"Hashmap Write {entity} to {key} and around");
                for (var x = -1; x <= 1; x++) {
                    for (var y = -1; y <= 1; y++) {
                        for (var z = -1; z <= 1; z++) {
                            var writeKey = key + new int3(x, y, z);
                            spatialGridWriter.Add(writeKey, entity);
                        }
                    }
                }
            }).ScheduleParallel();
            gridWriterHandle = Dependency;
        }

        protected override void OnDestroy() {
            spatialGrid.Dispose();
        }

        public static int3 GetSpatialHash(in float3 pos, float cellSize) {
            return new int3(
                (int)math.floor(pos.x/cellSize),
                (int)math.floor(pos.y/cellSize),
                (int)math.floor(pos.z/cellSize)
            );
        }
    }
