using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class InheritenceCheck : MonoBehaviour
{
    [SerializeField] bool check;
   
    [SerializeField] GridElement one, two;

    void Update() {
        if (check) {
            if (one != null && two != null) {
                if (two.GetType().IsSubclassOf(one.GetType())) {
                    Debug.Log("IsSubclassOf");
                } else 
                    Debug.Log("Is not SubclassOf");

            }

            check = false;
        }
    }

}
