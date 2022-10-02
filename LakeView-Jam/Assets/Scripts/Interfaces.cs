using UnityEngine;

public interface IShootable 
{
	bool OnGettingShot(RaycastHit hit);
}

public interface IKillable
{
	bool OnDeath();
}