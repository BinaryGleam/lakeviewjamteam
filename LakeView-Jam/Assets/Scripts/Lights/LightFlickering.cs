using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightFlickering : MonoBehaviour
{
    [SerializeField]
    private FlickeringDefinition[] flickeringDefinition;
    // Start is called before the first frame update

    private Light m_lightRef;
    private float m_currentTime = 0;
    private bool m_flickering = false;
    private FlickeringDefinition m_activeFlickering;

    [SerializeField]
    [NaughtyAttributes.MinMaxSlider(0.5f, 10f)]
    private Vector2 m_flickeringTiming = new Vector2(1f, 5f);
    [SerializeField]
    private float m_lightIntensity = 10f;
    
    void Start()
    {
        m_lightRef = GetComponent<Light>();
        m_currentTime = 0;
        m_lightRef.intensity = m_lightIntensity;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (flickeringDefinition.Length == 0)
        {
            return;
        }

        if (!m_flickering)
        {
            m_currentTime -= Time.fixedDeltaTime;
            
            if (m_currentTime <= 0)
            {
                m_activeFlickering = flickeringDefinition[Random.Range(0, flickeringDefinition.Length - 1)];
                m_flickering = true;
                m_currentTime = 0;
            }
            return;
        }

        m_currentTime += Time.fixedDeltaTime / m_activeFlickering.FlickeringDuration;
        m_lightRef.intensity = m_activeFlickering.FlickeringIntensityCurve.Evaluate(m_currentTime) * m_lightIntensity;     

        if (m_currentTime >= 1)
        {
            m_currentTime = 0;
            m_flickering = false;
            m_currentTime = Random.Range(m_flickeringTiming.x, m_flickeringTiming.y);
            m_lightRef.intensity = m_lightIntensity;
        }
    }
}
