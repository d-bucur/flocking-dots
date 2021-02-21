    using Unity.Collections;
    using Unity.Entities;
    using Unity.Jobs;
    using Unity.Mathematics;
    using Unity.Transforms;
    using UnityEngine;

    public class SpatialHashingSystem : SystemBase {
        private FlockingConfig steeringData;

        protected override void OnCreate() {
            base.OnCreate();
            steeringData = Resources.Load<FlockingConfig>("SteeringConfig");
        }

        protected override void OnUpdate() {
            var spatialGrid = SpatialMap.Instance.spatialGrid;
            spatialGrid.Clear();
            var steeringDataCaptured = steeringData.data;
            var boidQuery = GetEntityQuery(
                ComponentType.ReadOnly(typeof(BoidVelocityComponent)),
                ComponentType.ReadOnly(typeof(Translation)));
            int valuesCount = boidQuery.CalculateEntityCount() * 27; // each value gets written in 27 cells
            if (spatialGrid.Capacity < valuesCount) {
                spatialGrid.Capacity = valuesCount * 2;
                // Debug.Log($"Resizing map to {spatialGrid.Capacity}");
            }
            var spatialGridWriter = spatialGrid.AsParallelWriter();

            Entities.ForEach((in BoidVelocityComponent boidData, in Translation translation, in Entity entity) => {
                var key = SpatialMap.GetSpatialHash(translation.Value, steeringDataCaptured.senseRange);
                // Debug.Log($"Hashmap Write {entity} to {key} and around");
                for (var x = -1; x <= 1; x++) {
                    for (var y = -1; y <= 1; y++) {
                        for (var z = -1; z <= 1; z++) {
                            var writeKey = key + new int3(x, y, z);
                            var v = new SpatialMapSingleValue() {
                                Translation = translation.Value,
                                Velocity = boidData.Value
                            };
                            spatialGridWriter.Add(writeKey, v);
                        }
                    }
                }
            }).ScheduleParallel();
            SpatialMap.Instance.gridWriterHandle = Dependency;
        }

        protected override void OnDestroy() {
            SpatialMap.Instance.spatialGrid.Dispose();
        }
    }
