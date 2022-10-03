using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[Serializable]
public class FloatEvent : UnityEvent<float> { }
[Serializable]
public class BoolEvent : UnityEvent<bool> { }

[Serializable]
public class VectorEvent : UnityEvent<Vector3> { }

public class PlayerMovements : MonoBehaviour
{
    public Action OnTimerEnd = null;
    

    //-------------- ROTATION
    [Header("UI")]
    [NaughtyAttributes.HorizontalLine(1)]
    [SerializeField]
    private TextMeshProUGUI TimerUI = null;
    
    //-------------- PHYSICS
    [Header("Stats")]
    [NaughtyAttributes.HorizontalLine(1)]
    [SerializeField]
    [Min(0f)]
    private float m_minAngularDrag = 0.05f;
    [SerializeField]
    [Range(0f,100f)]
    private float m_maxAngularVelocity = 1f;   
    [SerializeField]
    [Range(0f, 1000f)]
    private float m_maxLinearVelocity = 10f;
    [SerializeField]
    [Tooltip("X is horzontal speed, Y vertical, and Z roll speed")]
    private Vector3 m_rotationSpeed = new Vector3(1f, 1f, 1f);
    [SerializeField]
    [Range(0f,100f)]
    private float forwardImpulse = 10f;

    [SerializeField]
    private float m_pushAwayDistance = 1f;
    [SerializeField]
    private float m_pushAwaySpeed = .5f;

    private Rigidbody rigidbodyRef = null;
    private float m_horizontalInputValue = 0f,
                    m_verticalInputValue = 0f,
                    m_rollInputValue = 0f,
                    chronoMax = 10f,
                    chrono;


    [Header("Special Feature")]
    [NaughtyAttributes.HorizontalLine(1)]
    [SerializeField]
    private float m_warningDisplayTime = 0.5f;

    //-------------- ROTATION
    [Header("Rotation")]
    [NaughtyAttributes.HorizontalLine(1)]
    [SerializeField]
    private float m_rotationAccelTimeReference = 1f;
    [SerializeField]
    private AnimationCurve m_rotationAccelCurve;
    [SerializeField]
    private bool m_useContinuousRotation = false;
    [SerializeField]
    private AnimationCurve m_continuousRotationAccelCurve;
    [SerializeField]
    private float m_stabilizerAccelTimeReference = 1f;
    [SerializeField]
    private AnimationCurve m_stabilizerAccelCurve;

    //-------------- STABILIZER
    [Header("Stabilizer")]
    [NaughtyAttributes.HorizontalLine(1)]
    [SerializeField]
    private bool m_enableManualStabilizer = true;
    [SerializeField]
    private bool m_canStabilizeAndRotateSimultaneously = false;
    [SerializeField]
    [Range(0f, 5f)]
    private float m_stabilizerForce = 1f;
    private float m_stabilizerInputValue = 0f;

    //-------------- STABILIZER
    [Header("Auto Stabilizer")]
    [NaughtyAttributes.HorizontalLine(1)]
    [SerializeField]
    private bool m_AutoStabilizeOnCollision = true;
    [SerializeField]
    private float m_autoStabilizerDuration = 1f;
    [SerializeField]
    private float m_autoStabilizerForce = 3f;
    private bool m_isAutoStabilizerActive = false;


    public FloatEvent OnPitchBoosterChanged;
    public FloatEvent OnYawBoosterChanged;
    public FloatEvent OnRollBoosterChanged;
    public FloatEvent OnStabilizerBoosterChanged;
    public BoolEvent OnSuitFeatureForcedActivation;
    public VectorEvent OnTerrainCollision;

    [SerializeField]
    [NaughtyAttributes.Layer]
    private int m_terrainLayer;

    //-------------- DEBUG
    [Header("Debug")]
    [NaughtyAttributes.HorizontalLine(1)]
    [SerializeField]
    [NaughtyAttributes.ReadOnly]
    [NaughtyAttributes.ProgressBar("Yaw", 1)]
    private float m_horizontalAccelTime = 0f;
    [SerializeField]
    [NaughtyAttributes.ProgressBar("Pitch", 1)]
    [NaughtyAttributes.ReadOnly]
    private float m_verticalAccelTime = 0f;
    [SerializeField]
    [NaughtyAttributes.ProgressBar("Roll", 1)]
    [NaughtyAttributes.ReadOnly]
    private float m_rollAccelTime = 0f;
    [SerializeField]
    [NaughtyAttributes.ProgressBar("Stabilizer", 1)]
    [NaughtyAttributes.ReadOnly]
    private float m_stabilizerAccelTime = 0f;
    [SerializeField]
    bool m_showDebugUI = false;
#if UNITY_EDITOR
    [SerializeField]
    bool m_disableAutoDash = false;
    [SerializeField]
    private KeyCode m_debugDisableAutoDashKey = KeyCode.X;
    [SerializeField]
    private KeyCode m_debugDashKey = KeyCode.Space;
#endif

    private void Awake()
	{
  //      if(TimerUI == null)
		//{
  //          Debug.LogError(this.name + " timer ui variable wasn't set up in editor. Script gonna auto destroy");
  //          Destroy(this);
  //      }
        rigidbodyRef = GetComponent<Rigidbody>();
        if(rigidbodyRef == null)
		{
            Debug.LogError(this.name + " script isn't linked to something with a rigidbody. Script gonna auto destroy");
            Destroy(this);
		}
        
        rigidbodyRef.angularDrag = m_minAngularDrag;
        rigidbodyRef.maxAngularVelocity = m_maxAngularVelocity;
        rigidbodyRef.maxLinearVelocity = m_maxLinearVelocity;

        chrono = chronoMax;
        OnTimerEnd += OnTimerReachZero;

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        InitDebugProxies();
#endif
    }

    void Update()
    {
        CatchInputs();
        CountDown();
    }

    private void FixedUpdate()
    {
        float PitchBoosterValue, YawBoosterValue, RollBoosterValue;
        if (m_useContinuousRotation)
        {
            PitchBoosterValue = m_continuousRotationAccelCurve.Evaluate(m_verticalAccelTime);
            YawBoosterValue = m_continuousRotationAccelCurve.Evaluate(m_horizontalAccelTime);
            RollBoosterValue = m_continuousRotationAccelCurve.Evaluate(m_rollAccelTime);
        }
        else
        {
            PitchBoosterValue = m_rotationAccelCurve.Evaluate(m_verticalAccelTime);
            YawBoosterValue = m_rotationAccelCurve.Evaluate(m_horizontalAccelTime);
            RollBoosterValue = m_rotationAccelCurve.Evaluate(m_rollAccelTime);
        }

        OnPitchBoosterChanged?.Invoke(PitchBoosterValue);
        OnYawBoosterChanged?.Invoke(YawBoosterValue);
        OnRollBoosterChanged?.Invoke(RollBoosterValue);

        Vector3 torqueSpeed = new Vector3(PitchBoosterValue * Mathf.Sign(m_verticalInputValue), 
            YawBoosterValue * Mathf.Sign(m_horizontalInputValue), 
            RollBoosterValue * Mathf.Sign(m_rollInputValue));

        torqueSpeed.Scale(m_rotationSpeed);

        rigidbodyRef.AddRelativeTorque(torqueSpeed, ForceMode.Acceleration);

        if (!m_isAutoStabilizerActive && m_enableManualStabilizer)
        {
            float stabilizerAcc = m_stabilizerAccelCurve.Evaluate(m_stabilizerAccelTime);
            OnStabilizerBoosterChanged?.Invoke(stabilizerAcc);
            rigidbodyRef.angularDrag = Mathf.Max(m_minAngularDrag, stabilizerAcc * m_stabilizerForce);
        }

        PushAwayFromGround(Time.fixedDeltaTime);
    }

   
    // Added this to prevent player to feel like it is just a ball
    private void PushAwayFromGround(float deltaTime)
    {
        Vector3 rayDir = -transform.up;
        SphereCollider m_colliderRef = GetComponent<SphereCollider>();
        
        Vector3 rayOrigin = transform.position + -transform.up * m_colliderRef.radius;
        
        Debug.DrawRay(rayOrigin, rayDir * m_pushAwayDistance, Color.red);
        
        if (Physics.Raycast(rayOrigin, rayDir, out RaycastHit hitinfo, m_pushAwayDistance))
        {
            rigidbodyRef.position += hitinfo.normal * deltaTime * m_pushAwaySpeed;
        }       
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer != m_terrainLayer)
        {
            return;
        }

        if (m_AutoStabilizeOnCollision && !m_isAutoStabilizerActive)
        {
            StartCoroutine(Coroutine_AutoStabilizer());
        }

        if (collision.contacts.Length == 0)
        {
            return;
        }
        
        OnTerrainCollision?.Invoke(collision.contacts[0].point);
    }
    
    private IEnumerator Coroutine_AutoStabilizer()
    {
        m_isAutoStabilizerActive = true;
        rigidbodyRef.angularDrag = m_autoStabilizerForce;
        OnStabilizerBoosterChanged?.Invoke(1);
        yield return new WaitForSecondsRealtime(m_autoStabilizerDuration);
        rigidbodyRef.angularDrag = m_minAngularDrag;
        OnStabilizerBoosterChanged?.Invoke(0);
        m_isAutoStabilizerActive = false;
    }

	private void CatchInputs()
	{
        bool blockRotation = ProcessInputValueToAccelTime("Fire2", ref m_stabilizerInputValue, ref m_stabilizerAccelTime, m_stabilizerAccelTimeReference, false) && !m_canStabilizeAndRotateSimultaneously;

        ProcessInputValueToAccelTime("Horizontal", ref m_horizontalInputValue, ref m_horizontalAccelTime, m_rotationAccelTimeReference, forceReset: blockRotation);
        ProcessInputValueToAccelTime("Vertical", ref m_verticalInputValue, ref m_verticalAccelTime, m_rotationAccelTimeReference, forceReset: blockRotation);
        ProcessInputValueToAccelTime("Roll", ref m_rollInputValue, ref m_rollAccelTime, m_rotationAccelTimeReference, forceReset: blockRotation);

#if UNITY_EDITOR
        if (Input.GetKeyDown(m_debugDashKey))
        {
            OnTimerReachZero();
        }
        if(Input.GetKeyDown(m_debugDisableAutoDashKey))
        {
            m_disableAutoDash = !m_disableAutoDash;
        }
#endif
    }

    // Return true if the axis is pressed.
    private bool ProcessInputValueToAccelTime(string input, ref float inputValue, ref float accelTime, float accelTimeRef, bool instantReset = true, bool forceReset = false)
    {
        float buffer = inputValue;
        inputValue = Input.GetAxisRaw(input);
        if (forceReset
            || Mathf.Approximately(inputValue, 0)
            || buffer == 0 && inputValue != 0
            || Mathf.Sign(buffer) != Mathf.Sign(inputValue))
        {
            if (instantReset)
            {
                accelTime = 0;
            }
            else
            {
                accelTime = Mathf.Max(0, accelTime - Time.fixedDeltaTime / accelTimeRef);
            }
            return false;
        }
        else if (inputValue != 0)
        {
            accelTime = Mathf.Min(1, accelTime + Time.fixedDeltaTime / accelTimeRef);
            return true;
        }

        return false;
    }

    
    private void CountDown()
	{
        if (TimerUI)
        {
            TimerUI.text = MathF.Ceiling(chrono).ToString();
        }

        chrono -= Time.deltaTime;
        if(chrono <= m_warningDisplayTime)
        {
            OnSuitFeatureForcedActivation?.Invoke(true);
        }

        if(chrono <= 0f)
		{
            if (!m_disableAutoDash)
            {
                OnTimerEnd.Invoke();
            }
            OnSuitFeatureForcedActivation?.Invoke(false);
        }
    }


    private void OnTimerReachZero()
	{
        chrono = chronoMax;
        rigidbodyRef.velocity = Vector3.zero;
        rigidbodyRef.AddRelativeForce(0f, 0f, forwardImpulse, ForceMode.Impulse);
	}


#if DEVELOPMENT_BUILD || UNITY_EDITOR
    // ------------------------------------ GUI
    
    float m_referenceWidth = 900;
    Rect m_windowRect = new Rect(0, 0, 400, 200);
    bool m_showSettings = false;
    bool m_showDebug = true;
    
    // DEBUG
    private float m_proxyStabilizerForce = 1;
    private float m_proxyAccelTime = 1;
    private float m_proxyMaxSpeed = 1;
    private Vector2 m_scrollPos;

    private void OnGUI()
    {
        if (!m_showDebugUI)
        {
            return;
        }

        float guiScale = Screen.safeArea.width / m_referenceWidth;
        GUI.matrix = Matrix4x4.Scale(guiScale * Vector3.one);

        // Register the window. Notice the 3rd parameter
        m_windowRect = GUILayout.Window(0, m_windowRect, RenderWindow, "Player Settings");
        m_windowRect.height = Mathf.Min(m_windowRect.height, Screen.safeArea.height * 0.5f);
    }

    // Make the contents of the window
    void RenderWindow(int windowID)
    {
        GUILayout.BeginHorizontal();
        m_showDebug = GUILayout.Toggle(m_showDebug, "Debug", GUI.skin.button);
        m_showSettings = GUILayout.Toggle(m_showSettings, "Settings", GUI.skin.button);
        GUILayout.EndHorizontal();

        m_scrollPos = GUILayout.BeginScrollView(m_scrollPos);
        if (m_showDebug)
        {
            GUILayout.BeginVertical();
            GUI_DrawDebug();
            GUILayout.EndVertical();
        }
        if (m_showSettings)
        {
            GUILayout.BeginVertical();
            GUI_DrawSettings();
            GUILayout.EndVertical();
        }
        GUILayout.EndScrollView();

        GUI.DragWindow(new Rect(0, 0, 10000, 20));
    }

    private void InitDebugProxies()
    {
        m_proxyAccelTime = m_rotationAccelTimeReference;
        m_proxyStabilizerForce = m_stabilizerForce;
        m_proxyMaxSpeed = m_rotationSpeed.x;
    }


    private void GUI_DrawDebug()
    {
        GUI.enabled = false;
        GUILayout.BeginVertical();
        GUILayout.Label("<b>Controls</b>");
        GUILayout.Label("  Horizontal: A - D");
        GUILayout.Label("  Vertical: W - S");
        GUILayout.Label("  Roll: Q - E");
        GUILayout.EndVertical();

        GUILayout.BeginVertical();
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Yaw:");
            GUILayout.HorizontalSlider(m_horizontalAccelTime, 0, 1, GUILayout.Width(200));
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Pitch:");
            GUILayout.HorizontalSlider(m_verticalAccelTime, 0, 1, GUILayout.Width(200));
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Roll:");
            GUILayout.HorizontalSlider(m_rollAccelTime, 0, 1, GUILayout.Width(200));
            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();


        GUI_DrawField("Angular vel", rigidbodyRef.angularVelocity.ToString());
        // GUI_DrawField("Angular vel", rigidbodyRef.angularVelocity.ToString());
        GUI.enabled = true;
    }

    private void GUI_DrawSettings()
    {
        if (GUILayout.Button("Fart!"))
        {
            OnTimerReachZero();
        }

        if (GUI_DebugSliderField("Torque Speed", ref m_proxyMaxSpeed, 0f, 5f, 2.5f))
        {
            m_rotationSpeed.Set(m_proxyMaxSpeed, m_proxyMaxSpeed, m_proxyMaxSpeed);
        }

        GUI_DebugSliderField("Stabilizer Force", ref m_stabilizerForce, 0, 5, m_proxyStabilizerForce);

        GUI_DebugSliderField("Accel Time", ref m_rotationAccelTimeReference, 0, 5, m_proxyAccelTime);        
    }

    private void GUI_DrawField(string name, string value)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label($"{name}:");
        GUILayout.Label($"{value}", GUI.skin.textField);
        GUILayout.EndHorizontal();
    }

    private bool GUI_DebugSliderField(string name, ref float value, float minValue, float maxValue, float defaultValue)
    {
        bool reset = false;
        GUILayout.BeginHorizontal();
        {
            //GUILayout.BeginHorizontal();
            {
                GUILayout.Label($"{name}: <color=orange>{value.ToString("0.00")}</color>", GUILayout.ExpandWidth(true));
                //GUILayout.Label($"", GUI.skin.box);
                
                value = GUILayout.HorizontalSlider(value, minValue, maxValue, GUILayout.Width(200));

                if (GUILayout.Button("Reset", GUILayout.ExpandWidth(false)))
                {
                    reset = true;
                    value = defaultValue;
                }
            }

        }
        GUILayout.EndHorizontal();
        // GUILayout.EndVertical();

        return GUI.changed || reset;
    }
#endif
}

