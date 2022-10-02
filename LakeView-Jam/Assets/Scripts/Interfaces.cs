using UnityEngine;

public interface IShootable 
{
	bool OnGettingShot(RaycastHit hit);
}