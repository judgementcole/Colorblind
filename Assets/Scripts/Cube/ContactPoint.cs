using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContactPoint : MonoBehaviour
{
    #region Variables
    [Header("Components")]
    private ParticleSystem _collisionParticles;
    #endregion

    #region Unity Callbacks
    private void Start()
    {
        _collisionParticles = GetComponent<ParticleSystem>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {  
            _collisionParticles.Play();
        }
    }
    #endregion
}
