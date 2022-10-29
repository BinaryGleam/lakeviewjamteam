using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.Events;

[RequireComponent(typeof(TextMeshProUGUI))]
public class UI_InGameHint : MonoBehaviour
{
    [SerializeField, TextArea]
    private string m_textToFormat = "- {rotate}\n- {roll}";

    [SerializeField]
    private string[] m_inputActionNameArray;

    private PlayerInput m_playerInput;
    private TextMeshProUGUI m_text;

    public UnityEvent OnFirstInputActionPressed;
    private InputAction m_firstAction;
    //private bool m_lastDeviceUsedIsGamepad = false;

    // Start is called before the first frame update
    void Awake()
    {
        m_text = GetComponent<TextMeshProUGUI>();
        m_playerInput = FindObjectOfType<PlayerInput>();
    }

    private void OnEnable()
    {
        m_playerInput.onControlsChanged += UpdateUIHints;
        m_firstAction = m_playerInput.actions[m_inputActionNameArray[0]];
        
        if (m_firstAction != null)
        {
            m_firstAction.started += OnFirstAction_Started;
        }
        UpdateUIHints(m_playerInput);
    }

    private void OnFirstAction_Started(InputAction.CallbackContext context)
    {
        OnFirstInputActionPressed?.Invoke();
    }

    private void OnDisable()
    {
        if (m_playerInput != null)
        {
            m_playerInput.onControlsChanged -= UpdateUIHints;
            if (m_firstAction != null)
            {
                m_firstAction.started -= OnFirstAction_Started;
            }
        }
    }

    private void UpdateUIHints(PlayerInput pInput)
    {
        string tempText = m_textToFormat;
        for (int i = 0, c = m_inputActionNameArray.Length; i < c; ++i)
        {
            string currentAction = m_inputActionNameArray[i];
            var action = pInput.actions[currentAction];
            tempText = tempText.Replace("{"+ currentAction+"}", GenerateHelpText(action));
        }

        m_text.text = tempText;
    }

    [SerializeField]
    private bool m_showInputAlternatives = false;
    [SerializeField]
    private bool m_generateFullHelpText = true;
    
    string GenerateHelpText(InputAction action)
    {
        if (action.controls.Count == 0)
            return string.Empty;

        var verb = action.type == InputActionType.Button ? "Press" : "Use";
        var lastCompositeIndex = -1;
        var isFirstControl = true;

        var controls = "";
        foreach (var control in action.controls)
        {
            var bindingIndex = action.GetBindingIndexForControl(control);
            var binding = action.bindings[bindingIndex];
            if (binding.isPartOfComposite)
            {
                if (lastCompositeIndex != -1)
                    continue;
                lastCompositeIndex = action.ChangeBinding(bindingIndex).PreviousCompositeBinding().bindingIndex;
                bindingIndex = lastCompositeIndex;
            }
            else
            {
                lastCompositeIndex = -1;
            }
            if (!isFirstControl)
                controls += " or ";
             
            controls += action.GetBindingDisplayString(bindingIndex);
            isFirstControl = false;

            if (!m_showInputAlternatives)
            {
                break;
            }    
        }
        return m_generateFullHelpText ? $"{verb} <i>{controls}</i> to {action.name.ToLower()}" : $"{controls}";
    }
}
