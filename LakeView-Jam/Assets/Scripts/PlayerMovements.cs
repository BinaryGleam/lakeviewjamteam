using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovements : MonoBehaviour
{
    [SerializeField]
    [Range(0f,100f)]
    private float maxAngularVelocity = 1f;

    [SerializeField]
    [Range(0f, 1000f)]
    private float maxLinearVelocity = 10f;

    [SerializeField]
    [Tooltip("X is horzontal speed, Y vertical, and Z roll speed")]
    private Vector3 moveSpeed = new Vector3(1f, 1f, 1f);

    private Rigidbody rigidbodyRef = null;

    private float horizontal = 0f, 
                    vertical = 0f, 
                    roll = 0f;

	private void Awake()
	{
        rigidbodyRef = GetComponent<Rigidbody>();
        if(rigidbodyRef == null)
		{
            Debug.LogError(this.name + " script isn't linked to something with a rigidbody. Script gonna auto destroy");
            Destroy(this);
		}
	}

	void Start()
    {
        rigidbodyRef.maxAngularVelocity = maxAngularVelocity;
        rigidbodyRef.maxLinearVelocity = maxLinearVelocity;
    }

    void Update()
    {
        CatchInputs();
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
}
