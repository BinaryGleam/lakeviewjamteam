using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

[RequireComponent(typeof(AudioSource))]
public class AudioTest : MonoBehaviour
{
    AudioSource m_audioSource;

    // Start is called before the first frame update
    void Start()
    {
        m_audioSource = GetComponent<AudioSource>();
    }

    [Button]
    // Update is called once per frame
    void PlaySound()
    {
        if (m_audioSource.isPlaying)
        {
            return;
        }
        m_audioSource.Play();
    }
}
