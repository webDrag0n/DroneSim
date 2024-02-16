using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackTargetController : MonoBehaviour
{

    private Transform self_target_transform;

    public void SetDeltaTransform(Transform t)
    {
        self_target_transform.position += t.position;
        self_target_transform.rotation *= t.rotation;
    }

    public void SetDeltaRotation(Transform t)
    {
        self_target_transform.position += t.position;
    }

    // Start is called before the first frame update
    void Start()
    {
        self_target_transform.position = Vector3.zero;
        self_target_transform.rotation = Quaternion.identity;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.Lerp(transform.position, self_target_transform.position, Time.deltaTime);
    }
}
