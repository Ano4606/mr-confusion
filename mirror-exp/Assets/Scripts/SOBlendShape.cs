using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BlinkBlendShapes", menuName = "Avatar/Blink BlendShapes")]
public class SOBlinkBlendShape : ScriptableObject
{
    public List<string> LeftEyeBlendshapeNames;
    public List<string> RightEyeBlendshapeNames;
}