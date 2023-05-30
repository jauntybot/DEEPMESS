using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoverHighlight : MonoBehaviour
{

    public GameObject leftHighlight;
    public GameObject rightHighlight;

    [Header("Enter Values")]
    public int leftEnterValue;
    public int rightEnterValue;

    [Header("Exit Values")]
    public int leftExitValue;
    public int rightExitValue;

    public float animSpeed;

    public void HighlightEnter()
    {
        LeanTween.moveLocalX(leftHighlight, leftEnterValue, animSpeed).setEase(LeanTweenType.easeInQuad);
        LeanTween.moveLocalX(rightHighlight, rightEnterValue, animSpeed).setEase(LeanTweenType.easeInQuad);
    }

    public void HighlightExit()
    {
        LeanTween.moveLocalX(leftHighlight, leftExitValue, animSpeed).setEase(LeanTweenType.easeInQuad);
        LeanTween.moveLocalX(rightHighlight, rightExitValue, animSpeed).setEase(LeanTweenType.easeInQuad);
    }

}
