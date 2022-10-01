using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGun : MonoBehaviour
{
    private LineRenderer lineRendererRef = null;
    private Rigidbody rigidbodyRef = null;

    [SerializeField]
    private Light gunLight = null;

    private bool bShooting = false;

    void Awake()
    {
        lineRendererRef = GetComponent<LineRenderer>();
        if (rigidbodyRef == null)
        {
            Debug.LogError(this.name + " script isn't linked to something with a lineRenderer. Script gonna auto destroy");
            Destroy(this);
        }
        rigidbodyRef = GetComponent<Rigidbody>();
        if (rigidbodyRef == null)
        {
            Debug.LogError(this.name + " script isn't linked to something with a rigidbody. Script gonna auto destroy");
            Destroy(this);
        }
    }

    void Update()
    {
        bShooting = Input.GetButtonDown("Fire1");
    }

	private void FixedUpdate()
	{
		if(bShooting)
		{
            Debug.Log("Shooting");
		}
	}
}
