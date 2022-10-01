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

    [SerializeField]
    [Range(0.01f,0.15f)]
    private float fireVFXDecayDelay = 0.05f;
    private float chrono = 0f;

    void Awake()
    {
        lineRendererRef = GetComponent<LineRenderer>();
        if (lineRendererRef == null)
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
        if(bShooting)
		{
            chrono = fireVFXDecayDelay;
		}
        if(VFXEnableCondition())
		{
            chrono -= Time.deltaTime;
		}
        lineRendererRef.enabled = VFXEnableCondition();
        gunLight.enabled = VFXEnableCondition();
    }

	private void FixedUpdate()
	{
        lineRendererRef.SetPosition(0, gunLight.transform.position);
        if (bShooting)
		{
            OnShoot();
		}
	}

    private void OnShoot()
	{
        RaycastHit hitInfos;
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hitInfos, Mathf.Infinity))
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hitInfos.distance, Color.yellow);
            lineRendererRef.SetPosition(1, hitInfos.point);
            IShootable shotGameobject = hitInfos.collider.GetComponent<IShootable>();
            if (shotGameobject != null)
            {
                shotGameobject.OnGettingShot();
            }
        }
        else
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 1000, Color.white);
            lineRendererRef.SetPosition(1, transform.TransformDirection(Vector3.forward) * 1000);
        }
    }

    bool VFXEnableCondition() { return chrono > 0f; }
}
