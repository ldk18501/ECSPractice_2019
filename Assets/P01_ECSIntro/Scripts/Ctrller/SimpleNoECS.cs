using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DOTS_2019
{
    public class SimpleNoECS : MonoBehaviour
    {
        [SerializeField] private GameObject m_Prefab;
        void Start()
        {
            for (int i = 0; i < 50000; i++)
            {
                (GameObject.Instantiate(
                    m_Prefab,
                    new Vector3(Random.Range(-10f, 10f), Random.Range(-5f, 5f), 0),
                    Quaternion.identity) as GameObject)
                .AddComponent<PingPongMove>();
            }
        }


        void Update()
        {

        }
    }
}