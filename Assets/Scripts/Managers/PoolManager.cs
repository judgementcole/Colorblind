using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PoolManager : MonoSingleton<PoolManager>
{
    #region Variables
    [Header("Detection Stats")]
    [SerializeField] private GameObject _detectionStatsPoolFolder;
    [SerializeField] private GameObject _detectionStatsPrefab;
    private List<DetectionStats> _detectionStatsPool = new List<DetectionStats>();

    [Header("Soul Orb")]
    [SerializeField] private GameObject _soulOrbsPoolFolder;
    [SerializeField] private GameObject _soulOrbPrefab;
    private List<SoulOrb> _soulOrbsPool = new List<SoulOrb>();
    #endregion

    #region Unity Callbacks
    private void Start()
    {
        DetectionStats[] detectionStatsArray = _detectionStatsPoolFolder.GetComponentsInChildren<DetectionStats>();
        _detectionStatsPool = detectionStatsArray.ToList();

        SoulOrb[] soulOrbArray = _soulOrbsPoolFolder.GetComponentsInChildren<SoulOrb>();
        _soulOrbsPool = soulOrbArray.ToList();
    }
    #endregion

    #region Enemy Detection Stats
    public void RequestDetectionStats(Vector3 position, float reactionTime)
    {
        foreach (var detectionStats in _detectionStatsPool)
        {
            if (!detectionStats.inUse)
            {
                detectionStats.PlayDetectionStatsAnimation(position, reactionTime);

                return;
            }
        }

        // Creates new object and puts it in the pool if all objects in pool are in use
        DetectionStats newObjectInPool = CreateNewObjectInPool(_detectionStatsPrefab, _detectionStatsPoolFolder).GetComponentInChildren<DetectionStats>();
        _detectionStatsPool.Add(newObjectInPool);
    
        newObjectInPool.PlayDetectionStatsAnimation(position, reactionTime);
    }
    #endregion

    #region Soul Orbs
    public void RequestSoulOrb(SoulDoor soulDoor, Vector3 enemyDeathPosition)
    {
        foreach (var soulOrb in _soulOrbsPool)
        {
            if (!soulOrb.inUse)
            {
                soulOrb.AnimateTowardsDoor(soulDoor, enemyDeathPosition);

                return;
            }
        }

        // Creates new object and puts it in the pool if all objects in pool are in use
        SoulOrb newObjectInPool = CreateNewObjectInPool(_soulOrbPrefab, _soulOrbsPoolFolder).GetComponentInChildren<SoulOrb>();
        _soulOrbsPool.Add(newObjectInPool);

        newObjectInPool.AnimateTowardsDoor(soulDoor, enemyDeathPosition);
    }
    #endregion

    #region Pool Helper Functions
    public GameObject CreateNewObjectInPool(GameObject objectToCreate, GameObject poolFolder)
    {
        GameObject objectToReturn = Instantiate(objectToCreate, poolFolder.transform);

        return objectToReturn;
    }
    #endregion
}
