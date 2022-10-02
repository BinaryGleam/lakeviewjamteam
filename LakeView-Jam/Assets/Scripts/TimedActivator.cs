using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimedActivator : MonoBehaviour
{
    [SerializeField]
    private GameObject[] toToggle = null;

    [SerializeField]
    private float timeLimit = 4f;
    private float chrono = 0f;

    void Start()
    {
        chrono = timeLimit;
    }

    void Update()
    {
        chrono -= Time.deltaTime;
        if(chrono <= 0f)
		{
            chrono = timeLimit;
			foreach (GameObject go in toToggle)
			{
                go.SetActive(!go.activeInHierarchy);
			}
		}
    }
}
