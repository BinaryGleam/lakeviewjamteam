using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerMovements : MonoBehaviour
{
    public Action OnTimerEnd = null;

    [SerializeField]
    private TextMeshProUGUI TimerUI = null;

    [SerializeField]
    [Range(0f,100f)]
    private float maxAngularVelocity = 1f;

    [SerializeField]
    [Range(0f, 1000f)]
    private float maxLinearVelocity = 10f;

    [SerializeField]
    [Tooltip("X is horzontal speed, Y vertical, and Z roll speed")]
    private Vector3 moveSpeed = new Vector3(1f, 1f, 1f);

    [SerializeField]
    [Range(0f,100f)]
    private float forwardImpulse = 5f;

    private Rigidbody rigidbodyRef = null;

    private float horizontal = 0f, 
                    vertical = 0f, 
                    roll = 0f,
                    chronoMax = 10f,
                    chrono;

	private void Awake()
	{
        if(TimerUI == null)
		{
            Debug.LogError(this.name + " timer ui variable wasn't set up in editor. Script gonna auto destroy");
            Destroy(this);
        }
        rigidbodyRef = GetComponent<Rigidbody>();
        if(rigidbodyRef == null)
		{
            Debug.LogError(this.name + " script isn't linked to something with a rigidbody. Script gonna auto destroy");
            Destroy(this);
		}
        rigidbodyRef.maxAngularVelocity = maxAngularVelocity;
        rigidbodyRef.maxLinearVelocity = maxLinearVelocity;

        chrono = chronoMax;
        OnTimerEnd += OnTimerReachZero;
	}
    void Update()
    {
        CatchInputs();
        CountDown();
    }

	private void FixedUpdate()
	{
        Vector3 torqueSpeed = new Vector3(vertical, horizontal, roll);
        torqueSpeed.Scale(moveSpeed);
        rigidbodyRef.AddRelativeTorque(torqueSpeed, ForceMode.Acceleration);
	}

	private void CatchInputs()
	{
        horizontal  = Input.GetAxis("Horizontal");
        vertical    = Input.GetAxis("Vertical");
        roll        = Input.GetAxis("Roll");
    }

    private void CountDown()
	{
        TimerUI.text = MathF.Ceiling(chrono).ToString();
        chrono -= Time.deltaTime;
        if(chrono <= 0f)
		{
            OnTimerEnd.Invoke();
		}
	}

    private void OnTimerReachZero()
	{
        chrono = chronoMax;
        rigidbodyRef.velocity = Vector3.zero;
        rigidbodyRef.AddRelativeForce(0f, 0f, forwardImpulse, ForceMode.Impulse);
	}
}
