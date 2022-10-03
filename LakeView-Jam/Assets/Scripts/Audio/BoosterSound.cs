using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlaySoundEffect))]
public class BoosterSound : MonoBehaviour
{
    private PlaySoundEffect m_pse;

    // Start is called before the first frame update
    void Start()
    {
        m_pse = GetComponent<PlaySoundEffect>();
    }

    // Update is called once per frame
    void PlaySound()
    {
        m_pse.Play();
    }
}
