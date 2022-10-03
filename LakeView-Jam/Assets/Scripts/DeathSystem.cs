using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DeathSystem : MonoBehaviour, IKillable
{
    [SerializeField]
    private MonoBehaviour[] behaviourDeactivatedOnDeath = null;
    [SerializeField]
    private GameObject[] objectsDeactivatedOnDeath = null,
                            activatedOnDeath = null;

    public UnityEvent OnDeathEvent;
    private bool bDead = false;

    public bool OnDeath()
	{
        if (bDead)
            return false;
		foreach (GameObject toActivate in activatedOnDeath)
		{
            toActivate.SetActive(true);
		}
        foreach (GameObject toDeactivate in objectsDeactivatedOnDeath)
		{
            toDeactivate.SetActive(false);
		}
        foreach (MonoBehaviour behaviour in behaviourDeactivatedOnDeath)
		{
            behaviour.enabled = false;
		}

        OnDeathEvent?.Invoke();
        bDead = true;
        return true;
	}
}
