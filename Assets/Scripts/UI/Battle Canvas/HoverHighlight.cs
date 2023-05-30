using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoverHighlight : MonoBehaviour
{

    public GameObject leftHighlight;
    public GameObject rightHighlight;

    [Header("Enter Values")]
    public int leftEnterValue;

    [Header("Exit Values")]
    public int leftExitValue;

    public float animSpeed;

    public void HighlightEnter()
    {
        LeanTween.moveLocalX(leftHighlight, leftEnterValue, animSpeed).setEase(LeanTweenType.easeInQuad);
        LeanTween.moveLocalX(rightHighlight, Mathf.Abs(leftEnterValue), animSpeed).setEase(LeanTweenType.easeInQuad);
    }

    public void HighlightExit()
    {
        LeanTween.moveLocalX(leftHighlight, leftExitValue, animSpeed).setEase(LeanTweenType.easeInQuad);
        LeanTween.moveLocalX(rightHighlight, Mathf.Abs(leftExitValue), animSpeed).setEase(LeanTweenType.easeInQuad);
    }

}
