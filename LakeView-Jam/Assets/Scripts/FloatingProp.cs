using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingProp : MonoBehaviour, IShootable
{
    private Rigidbody rigidbodyRef = null;

    [SerializeField]
    private Vector3 linearInitForceMin = Vector3.zero,
                    linearInitForceMax = Vector3.zero,
                    angularInitForceMin = Vector3.zero,
                    angularInitForceMax = Vector3.zero;

    [SerializeField]
    private float bulletForceResponse = 1f;

    void Awake()
    {
        rigidbodyRef = GetComponent<Rigidbody>();
        if (rigidbodyRef == null)
        {
            Debug.LogError(this.name + " script isn't linked to something with a rigidbody. Script gonna auto destroy");
            Destroy(this);
        }
        rigidbodyRef.velocity = new Vector3(Random.Range(linearInitForceMin.x,linearInitForceMax.x),
                                            Random.Range(linearInitForceMin.y, linearInitForceMax.y),
                                            Random.Range(linearInitForceMin.z, linearInitForceMax.z));
        
        rigidbodyRef.angularVelocity = new Vector3(Random.Range(angularInitForceMin.x, angularInitForceMax.x),
                                                    Random.Range(angularInitForceMin.y, angularInitForceMax.y),
                                                    Random.Range(angularInitForceMin.z, angularInitForceMax.z));
    }

    public bool OnGettingShot(RaycastHit hit)
	{
        rigidbodyRef.AddForceAtPosition(-hit.normal * bulletForceResponse, hit.point, ForceMode.Impulse);
        return true;
	}
}
