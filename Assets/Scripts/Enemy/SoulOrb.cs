using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoulOrb : MonoBehaviour
{
    #region Variables
    [Header("Stats")]
    private float _speedMultiplier = 3.5f;
    private float _stopThreshold = 0.01f;

    [Header("Soul Orb")]
    /*[HideInInspector]*/ public bool inUse;

    [Header("Components")]
    private ParticleSystem _ps;
    #endregion

    #region Unity Callbacks
    private void Awake()
    {
        _ps = GetComponent<ParticleSystem>();
    }

    private void OnEnable()
    {
        GameManager.Instance.OnRespawnStart += Reset;
    }

    private void OnDisable()
    {
        GameManager.Instance.OnRespawnStart -= Reset;
    }
    #endregion

    #region Soul Orb Animation
    public void AnimateTowardsDoor(SoulDoor soulDoor, Vector3 enemyDeathPosition)
    {
        inUse = true;
        transform.position = enemyDeathPosition;

        StartCoroutine(EaseOutTowardsDoor(soulDoor));
    }

    IEnumerator EaseOutTowardsDoor(SoulDoor soulDoor)
    {
        _ps.Play();

        if (soulDoor.freeSoulSlot - 1 >= 0)
        {
            Vector3 targetPos = soulDoor.soulSlots[soulDoor.freeSoulSlot - 1].transform.position;
            soulDoor.freeSoulSlot++;

            while (Mathf.Abs(transform.position.x - targetPos.x) > _stopThreshold)
            {
                var distance = (targetPos - transform.position);
                transform.position += new Vector3(distance.x * _speedMultiplier * Time.deltaTime, distance.y * _speedMultiplier * Time.deltaTime, 0);

                yield return null;
            }

            soulDoor.SoulAcquired(_ps, this);
        }
    }
    #endregion

    #region Initialize
    private void Reset()
    {
        transform.localPosition = Vector2.zero;
        _ps.Stop();

        inUse = false;
    }
    #endregion
}
