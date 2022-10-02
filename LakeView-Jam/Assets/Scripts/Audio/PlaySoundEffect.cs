using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

[RequireComponent(typeof(AudioSource))]
public class PlaySoundEffect : MonoBehaviour
{
    [SerializeField]
    private AudioClip[] m_audioClipArray;

    [SerializeField]
    [MinMaxSlider(0, 2)]
    private Vector2 m_pitchRange = new Vector2(0.85f, 1.15f);

    private AudioSource m_AudioSource;

    private void Start()
    {
        m_AudioSource = GetComponent<AudioSource>();
    }

    [Button("Play Random SE")]
    public void Play()
    {
#if UNITY_EDITOR
        if (m_AudioSource == null)
        {
            m_AudioSource = GetComponent<AudioSource>();
        }
#endif

        if (m_audioClipArray == null || m_audioClipArray.Length == 0)
        {
            return;
        }

        if (m_AudioSource.isPlaying)
        {
            m_AudioSource.Stop();
        }

        int clipIndex = Random.Range(0, m_audioClipArray.Length - 1);
        m_AudioSource.clip = m_audioClipArray[clipIndex];
        m_AudioSource.pitch = Random.Range(m_pitchRange.x, m_pitchRange.y);

        m_AudioSource.Play();
    }

    public void PlayAtLocation(Vector3 location)
    {
        transform.position = location;
        Play();
    }
}
