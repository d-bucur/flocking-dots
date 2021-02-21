    using Unity.Collections;
    using Unity.Jobs;
    using Unity.Mathematics;

    public class SpatialMap {
        // Would be a component if DOTS had a good way of passing native collections between systems
        public static SpatialMap Instance {
            get {
                if (_instance == null) {
                    _instance = new SpatialMap();
                }
                return _instance;
            }
        }
        private static SpatialMap _instance;
        
        public NativeMultiHashMap<int3, SpatialMapSingleValue> spatialGrid = new NativeMultiHashMap<int3, SpatialMapSingleValue>(5, Allocator.Persistent);
        public JobHandle gridWriterHandle;

        public static int3 GetSpatialHash(in float3 pos, float cellSize) {
            return new int3(
                (int)math.floor(pos.x/cellSize),
                (int)math.floor(pos.y/cellSize),
                (int)math.floor(pos.z/cellSize)
            );
        }
    }
