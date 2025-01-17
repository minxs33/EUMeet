#if UNITY_SERVER
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalizeDoor : MonoBehaviour
{
    void Start()
    {
        this.gameObject.SetActive(false);
    }

}
#endif