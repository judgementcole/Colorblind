using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayParticles : MonoBehaviour
{
    #region Variables
    [Header("Color")]
    [SerializeField] private Colors.colorName _colorName;
    private Color currentColor;

    [Header("Components")]
    private ParticleSystem _particles;
    #endregion

    #region Unity Callbacks
    private void Awake()
    {
        _particles = GetComponent<ParticleSystem>();
    }

    private void Start()
    {
        SetParticleColorToColorName();
    }

    private void OnEnable()
    {
        ColorManager.Instance.OnFCPColorChange += SetParticleColorToColorName;
        GameManager.Instance.OnRespawnStart += SetParticleColorToWhite;
        GameManager.Instance.OnRespawnComplete += SetParticleColorToColorName;
    }

    private void OnDisable()
    {
        ColorManager.Instance.OnFCPColorChange -= SetParticleColorToColorName;
        GameManager.Instance.OnRespawnStart -= SetParticleColorToWhite;
        GameManager.Instance.OnRespawnComplete -= SetParticleColorToColorName;
    }
    #endregion

    #region To White Color
    private void SetParticleColorToWhite()
    {
        currentColor = Color.white;

        var main = _particles.main;
        main.startColor = Color.white;

        var particleCount = _particles.particleCount;
        var particles = new ParticleSystem.Particle[particleCount];

        _particles.GetParticles(particles);

        for (int i = 0; i < particles.Length; i++)
        {
            particles[i].startColor = Color.white;
        }

        _particles.SetParticles(particles, particleCount);
    }
    #endregion

    #region To Actual Color
    private void SetParticleColorToColorName()
    {
        if (currentColor != Colors.ConvertEnumToColor(_colorName))
        {
            if ((GameManager.Instance._inCustomizationScreen && GameManager.Instance.spawned)
                || !GameManager.Instance._inCustomizationScreen)
            {
                currentColor = Colors.ConvertEnumToColor(_colorName);

                var main = _particles.main;
                main.startColor = currentColor;

                var particleCount = _particles.particleCount;
                var particles = new ParticleSystem.Particle[particleCount];

                _particles.GetParticles(particles);

                for (int i = 0; i < particles.Length; i++)
                {
                    particles[i].startColor = currentColor;
                }

                _particles.SetParticles(particles, particleCount);
            }
        }
    }

    #region Rainbow Colored Particles
    //IEnumerator SetRainbowGraidentEOF()
    //{
    //    yield return new WaitForEndOfFrame();

    //    SetParticleColorToRainbowGradient();
    //}

    //private void SetParticleColorToRainbowGradient()
    //{
    //    var main = _particles.main;
    //    main.startColor = new ParticleSystem.MinMaxGradient(ColorManager.Instance.rainbowGradient);

    //    var particleCount = _particles.particleCount;
    //    var particles = new ParticleSystem.Particle[particleCount];

    //    _particles.GetParticles(particles);

    //    for (int i = 0; i < particles.Length; i++)
    //    {
    //        particles[i].startColor = ColorManager.Instance.rainbowGradient.Evaluate(Random.Range(0f, 1f));
    //    }

    //    _particles.SetParticles(particles, particleCount);
    //}
    #endregion
    #endregion
}
