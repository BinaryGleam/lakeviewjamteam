using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    private Animator m_doorAnimator = null;
    DoorButton[] buttonsRef = null;

    private int ConnectedButtons = 0;

	private void Awake()
	{
        m_doorAnimator = GetComponent<Animator>();
		if(m_doorAnimator == null)
		{
            Debug.LogError("No animator on my door");
		}
	}

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
            m_doorAnimator.SetBool("Open", true);
            Destroy(this);
		}
    }
}
