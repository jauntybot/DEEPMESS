using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Facsimile of lua Global, will revise this purpose

public class Util : MonoBehaviour
{
    public static float initD = 0.05f;
    public static float cardSize=2.5f;
    public static int fps = 60;

    public static Vector3 SampleParabola ( Vector3 start, Vector3 end, float h, float t ) {
        float parabolicT = t * 2 - 1;
        if ( Mathf.Abs( start.y - end.y ) < 0.1f ) {
            //start and end are roughly level, pretend they are - simpler solution with less steps
            Vector3 travelDirection = end - start;
            Vector3 result = start + t * travelDirection;
            result.y += ( -parabolicT * parabolicT + 1 ) * h;
            return result;
        } else {
            //start and end are not level, gets more complicated
            Vector3 travelDirection = end - start;
            Vector3 levelDirecteion = end - new Vector3( start.x, end.y, start.z );
            Vector3 right = Vector3.Cross( travelDirection, levelDirecteion );
            Vector3 result = start + t * travelDirection;
            result += ( ( -parabolicT * parabolicT + 1 ) * h ) * Vector3.up;
            return result;
        }
    }

    public static float Remap (this float value, float from1, float to1, float from2, float to2) {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }

    public static List<Vector3> SampledParabola ( Vector3 start, Vector3 end, float h, int segments ) {
        List<Vector3> pts = new List<Vector3>();        
        float time = 0;
        for (float i = 0; i < segments; i+=1) {
            time = i/segments;
            float parabolicT = time * 2 - 1;
            Vector3 result = Vector3.zero;
            if ( Mathf.Abs( start.y - end.y ) < 0.1f ) {
                //start and end are roughly level, pretend they are - simpler solution with less steps
                Vector3 travelDirection = end - start;
                result = start + time * travelDirection;
                result.y += ( -parabolicT * parabolicT + 1 ) * h;
            } else {
                //start and end are not level, gets more complicated
                Vector3 travelDirection = end - start;
                Vector3 levelDirecteion = end - new Vector3( start.x, end.y, start.z );
                Vector3 right = Vector3.Cross( travelDirection, levelDirecteion );
                result = start + time * travelDirection;
                result += ( ( -parabolicT * parabolicT + 1 ) * h ) * Vector3.up;
            }

            pts.Add(result);
        }
        
        return pts;
    }

}
