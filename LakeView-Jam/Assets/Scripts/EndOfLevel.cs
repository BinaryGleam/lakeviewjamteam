using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using NaughtyAttributes;

public class EndOfLevel : MonoBehaviour
{
	[Scene]
    public string nextLevel = "";
	public UnityEvent onTriggerEnter = null;
	private void OnTriggerEnter(Collider other)
	{
		if(other.tag == "Player" && other.GetComponent<PlayerMovements>().enabled)
		{
			onTriggerEnter?.Invoke();
		}
	}

	public void LoadNextLevel()
	{
		SceneManager.LoadSceneAsync(nextLevel, LoadSceneMode.Single);
	}
}
