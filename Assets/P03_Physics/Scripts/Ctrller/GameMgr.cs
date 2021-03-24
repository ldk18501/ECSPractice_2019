using System.Collections;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;
using Unity.Entities;
using Unity.Physics;

using Random = UnityEngine.Random;

public class GameMgr : MonoBehaviour
{
    private static int m_GameScore;

    private EntityManager m_EntityMgr;
    private BlobAssetStore m_BlobAssetStore;
    private GameObjectConversionSettings m_ConversionSettings;

    [SerializeField] private GameObject m_BallPrefab;
    [SerializeField] private GameObject m_CubePrefab;
    [SerializeField] private int m_CubeCount;

    private Entity m_BallEntityPrefab;
    private Entity m_CubeEntityPrefab;
    private int m_Score;
    private bool m_GameOver;

    void Awake()
    {
        m_EntityMgr = World.DefaultGameObjectInjectionWorld.EntityManager;
        m_BlobAssetStore = new BlobAssetStore();
        m_ConversionSettings = GameObjectConversionSettings.FromWorld(m_EntityMgr.World, m_BlobAssetStore);

        m_BallEntityPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(m_BallPrefab, m_ConversionSettings);
        m_CubeEntityPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(m_CubePrefab, m_ConversionSettings);
    }

    void Start()
    {
        m_GameScore = 0;
        m_GameOver = false;
        SpawnBall();
    }

    void Update()
    {
        if (!m_GameOver && m_GameScore >= 8)
        {
            m_GameOver = true;
            StartCoroutine(SpawnLotsOfCubes());
        }
    }

    void OnDestroy()
    {
        if (m_BlobAssetStore != null)
        {
            m_BlobAssetStore.Dispose();
            m_BlobAssetStore = null;
        }
    }

    void SpawnBall()
    {
        Entity newBallEntity = m_EntityMgr.Instantiate(m_BallEntityPrefab);
        m_EntityMgr.SetComponentData(newBallEntity, new Translation { Value = new float3(0, 2f, 0) });
    }

    public static void AddScore()
    {
        m_GameScore += 1;
        Debug.Log(m_GameScore);
    }

    IEnumerator SpawnLotsOfCubes()
    {
        for (int i = 0; i < m_CubeCount; i++)
        {
            Entity newCubeEntity = m_EntityMgr.Instantiate(m_CubeEntityPrefab);

            m_EntityMgr.SetComponentData(newCubeEntity, new Translation { Value = new float3(Random.Range(-1f, 1f), 1f, Random.Range(-1f, 1f)) });
            m_EntityMgr.SetComponentData(newCubeEntity, new PhysicsVelocity { Linear = Vector3.up * 1.5f });
            yield return null;
        }
        yield return null;
    }
}
