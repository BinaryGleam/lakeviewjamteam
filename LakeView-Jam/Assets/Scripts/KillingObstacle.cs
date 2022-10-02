using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillingObstacle : MonoBehaviour
{
	private void OnCollisionEnter(Collision collision)
	{
		if (collision.collider.tag == "Player")
		{
			collision.collider.GetComponent<IKillable>()?.OnDeath();
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.tag == "Player")
		{
			other.GetComponent<IKillable>()?.OnDeath();
		}
	}
}
