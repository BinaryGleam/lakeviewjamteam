using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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

    [SerializeField]
    private Transform m_playerArm;

    [SerializeField]
    [NaughtyAttributes.Required]
    private Transform GunMuzzle;

    [SerializeField]
    private Transform gunCursor = null;

    [SerializeField, NaughtyAttributes.Required]
    private Animator m_animator;
    [SerializeField, NaughtyAttributes.AnimatorParam("m_animator")]
    private string m_animatorParam;

    [SerializeField]
    private Vector3 shootLinearCounterForce = Vector3.zero,
                    shootAngularCounterForce = Vector3.zero;

    public UnityEvent OnShot;

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

        //Cursor.visible = false;
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

        lineRendererRef.SetPosition(0, GunMuzzle.position);
        if (bShooting)
        {
            OnShoot();
        }

        mouseRayWorld = Camera.main.ScreenPointToRay(Input.mousePosition);
        if(gunCursor)
		{
            gunCursor.position = Input.mousePosition;
		}

        m_playerArm.forward = Vector3.MoveTowards(m_playerArm.forward, mouseRayWorld.direction, Time.deltaTime * m_aimSpeed);
            // mouseRayWorld.direction;
        // m_playerArm.LookAt();

        var beef = m_playerArm.localRotation.eulerAngles;
        beef.z = 0;
        m_playerArm.localRotation = Quaternion.Euler(beef);
    }
    [SerializeField]
    float m_aimSpeed = 10f;
    Ray mouseRayWorld;
    [SerializeField, Min(1)]
    private float ProjectionDistance = 1;

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(mouseRayWorld.origin + mouseRayWorld.direction * ProjectionDistance, .25f);
    }
#endif

    private void OnShoot()
	{
        OnShot?.Invoke();
        m_animator.SetTrigger(m_animatorParam);
        rigidbodyRef.AddRelativeForce(shootLinearCounterForce, ForceMode.Impulse);
        rigidbodyRef.AddRelativeTorque(shootAngularCounterForce, ForceMode.Impulse);
        RaycastHit hitInfos;

        // if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hitInfos, Mathf.Infinity))
        if (Physics.Raycast(mouseRayWorld, out hitInfos, Mathf.Infinity))
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hitInfos.distance, Color.yellow);
            lineRendererRef.SetPosition(1, hitInfos.point);
            IShootable shotGameobject = hitInfos.collider.GetComponent<IShootable>();
            if (shotGameobject != null)
            {
                shotGameobject.OnGettingShot(hitInfos);
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
