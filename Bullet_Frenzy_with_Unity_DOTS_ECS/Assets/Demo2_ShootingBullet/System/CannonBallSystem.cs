using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace AIGameMonster.Tanks
{
    [BurstCompile] // 使用 Burst 编译器编译此结构体
    partial struct CannonBallSystem : ISystem // 定义一个名为 CannonBallSystem 的部分结构体，它实现了 ISystem 接口
    {
        [BurstCompile] // 使用 Burst 编译器编译此方法
        public void OnCreate(ref SystemState state) // 定义 OnCreate 方法，它在系统创建时被调用
        {
            state.RequireForUpdate<CannonBall>(); // 指定此系统仅在存在 CannonBall 组件时才会更新
        }

        [BurstCompile] // 使用 Burst 编译器编译此方法
        public void OnUpdate(ref SystemState state) // 定义 OnUpdate 方法，它在每一帧中被调用
        {
            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>(); // 获取 EndSimulationEntityCommandBufferSystem 的单例

            var cannonBallJob = new CannonBallJob // 定义一个名为 cannonBallJob 的 CannonBallJob 实例
            {
                ECB = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged), // 设置 ECB 属性为 EndSimulationEntityCommandBufferSystem 的命令缓冲区
                DeltaTime = SystemAPI.Time.DeltaTime, // 设置 DeltaTime 属性为当前帧的时间增量
                MaxDistance = 30, // 设置 MaxDistance 属性为 30
                Speed = 30, // 设置 Speed 属性为 30
            };

            cannonBallJob.Schedule(); // 调度 cannonBallJob 以执行
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