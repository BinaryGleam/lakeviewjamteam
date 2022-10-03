using System;
using System.Collections.Generic;
using UnityEngine;

public class DoorButton : MonoBehaviour, IShootable
{
    public Action onBreakingEvent = null;
    private bool bBroke = false;
    private Light statusLight = null;

    public bool OnGettingShot(RaycastHit hit)
	{
        if(!bBroke)
		{
            onBreakingEvent.Invoke();
            return true;
		}
        return false;
	}

    void Start()
    {
        statusLight = GetComponentInChildren<Light>();
        if (statusLight == null)
        {
            Debug.LogError(this.name + " script isn't linked to something with a Light. Script gonna auto destroy");
            Destroy(this);
        }
        onBreakingEvent += OnBreak;
    }

    private void OnBreak()
	{
        bBroke = true;
        statusLight.enabled = false;
        transform.parent = null;
        GetComponent<IKillable>()?.OnDeath();
    }
}
