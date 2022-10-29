using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.InputSystem;

[Serializable]
public class FloatEvent : UnityEvent<float> { }
[Serializable]
public class BoolEvent : UnityEvent<bool> { }

[Serializable]
public class VectorEvent : UnityEvent<Vector3> { }

public class PlayerMovements : MonoBehaviour
{
    private static readonly string ActionMapUIName = "UI";
    private static readonly string ActionMapGameplayName = "Gameplay";

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
    [Range(0f,200f)]
    private float forwardImpulse = 10f;

    [SerializeField]
    private float m_pushAwayDistance = 1f;
    [SerializeField]
    private float m_pushAwaySpeed = .5f;

    private Rigidbody rigidbodyRef = null;
    private float chronoMax = 10f;
    

   


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
    private AnimationCurve m_stabilizerAccelCurve;

    //-------------- STABILIZER
    [Header("Stabilizer")]
    [NaughtyAttributes.HorizontalLine(1)]
    [SerializeField]
    private bool m_enableManualStabilizer = true;
    [SerializeField]
    [Range(0f, 5f)]
    private float m_stabilizerForce = 1f;
    

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


    [NaughtyAttributes.Foldout("Quaternion Booster")]
    public FloatEvent OnPitchBoosterChanged;
    [NaughtyAttributes.Foldout("Quaternion Booster")]
    public FloatEvent OnYawBoosterChanged;
    [NaughtyAttributes.Foldout("Quaternion Booster")]
    public FloatEvent OnRollBoosterChanged;
    [NaughtyAttributes.Foldout("Quaternion Booster")]
    public FloatEvent OnStabilizerBoosterChanged;
    [NaughtyAttributes.Foldout("Quaternion Booster"), UnityEngine.Serialization.FormerlySerializedAs("OnSuitFeatureForcedActivation")]
    public BoolEvent OnSuitWarningDisplay;
    [NaughtyAttributes.Foldout("Quaternion Booster")]
    public UnityEvent OnSuitBoostActivation;

    [NaughtyAttributes.Foldout("Booster Sound ")]
    public UnityEvent OnUpBoosterTrigger;
    [NaughtyAttributes.Foldout("Booster Sound ")]
    public UnityEvent OnDownBoosterTrigger;
    [NaughtyAttributes.Foldout("Booster Sound ")]
    public UnityEvent OnRightBoosterTrigger;
    [NaughtyAttributes.Foldout("Booster Sound ")]
    public UnityEvent OnLeftBoosterTrigger;
    [NaughtyAttributes.Foldout("Booster Sound ")]
    public UnityEvent OnRollDownLeftBoosterTrigger;
    [NaughtyAttributes.Foldout("Booster Sound ")]
    public UnityEvent OnRollUpRightBoosterTrigger;

    public VectorEvent OnTerrainCollision;

    [SerializeField]
    [NaughtyAttributes.Layer]
    private int m_terrainLayer;

    private Vector3 m_invertMovementVector = Vector3.one;

    //-------------- DEBUG
    [Header("Debug")]
    [NaughtyAttributes.HorizontalLine(1)]
    [NaughtyAttributes.ReadOnly]
    [SerializeField]
    private float chrono;
    // [SerializeField]
    // [NaughtyAttributes.ReadOnly]
    // [NaughtyAttributes.ProgressBar("Yaw", 1)]
    // private float m_horizontalAccelTime = 0f;
    // [SerializeField]
    // [NaughtyAttributes.ProgressBar("Pitch", 1)]
    // [NaughtyAttributes.ReadOnly]
    // private float m_verticalAccelTime = 0f;
    [SerializeField]
    //[NaughtyAttributes.ProgressBar("Roll", 1)]
    [NaughtyAttributes.ReadOnly]
    private float m_rollAccelTime = 0f;
    [SerializeField]
    //[NaughtyAttributes.ProgressBar("Stabilizer", 1)]
    [NaughtyAttributes.ReadOnly]
    private float m_stabilizerAccelTime = 0f;
    [SerializeField]
    bool m_showDebugUI = false;
    [SerializeField]
    bool m_disableAutoDash = false;
    bool m_onLogReading = false;
#if UNITY_EDITOR
    [SerializeField]
    private KeyCode m_debugDisableAutoDashKey = KeyCode.X;
    [SerializeField]
    private KeyCode m_debugDashKey = KeyCode.Space;
#endif


    private bool m_isGamePause;
    public UnityEvent OnLogExit;


    [SerializeField, NaughtyAttributes.ReadOnly]
    private Vector2 m_rotationInputValue;
    private Vector2 m_rotationAccelTime;
    [NaughtyAttributes.ReadOnly, SerializeField]
    private float PitchBoosterValue;
    [NaughtyAttributes.ReadOnly, SerializeField]
    private float YawBoosterValue;
    [NaughtyAttributes.ReadOnly, SerializeField]
    private float m_rollInputValue = 0f;
    [NaughtyAttributes.ReadOnly, SerializeField]
    private float RollBoosterValue;
    
    // Input
    private PlayerInput m_playerInput;
    private InputAction m_pauseAction;
    private InputAction m_uiConfirmAction;

    public void InvertHorizontalRotation(bool invert)
    {
        m_invertMovementVector.y = invert ? -1 : 1;
    }

    public void InvertVerticalRotation(bool invert)
    {
        m_invertMovementVector.x = invert ? -1 : 1;
    }

    public void InvertRoll(bool invert)
    {
        m_invertMovementVector.z = invert ? -1 : 1;
    }

    private void Awake()
	{
        //      if(TimerUI == null)
        //{
        //          Debug.LogError(this.name + " timer ui variable wasn't set up in editor. Script gonna auto destroy");
        //          Destroy(this);
        //      }
        m_playerInput = GetComponent<PlayerInput>();

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

    private void OnPause(bool pause)
    {
        if (pause)
        {
            m_playerInput.SwitchCurrentActionMap(ActionMapUIName);
            m_isGamePause = true;
            m_disableAutoDash = true;
        }
        else
        {
            m_playerInput.SwitchCurrentActionMap(ActionMapGameplayName);
            m_isGamePause = false;
            m_disableAutoDash = false;
        }
    }

    private void OnEnable()
    {
        m_playerInput.SwitchCurrentActionMap(ActionMapUIName);
        m_uiConfirmAction = m_playerInput.actions["Confirm"];
        m_uiConfirmAction.started += LogConfirmButton;

        if (GameManager.Instance)
        {
            GameManager.Instance.OnGamePause.AddListener(OnPause);
        }

        m_pauseAction = m_playerInput.actions["Pause"];
        m_pauseAction.started += PauseAction_started;

        if (PlayerSetting.Instance)
        {
            PlayerSetting.Instance.OnInvertRotationXChanged.AddListener(InvertHorizontalRotation);
            PlayerSetting.Instance.OnInvertRotationYChanged.AddListener(InvertVerticalRotation);
            PlayerSetting.Instance.OnInvertRollChanged.AddListener(InvertRoll);
        }
    }

    private void OnDisable()
    {
        if (m_playerInput != null)
        {
            m_uiConfirmAction.canceled -= LogConfirmButton;
            m_pauseAction.started -= PauseAction_started;
        }

        if (GameManager.Instance)
        {
            GameManager.Instance.OnGamePause.RemoveListener(OnPause);
        }

        if (PlayerSetting.Instance)
        {
            PlayerSetting.Instance.OnInvertRotationXChanged.RemoveListener(InvertHorizontalRotation);
            PlayerSetting.Instance.OnInvertRotationYChanged.RemoveListener(InvertVerticalRotation);
            PlayerSetting.Instance.OnInvertRollChanged.RemoveListener(InvertRoll);
        }
    }

    void Update()
    {
        CatchInputs();
        CountDown();
    }

    private void FixedUpdate()
    {
        if (m_onLogReading || m_isGamePause)
        {
            return;
        }

        if (m_useContinuousRotation)
        {
            PitchBoosterValue = m_rotationAccelTime.y == 0 ? 0 : m_continuousRotationAccelCurve.Evaluate(m_rotationAccelTime.y);
            YawBoosterValue = m_rotationAccelTime.x == 0 ? 0 : m_continuousRotationAccelCurve.Evaluate(m_rotationAccelTime.x);
            RollBoosterValue = m_rollAccelTime == 0 ? 0 : m_continuousRotationAccelCurve.Evaluate(m_rollAccelTime);
        }
        else
        {
            PitchBoosterValue = m_rotationAccelCurve.Evaluate(m_rotationAccelTime.y);
            YawBoosterValue = m_rotationAccelCurve.Evaluate(m_rotationAccelTime.x);
            RollBoosterValue = m_rotationAccelCurve.Evaluate(m_rollAccelTime);
        }
        
        OnPitchBoosterChanged?.Invoke(PitchBoosterValue);
        OnYawBoosterChanged?.Invoke(YawBoosterValue);
        OnRollBoosterChanged?.Invoke(RollBoosterValue);

        Vector3 torqueSpeed = new Vector3(PitchBoosterValue * Mathf.Sign(m_rotationInputValue.y) * m_invertMovementVector.x, 
            YawBoosterValue * Mathf.Sign(m_rotationInputValue.x) * m_invertMovementVector.y, 
            RollBoosterValue * Mathf.Sign(m_rollInputValue) * m_invertMovementVector.z);

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

    private void PauseAction_started(InputAction.CallbackContext obj)
    {
        if (GetComponent<DeathSystem>().IsDead)
        {
            return;
        }

        GameManager.Instance.Pause(true);
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
    
    public void OnLogBegin()
    {
        m_disableAutoDash = true;
        m_onLogReading = true;
    }

    private void LogConfirmButton(InputAction.CallbackContext context)
    {
        if (m_onLogReading)
        {
            OnLogExit?.Invoke();
            m_disableAutoDash = false;
            m_onLogReading = false;
            m_playerInput.SwitchCurrentActionMap(ActionMapGameplayName);
        }
        else if (m_isGamePause)
        {
            GameManager.Instance.Pause(false);
        }
    }
    
    private void CatchInputs()
	{
        if (!m_onLogReading)
        {      
            ProcessInputValueToAccelTime("Rotate", ref m_rotationInputValue, ref m_rotationAccelTime, m_rotationAccelTimeReference,
                OnDownBoosterTrigger, OnUpBoosterTrigger, OnLeftBoosterTrigger, OnRightBoosterTrigger);

            ProcessInputValueToAccelTime("Roll", ref m_rollInputValue, ref m_rollAccelTime, m_rotationAccelTimeReference,
                boosterPositiveEvent: OnRollDownLeftBoosterTrigger, boosterNegativeEvent: OnRollUpRightBoosterTrigger);
        }
        

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

    // No template specialization in C# (because no compilation time :x) 
    private bool ProcessInputValueToAccelTime(string actionName, ref float inputValue, ref float accelTime, float accelTimeRef, UnityEvent boosterPositiveEvent = null, UnityEvent boosterNegativeEvent = null)        
    {
# if ENABLE_INPUT_SYSTEM
        float val = m_playerInput.actions[actionName].ReadValue<float>();
#else
        var val = Input.GetAxisRaw(actionName);
#endif
        return ProcessInputValueToAccelTimeGeneric(val, ref inputValue, ref accelTime, accelTimeRef, true, false, boosterPositiveEvent, boosterNegativeEvent);
    }

    private bool ProcessInputValueToAccelTime(string actionName, ref Vector2 inputValue, ref Vector2 accelTime, float accelTimeRef,
        UnityEvent boosterVPositiveEvent, UnityEvent boosterVNegativeEvent, UnityEvent boosterHPositiveEvent, UnityEvent boosterHNegativeEvent)
    {
# if ENABLE_INPUT_SYSTEM
        Vector2 val = m_playerInput.actions[actionName].ReadValue<Vector2>();
#else
        var val = Input.GetAxisRaw(actionName);
#endif
        return ProcessInputValueToAccelTimeGeneric(val, ref inputValue, ref accelTime, accelTimeRef, true,
            boosterVPositiveEvent, boosterVNegativeEvent, boosterHPositiveEvent, boosterHNegativeEvent);
    }

    [SerializeField]
    private float joystickDeadZone = 0.4f;
    private bool ProcessInputValueToAccelTimeGeneric(Vector2 newInputValue, ref Vector2 inputValue, ref Vector2 accelTime, float accelTimeRef, bool instantReset,
        UnityEvent boosterVPositiveEvent, UnityEvent boosterVNegativeEvent, UnityEvent boosterHPositiveEvent, UnityEvent boosterHNegativeEvent)
    {
        float xSign = Mathf.Sign(newInputValue.x);
        float ySign = Mathf.Sign(newInputValue.y);
        float xAbs = Mathf.Abs(newInputValue.x);
        float yAbs = Mathf.Abs(newInputValue.y);

        newInputValue.x = xAbs > joystickDeadZone /*&& xAbs > yAbs*/? xSign : 0f;
        newInputValue.y = yAbs > joystickDeadZone /*&& yAbs > xAbs */? ySign : 0f;

        bool xResult = ProcessInputValueToAccelTimeGeneric(newInputValue.x, ref inputValue.x, ref accelTime.x, accelTimeRef, instantReset, false, boosterHPositiveEvent, boosterHNegativeEvent);
        bool yResult = ProcessInputValueToAccelTimeGeneric(newInputValue.y, ref inputValue.y, ref accelTime.y, accelTimeRef, instantReset, false, boosterVPositiveEvent, boosterVNegativeEvent);

        return xResult || yResult;
    }

    // Return true if the axis is pressed.
    private bool ProcessInputValueToAccelTimeGeneric(float newInputValue, ref float previousFrameInputValue, ref float accelTime, float accelTimeRef, bool instantReset = true, bool forceReset = false, UnityEvent boosterPositiveEvent = null, UnityEvent boosterNegativeEvent = null)
    {
        float buffer = previousFrameInputValue;
        previousFrameInputValue = newInputValue;

        if (buffer == 0 && previousFrameInputValue != 0)
        {
            if (Mathf.Sign(previousFrameInputValue) > 0)
            {
                boosterPositiveEvent?.Invoke();
            }
            else
            {
                boosterNegativeEvent?.Invoke();
            }
        }

        if (forceReset
            || Mathf.Approximately(previousFrameInputValue, 0)
            || buffer == 0 && previousFrameInputValue != 0
            || Mathf.Sign(buffer) != Mathf.Sign(previousFrameInputValue))
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
        else if (previousFrameInputValue != 0)
        {
            accelTime = Mathf.Min(1, accelTime + Time.fixedDeltaTime / accelTimeRef);
            return true;
        }

        return false;
    }

    private bool m_suitFeatureActivationFlip = false;
    private void CountDown()
	{
        if (m_onLogReading || m_disableAutoDash)
        {
            return;
        }

        if (TimerUI)
        {
            TimerUI.text = MathF.Ceiling(chrono).ToString();
        }

        chrono -= Time.deltaTime;
        if(chrono <= m_warningDisplayTime && !m_suitFeatureActivationFlip)
        {
            m_suitFeatureActivationFlip = true;
            OnSuitWarningDisplay?.Invoke(true);
        }

        if(chrono <= 0f)
		{
            OnTimerEnd.Invoke();
            
            m_suitFeatureActivationFlip = false;
            OnSuitWarningDisplay?.Invoke(false);
            OnSuitBoostActivation?.Invoke();
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
        // m_windowRect = GUILayout.Window(0, m_windowRect, RenderWindow, "Player Settings");
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
            // GUI_DrawDebug();
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


    //private void GUI_DrawDebug()
    //{
    //    GUI.enabled = false;
    //    GUILayout.BeginVertical();
    //    GUILayout.Label("<b>Controls</b>");
    //    GUILayout.Label("  Horizontal: A - D");
    //    GUILayout.Label("  Vertical: W - S");
    //    GUILayout.Label("  Roll: Q - E");
    //    GUILayout.EndVertical();

    //    GUILayout.BeginVertical();
    //    {
    //        GUILayout.BeginHorizontal();
    //        GUILayout.Label("Yaw:");
    //        GUILayout.HorizontalSlider(m_horizontalAccelTime, 0, 1, GUILayout.Width(200));
    //        GUILayout.EndHorizontal();
    //        GUILayout.BeginHorizontal();
    //        GUILayout.Label("Pitch:");
    //        GUILayout.HorizontalSlider(m_verticalAccelTime, 0, 1, GUILayout.Width(200));
    //        GUILayout.EndHorizontal();
    //        GUILayout.BeginHorizontal();
    //        GUILayout.Label("Roll:");
    //        GUILayout.HorizontalSlider(m_rollAccelTime, 0, 1, GUILayout.Width(200));
    //        GUILayout.EndHorizontal();
    //    }
    //    GUILayout.EndVertical();


    //    GUI_DrawField("Angular vel", rigidbodyRef.angularVelocity.ToString());
    //    // GUI_DrawField("Angular vel", rigidbodyRef.angularVelocity.ToString());
    //    GUI.enabled = true;
    //}

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

