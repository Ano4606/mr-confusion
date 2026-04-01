using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "BlinkingBlendshapes", menuName = "ScriptableObjects/SOBlinkBlendShape", order = 1)]
public class SOBlinkBlendShape : ScriptableObject
{
    [SerializeField] private List<string> eyesBlendshapeName = new List<string>();
    public List<string> EyesBlendshapeName {  get { return eyesBlendshapeName; }  }
}
