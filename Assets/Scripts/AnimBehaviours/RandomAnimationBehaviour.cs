using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomAnimationBehaviour : StateMachineBehaviour
{
    [SerializeField] string animatorParameter;
    [SerializeField] int numOfStates;

    // OnStateMachineEnter is called when entering a state machine via its Entry Node
    override public void OnStateMachineEnter(Animator animator, int stateMachinePathHash)
    {
        animator.SetInteger(animatorParameter, Random.Range(0, numOfStates));
    }
}
