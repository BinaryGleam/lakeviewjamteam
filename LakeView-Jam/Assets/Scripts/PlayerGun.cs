using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGun : MonoBehaviour
{
    private bool bShooting = false;

    private LineRenderer lineRendererRef = null;
    private Rigidbody rigidbodyRef = null;

    [SerializeField]
    private Light gunLight = null;

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
            RaycastHit hitInfos;
            if(Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hitInfos, Mathf.Infinity))
			{
                Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hitInfos.distance, Color.yellow);
                IShootable shotGameobject = hitInfos.collider.GetComponent<IShootable>();
                if (shotGameobject != null)
				{
                    shotGameobject.OnGettingShot();
				}
            }
            else
            {
                Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 1000, Color.white);
            }
            Debug.Log("Shooting");
		}
	}
}
