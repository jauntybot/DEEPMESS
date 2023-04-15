using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
 
[RequireComponent(typeof(LineRenderer))]
public class LineController : MonoBehaviour
{
    public enum AnimationType{None,SingleColorMorph,MultiColorMorph,Shuffle,Shift};
    public AnimationType myAnimationType;
    LineRenderer myLineRenderer;
    public float morphTime = 2;    
 
    void Start()
    {
        myLineRenderer = this.GetComponent<LineRenderer>();
        switch(myAnimationType)
        {
            case AnimationType.SingleColorMorph:
                StartCoroutine(RandomSingleColorMorphing(myLineRenderer,morphTime));
                break;
            case AnimationType.MultiColorMorph:
                StartCoroutine(RandomMultiColorMorphing(myLineRenderer,morphTime));
                break;
            case AnimationType.Shuffle:
                StartCoroutine(ShuffleGradient(myLineRenderer, .5f));
                break;
            case AnimationType.Shift:
                StartCoroutine(AnimateLoop(myLineRenderer));
                break;
        }
    }
 
    void SetSingleColor(LineRenderer lineRendererToChange, Color newColor)
    {
        lineRendererToChange.startColor = newColor;
        lineRendererToChange.endColor = newColor;
    }
 
    void SetSingleColor2(LineRenderer lineRendererToChange, Color newColor)
    {
        Gradient tempGradient = new Gradient();
 
        GradientColorKey[] tempColorKeys = new GradientColorKey[2];
        tempColorKeys[0] = new GradientColorKey(newColor,0);
        tempColorKeys[1] = new GradientColorKey(newColor,1);
 
        tempGradient.colorKeys = tempColorKeys;
 
        lineRendererToChange.colorGradient = tempGradient;
    }
 
    void SetSingleColor3(LineRenderer lineRendererToChange, Color newColor)
    {
        Gradient tempGradient = lineRendererToChange.colorGradient;
 
        GradientColorKey[] tempColorKeys = tempGradient.colorKeys;
        for(int i =0 ; i < tempColorKeys.Length; i++)
        {
            tempColorKeys[i].color = newColor;
        }
 
        tempGradient.colorKeys = tempColorKeys;
 
        lineRendererToChange.colorGradient = tempGradient;
    }
    
    IEnumerator ShuffleGradient(LineRenderer targetLineRenderer, float waitTime)
    {
        while(true)
        {
            ShuffleGradient(targetLineRenderer);
            yield return new WaitForSeconds(waitTime);
        }
    }
 
    void ShuffleGradient(LineRenderer targetLineRenderer)
    {
        GradientColorKey[] newColorKeys = targetLineRenderer.colorGradient.colorKeys;
        for(int i = 0 ; i <newColorKeys.Length; i ++)
        {
            Color tempColor = newColorKeys[i].color;
            int randomIndex = Random.Range(0,newColorKeys.Length-1);
            newColorKeys[i].color = newColorKeys[randomIndex].color;
            newColorKeys[randomIndex].color = tempColor;
        }
        Gradient tempGradient = targetLineRenderer.colorGradient;
        tempGradient.colorKeys = newColorKeys;
        targetLineRenderer.colorGradient = tempGradient;
    }
    
    
    IEnumerator RandomMultiColorMorphing(LineRenderer lineRendererToChange, float timeToMorph)
    {
        float time = 0;
 
        while(true)
        {
            GradientColorKey[] initialColorKeys = lineRendererToChange.colorGradient.colorKeys;
            GradientColorKey[] newColorKeys = GenerateRandomColorKeys(initialColorKeys);
            time = 0;
            while(time<timeToMorph)
            {
                time += Time.deltaTime;
                float progress = time/timeToMorph;
                GradientColorKey[] currentColorKeys = GradientColorKeyLerp(initialColorKeys,newColorKeys,progress);
                Gradient tempGradient = lineRendererToChange.colorGradient;
                tempGradient.colorKeys = currentColorKeys;
                lineRendererToChange.colorGradient = tempGradient;
                yield return null;
            }
            yield return null;
        }
    }
    
    GradientColorKey[] GradientColorKeyLerp(GradientColorKey[] initialColorKeys, GradientColorKey[] endColorKeys, float progress)
    {
        GradientColorKey[] newColorKeys = new GradientColorKey[initialColorKeys.Length];
        for(int i = 0; i < newColorKeys.Length; i++)
        {
            newColorKeys[i].color = Color.Lerp(initialColorKeys[i].color, endColorKeys[i].color, progress);
            newColorKeys[i].time = initialColorKeys[i].time;
        }
        return newColorKeys;
    }
 
    //assigns new color to each colorkey and uses Time from incomingColorKeys
    GradientColorKey[] GenerateRandomColorKeys(GradientColorKey[] incomingColorKeys)
    {
        GradientColorKey[] newColorKeys = new GradientColorKey[incomingColorKeys.Length];
        for(int i = 0; i < newColorKeys.Length ; i++)
        {
            newColorKeys[i].color = RandomColor();
            newColorKeys[i].time = incomingColorKeys[i].time;
        }
        return newColorKeys;
    }
 
    //asumes Single color, 2 colorkeys
    IEnumerator RandomSingleColorMorphing(LineRenderer lineRendererToChange, float timeToMorph)
    {
        float time = 0;
        Color initialColor = lineRendererToChange.colorGradient.colorKeys[0].color;
        //this reduces colorkey amount to 2 just in case.
        SetSingleColor2(lineRendererToChange,initialColor); 
 
        while(true)
        {
            initialColor = lineRendererToChange.colorGradient.colorKeys[0].color;
            Color targetColor = RandomColor();
            time = 0;
            while(time<timeToMorph)
            {
                time+=Time.deltaTime;
                float progress = time/timeToMorph;
                Color currentColor = Color.Lerp(initialColor,targetColor,progress);
                SetSingleColor(lineRendererToChange,currentColor);
                yield return null;
            }
            yield return null;
        }
    }
 
    //Basically Color.Lerp?
    Color ColorLerpMath(Color firstColor, Color secondColor, float progress){
        Vector3 firstRGB = new Vector3(firstColor.r,firstColor.g,firstColor.b);
        Vector3 secondRGB = new Vector3(secondColor.r,secondColor.g,secondColor.b);
        Vector3 difference = secondRGB-firstRGB;
        Vector3 lerpedRGB = firstRGB + (progress*difference);
        return new Color(lerpedRGB.x,lerpedRGB.y,lerpedRGB.z);
    }
    
    Color RandomColor()
    {
        return new Color(Random.Range(0f,1f),Random.Range(0f,1f),Random.Range(0f,1f));
    }
    
    //returns the gradient with a copy of the first key for intersection purposes.
    Gradient AddInitialCopy(Gradient incomingGradient)
    {
        List<GradientColorKey> newColorKeys = new List<GradientColorKey>(incomingGradient.colorKeys);
        Color interSectionColor = newColorKeys[0].color;
        newColorKeys.Insert(0,new GradientColorKey(interSectionColor,0));
        Gradient newInitGradient = new Gradient();
        newInitGradient.colorKeys = newColorKeys.ToArray();
        return newInitGradient;
    }
 
    //remove first and last keys since they dont shift.
    List<GradientColorKey> RemoveFirstAndLast(Gradient incomingGradient)
    {
        List<GradientColorKey> currentColorKeys = new List<GradientColorKey>(incomingGradient.colorKeys);
        currentColorKeys.RemoveAt(currentColorKeys.Count-1);
        currentColorKeys.RemoveAt(0);
        return currentColorKeys;
    }
 
    Color GetIntersectionColor(List<GradientColorKey> incomingKeys, int lowestIndex, int highestIndex)
    {
        Color firstColor = incomingKeys[lowestIndex].color;
        Color lastColor = incomingKeys[highestIndex].color;
        float distance = 1 - (incomingKeys[highestIndex].time - incomingKeys[lowestIndex].time);
        float colorLerpAmount = (1f-incomingKeys[highestIndex].time) / distance;;
        Color newIntersectionColor = Color.Lerp(lastColor,firstColor,colorLerpAmount);
        return newIntersectionColor;
    }
 
    //accepts max 7 colors, 1st and last should be at 0 and 1
    IEnumerator AnimateLoop(LineRenderer lineRendererToChange, float movementPerTick = .001f)
    {
        lineRendererToChange.colorGradient = AddInitialCopy(lineRendererToChange.colorGradient);
 
        while(true)
        {
            List<GradientColorKey> currentColorKeys = RemoveFirstAndLast(lineRendererToChange.colorGradient);
            float highestTime=0;
            float lowestTime=1;
            int highestIndex = currentColorKeys.Count-1;
            int lowestIndex = 0;
            //Move all inner ones.
            for(int i = 0 ;i<currentColorKeys.Count;i++)
            {
                GradientColorKey tempColorKey = currentColorKeys[i];
                float newTime = tempColorKey.time + movementPerTick;
                
                if(newTime>1)
                {
                    newTime = newTime-1;
                }
                tempColorKey.time = newTime;
                currentColorKeys[i] = tempColorKey;
                if(newTime<lowestTime)
                {
                    lowestTime = newTime;
                    lowestIndex = i;
                }
                if(newTime>highestTime)
                {
                    highestTime = newTime;
                    highestIndex = i;
                }
            }
            Color newIntersectionColor = GetIntersectionColor(currentColorKeys,lowestIndex,highestIndex);
            currentColorKeys.Insert(0,new GradientColorKey(newIntersectionColor,0));
            currentColorKeys.Add(new GradientColorKey(newIntersectionColor,1));
            Gradient tempGradient = lineRendererToChange.colorGradient;
            tempGradient.colorKeys = currentColorKeys.ToArray();
            lineRendererToChange.colorGradient = tempGradient;  
            yield return null;
        }
    }
 
    void AssignGradient(LineRenderer targetLineRenderer, Gradient newGradient)
    {
        targetLineRenderer.colorGradient = newGradient;
    }
 
    void DrawTestLine()
    {
        Vector3 firstPos = new Vector3(-5,0,0);
        Vector3 secondPos = new Vector3(5,0,0);
        int resolution = 100;
        myLineRenderer.positionCount = resolution;
        myLineRenderer.SetPositions(MakeLine(firstPos,secondPos,100));
    }
 
    //makes a line from point A to point B with resolution of size points
    Vector3[] MakeLine(Vector3 initPos, Vector3 endPos, int points)
    {
        Vector3 difference = endPos-initPos;
        Vector3[] newLine = new Vector3[points];
        Vector3 differencePerPoint = difference/(float)(points-1);
        for(int i = 0; i < points ; i++)
        {
            newLine[i] = initPos + (differencePerPoint*i);
        }
        return newLine;
    }
 
}