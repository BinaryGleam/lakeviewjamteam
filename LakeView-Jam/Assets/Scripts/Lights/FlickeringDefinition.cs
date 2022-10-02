using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class FlickeringDefinition : ScriptableObject
{
    public AnimationCurve FlickeringIntensityCurve;
    public float FlickeringDuration = 1f;
}