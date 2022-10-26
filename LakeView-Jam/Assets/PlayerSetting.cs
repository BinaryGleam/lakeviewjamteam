using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSetting : MonoBehaviour
{
    [SerializeField]
    private bool m_invertRotationX = false;
    [SerializeField]
    private bool m_invertRotationY = false;
    [SerializeField]
    private bool m_invertRoll = false;

    public BoolEvent OnInvertRotationXChanged;
    public BoolEvent OnInvertRotationYChanged;
    public BoolEvent OnInvertRollChanged;

    private void Start()
    {
        DontDestroyOnLoad(this);
    }

    public void RefreshSettings()
    {
        OnInvertRotationXChanged?.Invoke(m_invertRotationX);
        OnInvertRotationYChanged?.Invoke(m_invertRotationY);
        OnInvertRollChanged?.Invoke(m_invertRoll);
    }

    public void InvertRotationX(bool value)
    {
        if (value != m_invertRotationX)
        {
            m_invertRotationX = value;
            OnInvertRotationXChanged?.Invoke(value);
        }
    }

    public void InvertRotationY(bool value)
    {
        if (value != m_invertRotationY)
        {
            m_invertRotationY = value;
            OnInvertRotationYChanged?.Invoke(value);
        }
    }

    public void InvertRoll(bool value)
    {
        if (value != m_invertRoll)
        {
            m_invertRoll = value;
            OnInvertRollChanged?.Invoke(value);
        }
    }
}
