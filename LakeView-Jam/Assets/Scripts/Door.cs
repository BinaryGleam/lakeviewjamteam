using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    DoorButton[] buttonsRef = null;

    private int ConnectedButtons = 0;

    void Start()
    {
        buttonsRef = GetComponentsInChildren<DoorButton>();
        ConnectedButtons = buttonsRef.Length;
		foreach (DoorButton button in buttonsRef)
		{
            button.onBreakingEvent += OnButtonBreak;
		}
    }

    private void OnButtonBreak()
	{
        ConnectedButtons--;
        if(ConnectedButtons <= 0)
		{
            Destroy(gameObject);
		}
    }
}
