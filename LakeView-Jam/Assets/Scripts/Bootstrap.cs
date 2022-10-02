using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bootstrap : MonoBehaviour
{
    [SerializeField]
    private Vector2 screenResolution = new Vector2(640, 480);
    [SerializeField]
    private int m_targetFramerate = 30;

    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = m_targetFramerate;
        Screen.SetResolution((int)screenResolution.x, (int)screenResolution.y, true);
    }
}
