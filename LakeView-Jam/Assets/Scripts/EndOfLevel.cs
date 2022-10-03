using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public class EndOfLevel : MonoBehaviour
{
    public string nextLevel = "";
	public UnityEvent onTriggerEnter = null;
	private void OnTriggerEnter(Collider other)
	{
		if(other.tag == "Player")
		{
			onTriggerEnter?.Invoke();
		}
	}

	public void LoadNextLevel()
	{
		SceneManager.LoadSceneAsync(nextLevel, LoadSceneMode.Single);
	}
}
