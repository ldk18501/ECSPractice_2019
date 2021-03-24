using UnityEngine;
using Unity.Mathematics;
using Unity.Jobs;

using Unity.Burst;
using Unity.Collections;

namespace DOTS_2019
{
    public class SimpleJob : MonoBehaviour
    {
        public bool useJob;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            float startTime = Time.realtimeSinceStartup;

            if (useJob)
            {
                NativeList<JobHandle> jhs = new NativeList<JobHandle>(Allocator.Temp);
                for (int i = 0; i < 5; i++)
                {
                    jhs.Add(DoHeavyMathJob());
                }
                JobHandle.CompleteAll(jhs);
                jhs.Dispose();
            }
            else
            {
                for (int i = 0; i < 5; i++)
                    HeavyMathTask();
            }
            Debug.Log((Time.realtimeSinceStartup - startTime) * 1000 + "ms");
        }

        void HeavyMathTask()
        {
            float value = 0f;
            for (int i = 0; i < 20000; i++)
            {
                value = math.exp10(math.sqrt(value));
            }
        }

        JobHandle DoHeavyMathJob()
        {
            SimpleHeavyMathJob job = new SimpleHeavyMathJob();
            return job.Schedule();

        }
    }

    //[BurstCompile]
    public struct SimpleHeavyMathJob : IJob
    {
        public void Execute()
        {
            float value = 0f;
            for (int i = 0; i < 20000; i++)
            {
                value = math.exp10(math.sqrt(value));
            }
        }
    }
}