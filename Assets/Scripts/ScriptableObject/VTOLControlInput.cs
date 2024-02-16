using System.Collections;
using System.Collections.Generic;
using UnityEditor.Timeline.Actions;
using UnityEngine;

[MenuEntry("ScriptableObjects/VTOL/VTOLContrlInput")]
public class VTOLControlInput : ScriptableObject
{
    // all input signals
    Vector2 L_Horizontal = Vector2.zero;
    Vector2 L_Vertical = Vector2.zero;
    Vector2 R_Horizontal = Vector2.zero;
    Vector2 R_Vertical = Vector2.zero;
    bool reset = false;
    bool is_hover = true;
}
