using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using UnityEngine.Jobs;
using UnityEngine;

using URandom = Unity.Mathematics.Random;

namespace DOTS_2019
{
    public class FishGen : MonoBehaviour
    {
        [Header("References")]
        public Transform waterObject;
        public Transform objectPrefab;

        [Header("Spawn Settings")]
        public int amountOfFish;
        public Vector3 spawnBounds;

        private NativeArray<Vector3> m_FishVels;
        // 由于Transform是引用类型，不能用于创建NativeContainer，所以Unity提供了TransformAccessArray，将Transform的信息以值类型的方式提取出来
        private TransformAccessArray m_TrsAccessArray;
        private NativeArray<float> m_FishRotTimes;

        private JobHandle m_JobHandle;
        private FishTrsMoveJob m_MoveJob;

        private float m_SwinSpeed = 30f;
        private float m_TurnSpeed = 5f;

        void Start()
        {
            // 修改为符合Jobs要求的数据结构
            m_FishVels = new NativeArray<Vector3>(amountOfFish, Allocator.Persistent);
            m_TrsAccessArray = new TransformAccessArray(amountOfFish);
            m_FishRotTimes = new NativeArray<float>(amountOfFish, Allocator.Persistent);

            for (int i = 0; i < amountOfFish; i++)
            {
                // 在规定边界范围内随机生成鱼
                var trsFish = (Transform)Instantiate(objectPrefab,
                    transform.position + new Vector3(
                        UnityEngine.Random.Range(-spawnBounds.x / 2, spawnBounds.x / 2),
                        0,
                        UnityEngine.Random.Range(-spawnBounds.z / 2, spawnBounds.z / 2)),
                    Quaternion.identity);


                // 在用JobsParallelForTransform的情况下，transform不能有parent，否则都会被视为一个transform来处理，无法分别运行在多个核上
                // 详情见：https://forum.unity.com/threads/jobs-performance-questions.520406/
                //trsFish.parent = this.transform;
                m_TrsAccessArray.Add(trsFish);
                m_FishRotTimes[i] = Random.Range(1f, 5f);
                m_FishVels[i] = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
            }
        }

        void Update()
        {
            m_MoveJob = new FishTrsMoveJob
            {
                fishVels = m_FishVels,
                fishRotLeftTimes = m_FishRotTimes,

                spawnBounds = spawnBounds,
                centerPos = waterObject.transform.position,
                swimSpeed = m_SwinSpeed,
                turnSpeed = m_TurnSpeed,
                deltaTime = Time.deltaTime,
                time = Time.time,
                seed = System.DateTimeOffset.Now.Millisecond,
            };
            m_JobHandle = m_MoveJob.Schedule(m_TrsAccessArray);
        }


        void LateUpdate()
        {
            m_JobHandle.Complete();
        }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireCube(transform.position, spawnBounds);
        }

        void OnDestroy()
        {
            m_FishVels.Dispose();
            m_TrsAccessArray.Dispose();
            m_FishRotTimes.Dispose();
        }


        [BurstCompile]
        public struct FishTrsMoveJob : IJobParallelForTransform
        {
            public NativeArray<Vector3> fishVels;
            public NativeArray<float> fishRotLeftTimes;
            public Vector3 spawnBounds;
            public Vector3 centerPos;
            public float swimSpeed;
            public float turnSpeed;
            public float deltaTime;
            public float time;
            public float seed;

            public void Execute(int index, TransformAccess transform)
            {
                Vector3 currentVelocity = fishVels[index];
                // 在Job中不允许用UnityEngine的Random.Range方法，只能使用MathRandom:
                // UnityException: Range can only be called from the main thread. Constructors and field initializers will be executed from the loading thread when loading a scene.
                URandom ranGen = new URandom((uint)(index * time + 1 + seed));

                transform.position += transform.localToWorldMatrix.MultiplyVector(Vector3.forward * swimSpeed * deltaTime * ranGen.NextFloat(0.3f, 1.0f));

                if (currentVelocity != Vector3.zero)
                {
                    transform.rotation = Quaternion.Lerp(
                        transform.rotation,
                        Quaternion.LookRotation(currentVelocity),
                        turnSpeed * deltaTime);
                }
                // 超出边界就朝中心游动
                if (transform.position.x > centerPos.x + spawnBounds.x / 2 ||
                    transform.position.x < centerPos.x - spawnBounds.x / 2 ||
                    transform.position.z > centerPos.z + spawnBounds.z / 2 ||
                    transform.position.z < centerPos.z - spawnBounds.z / 2)
                {
                    Vector3 internalPosition = new Vector3(centerPos.x + ranGen.NextFloat(-spawnBounds.x / 2, spawnBounds.x / 2), 0,
                        centerPos.z + ranGen.NextFloat(-spawnBounds.z / 2, spawnBounds.z / 2)) * 0.8f;
                    fishVels[index] = (internalPosition - transform.position).normalized;
                }
                else
                {
                    fishRotLeftTimes[index] -= deltaTime;
                    if (fishRotLeftTimes[index] <= 0)
                    {
                        fishVels[index] = new Vector3(ranGen.NextFloat(-1f, 1f), 0, ranGen.NextFloat(-1f, 1f));
                        fishRotLeftTimes[index] = ranGen.NextFloat(1f, 5f);
                    }
                }
            }
        }
    }


}