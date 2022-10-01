using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraResolution : MonoBehaviour
{
    [SerializeField]
    private Vector2 m_ScreenResolution = new Vector2(640, 480);

    // Start is called before the first frame update
    void Update()
    {
        Screen.SetResolution((int)m_ScreenResolution.x, (int)m_ScreenResolution.y, true);
    }
}
