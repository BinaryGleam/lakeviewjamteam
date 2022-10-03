using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ScreenFader_ColorAdjustmentEffect : MonoBehaviour
{
    [SerializeField]
    private Volume m_volume;
    private ColorAdjustments m_colorAdjustments;

    [SerializeField]
    private AudioMixer m_masterMixer;

    [System.Serializable]
    private class FadeDefinition
    {
        public float Duration = 1;

        public AnimationCurve TargetPostExposure;
        public AnimationCurve TargetContrast;
        public float TargetVolumeFadeInDb;
        public float TargetPitchFade;
        public bool DisableEffectAtTheEnd;

        public UnityEvent OnFadeEnd;
    }

    [SerializeField]
    private FadeDefinition m_fadeInDefinition;
    [SerializeField]
    private FadeDefinition m_fadeOutDefinition;
    [SerializeField]
    private FadeDefinition m_finalfadeInDefinition;
    
    float m_currentTime = 0;

    private void Start()
    {
        m_masterMixer.SetFloat("MasterVolume", -80f);
        m_volume.profile.TryGet<ColorAdjustments>(out m_colorAdjustments);
        m_currentTime = -1f;

        FadeOut();
    }

    [NaughtyAttributes.Button]
    public void FadeOut()
    {
        StartCoroutine(Coroutine_Fade(m_fadeOutDefinition));
    }
    private IEnumerator Coroutine_Fade(FadeDefinition fader)
    {
        if (m_currentTime == -1)
        {
            m_colorAdjustments.active = true;
            
            m_currentTime = 0;
            m_masterMixer.GetFloat("MasterVolume", out float initialVolume);
            m_masterMixer.GetFloat("MasterPitch", out float initialPitch);

            while (m_currentTime <= 1)
            {
                m_colorAdjustments.contrast.value = fader.TargetContrast.Evaluate(m_currentTime);
                m_colorAdjustments.postExposure.value = fader.TargetPostExposure.Evaluate(m_currentTime);
                m_masterMixer.SetFloat("MasterVolume", Mathf.Lerp(initialVolume, fader.TargetVolumeFadeInDb, m_currentTime));
                m_masterMixer.SetFloat("MasterPitch", Mathf.Lerp(initialPitch, fader.TargetPitchFade, m_currentTime));

                yield return new WaitForFixedUpdate();

                m_currentTime += Time.fixedDeltaTime / fader.Duration;
            }

            fader.OnFadeEnd?.Invoke();
            m_colorAdjustments.active = !fader.DisableEffectAtTheEnd;
            m_currentTime = -1;
        }
    }

    [NaughtyAttributes.Button]
    public void FadeIn()
    {
        StartCoroutine(Coroutine_Fade(m_fadeInDefinition));
    }

    [NaughtyAttributes.Button]
    public void FinalFadeIn()
    {
        StartCoroutine(Coroutine_Fade(m_finalfadeInDefinition));
    }
}
