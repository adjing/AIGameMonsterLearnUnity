using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace AIGameMonster.Tanks
{
    [BurstCompile]
    partial struct CannonBallSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<CannonBall>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();

            var cannonBallJob = new CannonBallJob
            {
                ECB = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged),
                DeltaTime = SystemAPI.Time.DeltaTime,
                MaxDistance = 30,
                Speed= 30,
            };

            cannonBallJob.Schedule();
        }
    }

    // IJobEntity relies on source generation to implicitly define a query from the signature of the Execute function.
    [BurstCompile]
    public partial struct CannonBallJob : IJobEntity
    {
        /// <summary>
        /// 用于在作业中记录对实体的更改
        /// </summary>
        public EntityCommandBuffer ECB;
        public float DeltaTime;
        public float MaxDistance; // 设置一个最大距离
        public float Speed; // 设置一个最大距离


        void Execute(Entity entity, ref CannonBall cannonBall, ref LocalTransform transform)
        {
            var forward = new float3(0.0f, 0.0f, 1.0f); // 定义前进方向

            transform.Position += forward * DeltaTime * Speed; // 只朝前运动

            // 当距离大于设置的最大距离时删除entity对象
            if (math.length(transform.Position) > MaxDistance)
            {
                ECB.DestroyEntity(entity);
            }
        }
    }

}

/*
   [BurstCompile]
    public partial struct CannonBallJob : IJobEntity
    {
        public EntityCommandBuffer ECB;
        public float DeltaTime;

        void Execute(Entity entity, ref CannonBall cannonBall, ref LocalTransform transform)
        {
           
            var gravity = new float3(0.0f, -9.82f, 0.0f);
            var invertY = new float3(1.0f, -1.0f, 1.0f);

            transform.Position += cannonBall.Velocity * DeltaTime;

            // bounce on the ground
            if (transform.Position.y < 0.0f)
            {
                transform.Position *= invertY;
                cannonBall.Velocity *= invertY * 0.8f;
            }

            cannonBall.Velocity += gravity * DeltaTime;

            var speed = math.lengthsq(cannonBall.Velocity);
            if (speed < 0.1f)
            {
                ECB.DestroyEntity(entity);
            }
        }
    }
 */