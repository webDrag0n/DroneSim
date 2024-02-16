using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GizmosTest : MonoBehaviour
{
    void OnDrawGizmosSelected()
    {

#if UNITY_EDITOR
        Gizmos.color = Color.red;

        //Draw the suspension
        Gizmos.DrawLine(
            Vector3.zero,
            Vector3.up
        );

        //draw force application point
        Gizmos.DrawWireSphere(Vector3.zero, 0.05f);

        Gizmos.color = Color.white;
#endif
    }
}
