    using Unity.Entities;
    using Unity.Jobs;
    using Unity.Mathematics;
    using Unity.Physics;
    using Unity.Transforms;
    using UnityEngine;

    public class ForwardRaycastSystem : SystemBase {
        public static JobHandle writeHandle;
        private FlockingConfig steeringData;

        protected override void OnCreate() {
            steeringData = Resources.Load<FlockingConfig>("SteeringConfig");
        }
        
        protected override void OnUpdate() {
            var physicsWorldSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystem<Unity.Physics.Systems.BuildPhysicsWorld>();
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
                    var position = translation.Value;
                    RaycastInput input = new RaycastInput()
                    {
                        Start = position,
                        End = position + math.normalizesafe(velocity.Linear) * steeringData.lookForwardDistance,
                        Filter = collisionFilter
                    };

                    bool haveHit = collisionWorld.CastRay(input, out var hit);
                    if (!haveHit) return;
                    var dirToHit = hit.Position - position;
                    var reflection = math.reflect(dirToHit, hit.SurfaceNormal);
                    Debug.DrawRay(position, dirToHit, Color.red);
                    Debug.DrawRay(hit.Position, reflection, Color.yellow);
                    result.hitPosition = hit.Position;
                    result.surfaceNormal = hit.SurfaceNormal;
                })
                .Schedule();
            writeHandle = Dependency;
        }
    }
