using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableSelf : StateMachineBehaviour
{

    [SerializeField] bool parent;
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!parent)
            animator.gameObject.SetActive(false);
        else
            animator.gameObject.transform.parent.gameObject.SetActive(false);
    }


}
