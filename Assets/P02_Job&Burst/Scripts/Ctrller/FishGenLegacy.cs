using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DOTS_2019
{
    public class FishGenLegacy : MonoBehaviour
    {
        [Header("References")]
        public Transform waterObject;
        public Transform objectPrefab;

        [Header("Spawn Settings")]
        public int amountOfFish;
        public Vector3 spawnBounds;

        private List<Vector3> m_FishVels;
        private List<Transform> m_TrsFishes;
        private List<float> m_FishRotTimes;

        private float m_SwinSpeed = 30f;
        private float m_TurnSpeed = 5f;

        private void Start()
        {
            m_FishVels = new List<Vector3>();
            m_TrsFishes = new List<Transform>();
            m_FishRotTimes = new List<float>();

            for (int i = 0; i < amountOfFish; i++)
            {
                // 在规定边界范围内随机生成鱼
                var trsFish = (Transform)Instantiate(objectPrefab,
                    transform.position + new Vector3(
                        Random.Range(-spawnBounds.x / 2, spawnBounds.x / 2),
                        0,
                        Random.Range(-spawnBounds.z / 2, spawnBounds.z / 2)),
                    Quaternion.identity);
                trsFish.parent = this.transform;
                m_TrsFishes.Add(trsFish);

                m_FishVels.Add(new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)));
                m_FishRotTimes.Add(Random.Range(1f, 5f));
            }
        }

        private void Update()
        {

            for (int i = 0; i < m_TrsFishes.Count; i++)
            {
                Vector3 currentVelocity = m_FishVels[i];
                m_TrsFishes[i].position += m_TrsFishes[i].forward * m_SwinSpeed * Time.deltaTime * Random.Range(0.3f, 1.0f);

                if (currentVelocity != Vector3.zero)
                {
                    m_TrsFishes[i].rotation = Quaternion.Lerp(
                        m_TrsFishes[i].rotation,
                        Quaternion.LookRotation(currentVelocity),
                        m_TurnSpeed * Time.deltaTime);
                }
                // 超出边界就朝中心游动
                if (m_TrsFishes[i].position.x > waterObject.position.x + spawnBounds.x / 2 ||
                    m_TrsFishes[i].position.x < waterObject.position.x - spawnBounds.x / 2 ||
                    m_TrsFishes[i].position.z > waterObject.position.z + spawnBounds.z / 2 ||
                    m_TrsFishes[i].position.z < waterObject.position.z - spawnBounds.z / 2)
                {
                    Vector3 internalPosition = new Vector3(waterObject.position.x + Random.Range(-spawnBounds.x / 2, spawnBounds.x / 2), 0,
                        waterObject.position.z + Random.Range(-spawnBounds.z / 2, spawnBounds.z / 2)) * 0.8f;
                    m_FishVels[i] = (internalPosition - m_TrsFishes[i].position).normalized;
                }
                else
                {
                    m_FishRotTimes[i] -= Time.deltaTime;
                    if (m_FishRotTimes[i] <= 0)
                    {
                        m_FishVels[i] = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
                        m_FishRotTimes[i] = Random.Range(1f, 5f);
                    }
                }
            }

        }


        private void OnDrawGizmos()
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireCube(transform.position, spawnBounds);
        }

        private void OnDestroy()
        {
        }
    }
}