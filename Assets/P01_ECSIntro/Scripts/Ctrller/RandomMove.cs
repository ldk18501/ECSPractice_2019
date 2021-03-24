using UnityEngine;

namespace DOTS_2019
{
    public class PingPongMove : MonoBehaviour
    {
        [SerializeField] private float m_Speed;
        [SerializeField] private float m_LifeTime;

        void Start()
        {
            m_Speed = Random.Range(1f, 3f);
            m_LifeTime = Random.Range(0f, 100f);
        }

        void Update()
        {
            m_LifeTime += Time.deltaTime;

            transform.Translate(Vector3.up * m_Speed * Time.deltaTime, Space.World);
            if (transform.position.y > 5f)
            {
                m_Speed = -1 * Mathf.Abs(m_Speed);
            }
            if (transform.position.y < -5f)
            {
                m_Speed = Mathf.Abs(m_Speed);
            }
        }
    }
}