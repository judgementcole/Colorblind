using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoSingleton<EnemyManager>
{
    #region Variables
    [Header("Enemy Colliders")]
    [HideInInspector] public List<BoxCollider2D> enemyColliders;

    [Header("Player Components")]
    public Transform playerTransform;
    public BoxCollider2D playerCollider;
    #endregion

    #region Enemy Colliders
    public void GetEnemyColliders()
    {
        var enemiesInLevel = GameObject.FindGameObjectsWithTag("Enemy");
    
        foreach (var enemy in enemiesInLevel)
        {
            var enemyCollider = enemy.GetComponent<BoxCollider2D>();
            enemyColliders.Add(enemyCollider);
        }
    }
    #endregion
}
