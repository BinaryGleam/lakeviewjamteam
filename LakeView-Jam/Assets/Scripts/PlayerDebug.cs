using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDebug : MonoBehaviour
{
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.K))
		{
            GetComponent<DeathSystem>().OnDeath();
		}
    }
}
