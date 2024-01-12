using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroySelf : StateMachineBehaviour {

    [SerializeField] bool parent;

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        if (parent)
            Destroy(animator.transform.parent.gameObject);
        else
            Destroy(animator.gameObject);
    }

}
