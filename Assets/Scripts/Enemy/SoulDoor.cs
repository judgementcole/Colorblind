using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SoulDoor : MonoBehaviour
{
    // Apply sprite masks to overlapping sprites first to avoid transparent sprite overlap issue
    // (Not sure if it works)

    #region Variables
    [Header("Soul Door")]
    [SerializeField] private int _soulsRequired = 1;
    private int _currentSouls;
    [HideInInspector] public int freeSoulSlot = 1; // Used for filling soul slots with soul orbs
    private float fadeOutDuration = 1f;
    private bool _opened = false;
    public GameObject[] soulSlots;
    private Coroutine _fadeOutRoutine;

    [Header("Components")]
    private BoxCollider2D[] _boxColliders;
    private SpriteRenderer[] _spriteRenderers;
    private Tilemap[] _tilemaps;
    private List<SoulOrb> _soulOrbs = new List<SoulOrb>();
    private List<ParticleSystem> _soulOrbsParticleSystems = new List<ParticleSystem>();
    #endregion

    #region Unity Callbacks
    private void Awake()
    {
        _boxColliders = GetComponentsInChildren<BoxCollider2D>();
        _spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        _tilemaps = GetComponentsInChildren<Tilemap>();
    }

    private void OnEnable()
    {
        GameManager.Instance.OnRespawnStart += ResetStats;
    }

    private void OnDisable()
    {
        GameManager.Instance.OnRespawnStart -= ResetStats;
    }
    #endregion

    #region Soul Door
    public void SoulAcquired(ParticleSystem particleSystem, SoulOrb soulOrbScript)
    {
        _currentSouls++;

        if (particleSystem != null)
        {
            _soulOrbsParticleSystems.Add(particleSystem);
            _soulOrbs.Add(soulOrbScript);
        }

        AllSoulsAcquiredCheck();
    }

    private void AllSoulsAcquiredCheck()
    {
        if (_currentSouls == _soulsRequired && !_opened)
        {
            OpenDoor();
        }
    }

    private void OpenDoor()
    {
        _opened = true;

        foreach (var collider in _boxColliders)
        {
            collider.enabled = false;
        }

        foreach (var soulOrbPs in _soulOrbsParticleSystems)
        {
            soulOrbPs.Stop();
        }

        foreach (var soulOrb in _soulOrbs)
        {
            soulOrb.inUse = false;
        }

        _fadeOutRoutine = StartCoroutine(FadeOut());
    }
    #endregion

    #region Reset
    private void ResetStats()
    {
        MakeOpaque();
        _soulOrbsParticleSystems.Clear();

        foreach (var collider in _boxColliders)
        {
            collider.enabled = true;
        }

        _opened = false;
        _currentSouls = 0;
        freeSoulSlot = 1;
    }
    #endregion

    #region Color Alpha Editing
    private void MakeOpaque()
    {
        foreach (var spriteRenderer in _spriteRenderers)
        {
            Color spriteColor = spriteRenderer.color;
            spriteColor.a = 1;
            spriteRenderer.color = spriteColor;
        }

        foreach (var tilemap in _tilemaps)
        {
            Color tileMap = tilemap.color;
            tileMap.a = 1;
            tilemap.color = tileMap;
        }
    }

    IEnumerator FadeOut()
    {
        float lerp = 0;

        while (_spriteRenderers[0].color.a > 0)
        {
            foreach (var spriteRenderer in _spriteRenderers)
            {
                Color spriteColor = spriteRenderer.color;
                spriteColor.a = Mathf.Lerp(1, 0, lerp);
                spriteRenderer.color = spriteColor;
            }

            foreach (var tilemap in _tilemaps)
            {
                Color tileMap = tilemap.color;
                tileMap.a = Mathf.Lerp(1, 0, lerp);
                tilemap.color = tileMap;
            }

            lerp += Time.deltaTime / fadeOutDuration;

            yield return null;
        }
    }
    #endregion
}
