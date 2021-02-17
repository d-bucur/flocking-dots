using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class BoidFlockingSystem : SystemBase {
    private NativeMultiHashMap<int3, Entity> spatialGrid;
    private FlockingConfig steeringData;

    protected override void OnCreate() {
        spatialGrid = new NativeMultiHashMap<int3, Entity>(100, Allocator.Persistent);
        steeringData = Resources.Load<FlockingConfig>("SteeringConfig");
    }

    protected override void OnUpdate() {
        // Create spatial hashmap
        var steeringDataCaptured = steeringData.data;
        spatialGrid.Clear();
        var boidQuery = GetEntityQuery(typeof(BoidComponent), typeof(Translation));
        if (spatialGrid.Capacity < boidQuery.CalculateEntityCount())
            spatialGrid.Capacity *= 2;
        var spatialGridWriter = spatialGrid.AsParallelWriter();
        Entities.ForEach((in BoidComponent boidData, in Translation translation, in Entity entity) => {
            spatialGridWriter.Add(GetSpatialHash(translation.Value, steeringDataCaptured.senseRange), entity);
        }).ScheduleParallel();

        // Get acceleration from flocking behaviours
        var spatialGridCaptured = spatialGrid;
        Entities
            .WithReadOnly(spatialGridCaptured)
            .ForEach((ref FlockingComponent flocking, in Translation translation, in Entity entity) => {
            var avoidance = float3.zero;
            var cohesion = float3.zero;
            var alignment = float3.zero;
            var neighborCount = 0;
            flocking.acceleration = float3.zero;
            for (var x = -1; x < 1; x++)
                for (var y = -1; y < 1; y++)
                    for (var z = -1; z < 1; z++) {
                        var cellHash = GetSpatialHash(translation.Value + new float3(x, y, z), steeringDataCaptured.senseRange);
                        if (spatialGridCaptured.TryGetFirstValue(cellHash, out var other, out var nextIterator)) {
                            do {
                                if (other == entity)
                                    continue;
                                neighborCount++;
                                var otherTranslation = GetComponent<Translation>(other);
                                var vectorToOther = translation.Value - otherTranslation.Value;
                                var distance = math.length(vectorToOther);
                                if (distance < steeringDataCaptured.senseRange) {
                                    avoidance += /*(steeringDataCaptured.senseRange - distance) **/ vectorToOther;
                                    cohesion += otherTranslation.Value;
                                    var otherVelocity = GetComponent<BoidComponent>(other).velocity;
                                    alignment += otherVelocity;
                                }
                            } while (spatialGridCaptured.TryGetNextValue(out other, ref nextIterator));
                        }
                    }

            flocking.acceleration += avoidance * steeringDataCaptured.avoidanceFactor;
            if (neighborCount > 0) {
                var cohesionDir = cohesion / neighborCount - translation.Value;
                var alignmentDir = alignment / neighborCount;
                flocking.acceleration += alignmentDir * steeringDataCaptured.alignmentFactor;
                flocking.acceleration += cohesionDir * steeringDataCaptured.cohesionFactor;
            }
            flocking.acceleration *= steeringDataCaptured.flockingFactor;
            }).ScheduleParallel();
    }

    protected override void OnDestroy() {
        spatialGrid.Dispose();
    }

    private static int3 GetSpatialHash(in float3 pos, float cellSize) {
        return new int3(
            (int)math.floor(pos.x/cellSize),
            (int)math.floor(pos.y/cellSize),
            (int)math.floor(pos.z/cellSize)
        );
    }
}
