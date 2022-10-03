using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicEnemy : MonoBehaviour, IShootable, IKillable
{
    [SerializeField]
    private float lerpSpeed = 1f,
                    moveSpeed = 10f,
                    bulletForceResponse = 1f;

    private Rigidbody rigidbodyRef = null;
    private Animator animatorRef = null;
    private Transform target = null;
    [SerializeField]
    private GameObject Gore = null;

    public bool OnGettingShot(RaycastHit hit)
	{
        rigidbodyRef.AddForceAtPosition(-hit.normal * bulletForceResponse, hit.point, ForceMode.Impulse);
        OnDeath();
        return true;
	}

    public bool OnDeath()
	{
        animatorRef.SetBool("bDead", true);
        Gore?.SetActive(true);
        gameObject.AddComponent(System.Type.GetType("FloatingProp"));
        GetComponent<FloatingProp>().bulletForceResponse = bulletForceResponse;
        Destroy(this);
        return true;
	}

	private void Awake()
	{
        rigidbodyRef = GetComponent<Rigidbody>();
        if (rigidbodyRef == null)
        {
            Debug.LogError(this.name + " script isn't linked to something with a rigidbody. Script gonna auto destroy");
            Destroy(this);
        }
        animatorRef = GetComponent<Animator>();
        if (animatorRef == null)
        {
            Debug.LogError(this.name + " script isn't linked to something with a animator. Script gonna auto destroy");
            Destroy(this);
        }
    }

	private void FixedUpdate()
	{
        if (target != null)
        {
            Vector3 targetDirection = (target.position - transform.position).normalized;
            transform.up = Vector3.MoveTowards(transform.up, targetDirection, Time.deltaTime * lerpSpeed);
            rigidbodyRef.velocity = targetDirection * moveSpeed;
        }
    }

	private void OnCollisionEnter(Collision collision)
	{
		if(collision.collider.tag == "Player")
		{
            collision.collider.GetComponent<IKillable>()?.OnDeath();
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if(other.tag == "Player")
		{
            target = other.transform;
            animatorRef.SetBool("bChasing", true);
		}
	}

	private void OnTriggerExit(Collider other)
	{
        if (other.tag == "Player")
        {
            target = null;
            animatorRef.SetBool("bChasing", false);
        }
    }
}
