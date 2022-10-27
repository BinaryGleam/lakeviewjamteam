using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MenuManager : MonoBehaviour
{
    public GameObject MenuEntryPoint;
    
    private void OnEnable()
    {
        EventSystem.current.SetSelectedGameObject(MenuEntryPoint);
    }

    private void OnDisable()
    {
        EventSystem.current.SetSelectedGameObject(null);
    }
}
