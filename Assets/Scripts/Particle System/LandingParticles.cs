using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandingParticles : MonoBehaviour
{
    #region Variables
    [Header("Components")]
    private ParticleSystem landingParticles;
    #endregion

    #region Unity Callbacks
    private void Awake()
    {
        landingParticles = GetComponent<ParticleSystem>();
    }
    #endregion

    #region Particles
    public void PlayLandingParticles(Vector2 raycastHitPos)
    {
        transform.position = raycastHitPos;
        landingParticles.Play();
    }
    #endregion
}
