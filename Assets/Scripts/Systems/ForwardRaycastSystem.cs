    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Physics;
    using Unity.Physics.Systems;
    using Unity.Transforms;
    using UnityEngine;

    public class ForwardRaycastSystem : SystemBase {
        private FlockingConfig steeringData;

        protected override void OnCreate() {
            steeringData = Resources.Load<FlockingConfig>("SteeringConfig");
        }
        
        protected override void OnUpdate() {
            var physicsWorldSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystem<BuildPhysicsWorld>();
            var collisionWorld = physicsWorldSystem.PhysicsWorld.CollisionWorld;
            var collisionFilter = new CollisionFilter()
            {
                BelongsTo = ~0u,
                CollidesWith = 1 << 1,
                GroupIndex = 0
            };
            var steeringData = this.steeringData.data;
            Entities
                .WithReadOnly(collisionWorld)
                .ForEach((ref BoidForwardRaycastComponent result, in Translation translation, in PhysicsVelocity velocity) => {
                    result.surfaceNormal = float3.zero;
                    if (steeringData.obstacleAvoidanceFactor == 0)
                        return;
                    
                    var position = translation.Value;
                    var input = new RaycastInput {
                        Start = position,
                        End = position + math.normalizesafe(velocity.Linear) * steeringData.lookForwardDistance,
                        Filter = collisionFilter
                    };
                    bool haveHit = collisionWorld.CastRay(input, out var hit);
                    if (!haveHit) return;
                    
                    result.hitPosition = hit.Position;
                    result.surfaceNormal = hit.SurfaceNormal;
                })
                .ScheduleParallel();
            BoidForwardRaycastComponent.writerHandle = Dependency;
        }
    }
