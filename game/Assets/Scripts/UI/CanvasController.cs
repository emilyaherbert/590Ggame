using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasController : MonoBehaviour
{

    Vector3 trueRotation = new Vector3(90.0f, 0.0f, 0.0f);

    void LateUpdate() {
        transform.localRotation = Quaternion.Euler(-transform.parent.rotation.eulerAngles + trueRotation);
    }
}
