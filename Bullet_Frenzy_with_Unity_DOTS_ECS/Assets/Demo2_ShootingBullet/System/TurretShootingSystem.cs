using System;
using System.Diagnostics;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

namespace AIGameMonster.Tanks
{
    [UpdateInGroup(typeof(LateSimulationSystemGroup))] // 指定此系统属于 LateSimulationSystemGroup 组
    public partial struct TurretShootingSystem : ISystem
    {
        public float FireRate; // 炮弹发射频率，每秒发射的炮弹数量
        private float timer; // 计时器

        [BurstCompile] // 使用 Burst 编译器编译此方法
        public void OnCreate(ref SystemState state)
        {
            //UnityEngine.Debug.LogError("00");
            state.RequireForUpdate<TurretShooting>(); // 指定此系统需要更新的组件类型
            FireRate = 10.0f; // 设置每秒发射2颗炮弹
            timer = 0.0f; // 初始化计时器
        }

        [BurstCompile] // 使用 Burst 编译器编译此方法
        public void OnUpdate(ref SystemState state)
        {
            timer += SystemAPI.Time.DeltaTime; // 更新计时器

            // 如果计时器大于等于1/FireRate，则发射一颗炮弹并重置计时器
            if (timer >= 1.0f / FireRate)
            {

                InstantiateGO(state);
                timer -= 1.0f / FireRate; // 重置计时器
            }
           
        }

        private void InstantiateGO(SystemState state)
        {
            // 遍历所有具有 TurretAspect 和 Shooting 组件的实体
            foreach (var (turret, localToWorld) in
                     SystemAPI.Query<TurretAspect, RefRO<LocalToWorld>>()
                         .WithAll<Shooting>())
            {
                Entity instance = state.EntityManager.Instantiate(turret.CannonBallPrefab); // 实例化炮弹预制体

                // 设置炮弹的位置、旋转和缩放
                state.EntityManager.SetComponentData(instance, new LocalTransform
                {
                    Position = SystemAPI.GetComponent<LocalToWorld>(turret.CannonBallSpawn).Position,
                    Rotation = quaternion.identity,
                    Scale = SystemAPI.GetComponent<LocalTransform>(turret.CannonBallPrefab).Scale
                });

                // 设置炮弹的速度
                state.EntityManager.SetComponentData(instance, new CannonBall
                {
                    Velocity = localToWorld.ValueRO.Up * 20.0f
                });

                // 设置炮弹的颜色
                state.EntityManager.SetComponentData(instance, new URPMaterialPropertyBaseColor
                {
                    Value = turret.Color
                });
            }
        }
    }

}
