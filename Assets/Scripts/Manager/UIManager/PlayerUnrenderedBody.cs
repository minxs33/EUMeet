using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUnrenderedBody : MonoBehaviour
{
    [SerializeField] private List<GameObject> unrenderedBodyParts;
    void Start()
    {
        foreach(GameObject obj in unrenderedBodyParts){
            if(obj != null){
                MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
                if(renderer != null){
                    renderer.enabled = false;
                }
            }   
        }
    }
}
