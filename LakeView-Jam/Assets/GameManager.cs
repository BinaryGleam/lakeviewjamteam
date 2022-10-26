using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;


public class GameManager : SingletonManager<GameManager>
{
    protected override GameManager GetInstance()
    {
        return this;
    }

    public BoolEvent OnGamePause;

    public void Pause(bool enable)
    {
        Time.timeScale = enable ? 0.1f : 1f;

        OnGamePause?.Invoke(enable);
    }
}
