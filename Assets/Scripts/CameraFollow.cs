using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public static CameraFollow instance {
        get => _singleton;
        set
        {
            if(value == null)
                _singleton = null;
            else if (_singleton == null)
                _singleton = value;
            else if (_singleton != value)
            {
                Destroy(value);
                Debug.LogError($"There should only ever be one instance of {nameof(CameraFollow)} in the scene. Destroying the new instance.");
            }
        }
    }

    private static CameraFollow _singleton;

    private Transform target;

    private void Awake() {
        instance = this;
    }

    private void OnDestroy() {
        if(instance == this)
            instance = null;
    }

    private void LateUpdate() {
        if(target != null)
        {
            transform.SetPositionAndRotation(target.position, target.rotation);
        }
    }

    public void SetTarget(Transform newTarget) {
        target = newTarget;
    }
}
