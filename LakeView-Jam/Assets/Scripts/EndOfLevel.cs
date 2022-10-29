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
	
	public bool forceTriggerZoneOnLevelLoad = false;

	private void OnTriggerEnter(Collider other)
	{
		if(other.tag == "Player" && other.GetComponent<PlayerMovements>().enabled)
		{
			onTriggerEnter?.Invoke();
		}
	}

	private bool m_isLoading = false;
	public void DelayedLevelLoading(float delayInSecond)
    {
		if (!m_isLoading)
        {
			StartCoroutine(DelayedLoading_Coroutine(delayInSecond));
        }
    }

	IEnumerator DelayedLoading_Coroutine(float delay)
    {
		m_isLoading = true;
		
		if (forceTriggerZoneOnLevelLoad)
        {
			onTriggerEnter?.Invoke();
		}

		yield return new WaitForSeconds(delay);

		LoadNextLevel();
		m_isLoading = false;
	}

	public void LoadNextLevel()
	{
		SceneManager.LoadSceneAsync(nextLevel, LoadSceneMode.Single);
	}
}
