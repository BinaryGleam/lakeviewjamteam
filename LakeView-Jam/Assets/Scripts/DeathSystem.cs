using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathSystem : MonoBehaviour, IKillable
{
    [SerializeField]
    private MonoBehaviour[] behaviourDeactivatedOnDeath = null;
    [SerializeField]
    private GameObject[] objectsDeactivatedOnDeath = null,
                            activatedOnDeath = null;

    public bool OnDeath()
	{
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
        return true;
	}
}
