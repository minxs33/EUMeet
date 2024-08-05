using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AuthSceneInit : MonoBehaviour
{   void Start()
    {
        if(PlayerPrefs.HasKey("error"))
        {
            Debug.LogError(PlayerPrefs.GetString("error"));
            PlayerPrefs.DeleteKey("error");
        }else
        {
            Debug.Log("No error message");
        }
    }
}
