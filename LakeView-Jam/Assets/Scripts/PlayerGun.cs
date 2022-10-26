using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerGun : MonoBehaviour
{
    private bool bShooting = false;

    [SerializeField]
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

    [SerializeField]
    private ParticleSystem m_laserImpactFX;

    [SerializeField]
    private bool m_canShoot = true;
    
    private bool m_lastDeviceUsedIsGamepad = false;

    public UnityEvent OnShot;

    private PlayerInput m_playerInput;

    private InputAction m_fireAction;

    void Awake()
    {
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

    private void OnEnable()
    {
        if (!m_playerInput)
        {
            m_playerInput = GetComponent<PlayerInput>();
            m_fireAction = m_playerInput.actions["Fire"];
        }

        m_playerInput.onControlsChanged += OnControlsChanged;
        m_fireAction.started += Shoot;
        m_fireAction.canceled += Shoot;
    }

    private void OnDisable()
    {
        if (m_playerInput != null)
        {
            m_fireAction.started -= Shoot;
            m_fireAction.canceled -= Shoot;
            m_playerInput.onControlsChanged -= OnControlsChanged;
        }
    }

    public void Shoot(InputAction.CallbackContext context)
    {
        if (!m_canShoot)
        {
            return;
        }

        if (context.started && !bShooting)
        {
            chrono = fireVFXDecayDelay;
            OnShoot();
            bShooting = true;
        }
        if (context.canceled)
        {
            bShooting = false;
        }
    }

    public void OnControlsChanged(PlayerInput pInput)
    {
        if (pInput.user.controlScheme != null)
        {
            m_lastDeviceUsedIsGamepad = pInput.user.controlScheme.Value.name == "Gamepad";
        }
    }

    void Update()
    {

        RenderCursorAim();
        UpdateGunAndArmTransform(m_mouseRayWorld.direction);

        if (!m_canShoot)
        {
            return;
        }

#if !ENABLE_INPUT_SYSTEM
        bShooting = Input.GetButtonDown("Fire1") || Input.GetAxis("Fire1") != 0;
        if(bShooting)
		{
            chrono = fireVFXDecayDelay;
		}
#endif
        if(VFXEnableCondition())
		{
            chrono -= Time.deltaTime;
		}
        lineRendererRef.enabled = VFXEnableCondition();
        gunLight.enabled = VFXEnableCondition();

        lineRendererRef.SetPosition(0, GunMuzzle.position);
    }

    [SerializeField]
    private float m_mouseAimSpeed = 5f;
    [SerializeField]
    private float m_gamepadAimSpeed = 3f;
    private Ray m_mouseRayWorld;

    public void EnableGun(bool value)
    {
        m_canShoot = value;
    }

    private void RenderCursorAim()
    {
        if (!m_lastDeviceUsedIsGamepad)
        {
            m_mouseRayWorld = Camera.main.ScreenPointToRay(Input.mousePosition);
        }
        else
        {
            Vector2 centerOfScreen = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            m_mouseRayWorld = new Ray(centerOfScreen, Vector3.Slerp(m_mouseRayWorld.direction, transform.forward, Time.deltaTime * m_gamepadAimSpeed));
        }

        if (gunCursor)
        {
            gunCursor.position = m_lastDeviceUsedIsGamepad ? m_mouseRayWorld.origin : Input.mousePosition;
        }
    }

    private void UpdateGunAndArmTransform(Vector3 Dir, bool forceDestination = false)
    {
        if (m_lastDeviceUsedIsGamepad)
        {
            m_playerArm.forward = Vector3.MoveTowards(m_playerArm.forward, Dir, forceDestination ? 10 : Time.deltaTime * m_gamepadAimSpeed);
        }
        else
        {
            m_playerArm.forward = Vector3.Lerp(m_playerArm.forward, Vector3.MoveTowards(m_playerArm.forward, Dir, forceDestination ? 10 : 1), forceDestination ? 1 : Time.deltaTime * m_mouseAimSpeed);
        }
        
        var beef = m_playerArm.localRotation.eulerAngles;
        beef.z = 0;
        m_playerArm.localRotation = Quaternion.Euler(beef);
    }

    [SerializeField]
    private float m_sphereCastRadius = 1;
    [SerializeField]
    private float m_shootMaxDistance = 100;
    
    [SerializeField, NaughtyAttributes.Layer]
    private int m_sphereCastLayer;

    private void OnShoot()
	{
        OnShot?.Invoke();
        m_animator.SetTrigger(m_animatorParam);
        rigidbodyRef.AddRelativeForce(shootLinearCounterForce, ForceMode.Impulse);
        rigidbodyRef.AddRelativeTorque(shootAngularCounterForce, ForceMode.Impulse);
        RaycastHit hitInfos;

        if (m_lastDeviceUsedIsGamepad)
        {
            Vector2 centerOfScreen = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            m_mouseRayWorld = Camera.main.ScreenPointToRay(centerOfScreen);
        }
        else
        {
            m_mouseRayWorld = Camera.main.ScreenPointToRay(Input.mousePosition);
        }

        UpdateGunAndArmTransform(m_mouseRayWorld.direction, true);

        // if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hitInfos, Mathf.Infinity))
        if (Physics.SphereCast(m_mouseRayWorld, m_sphereCastRadius, out hitInfos, m_shootMaxDistance, 1 << m_sphereCastLayer))
        {
#if UNITY_EDITOR
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hitInfos.distance, Color.yellow);
#endif
            m_lastHit = hitInfos.point;
            lineRendererRef.SetPosition(1, hitInfos.point);

            if (m_laserImpactFX)
            {
                Instantiate(m_laserImpactFX, hitInfos.point + hitInfos.normal, Quaternion.identity);
            }

            IShootable shotGameobject = hitInfos.collider.GetComponent<IShootable>();
            if (shotGameobject != null)
            {
                shotGameobject.OnGettingShot(hitInfos);
            }
        }
        else
        {
#if UNITY_EDITOR
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 1000, Color.white);
#endif

            if (Physics.Raycast(m_mouseRayWorld, out hitInfos, m_shootMaxDistance))
            {
                if (m_laserImpactFX)
                {
                    Instantiate(m_laserImpactFX, hitInfos.point + hitInfos.normal, Quaternion.identity);
                }

                m_lastHit = hitInfos.point;
                lineRendererRef.SetPosition(1, hitInfos.point);
            }
            else
            {
                lineRendererRef.SetPosition(1, transform.TransformDirection(Vector3.forward) * 1000);
            }
        }
    }


    bool VFXEnableCondition() { return chrono > 0f; }


#if UNITY_EDITOR
    [SerializeField, Min(1)]
    private float ProjectionDistance = 1;
    Vector3 m_lastHit;
    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(m_mouseRayWorld.origin + m_mouseRayWorld.direction * ProjectionDistance, .25f);

        if (m_lastHit != Vector3.zero)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(m_lastHit, m_sphereCastRadius);
            Gizmos.color = Color.red;
            Gizmos.DrawCube(m_lastHit, Vector3.one * m_sphereCastRadius * 0.33f);
        }
    }
#endif
}
