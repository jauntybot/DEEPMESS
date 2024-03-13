using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class StringCountUp {
    public static IEnumerator CountUp(int countTo, float dur, System.Action<string> callback) {
        float t = 0;
        int c = 0;
        callback(c.ToString());
        while (t <= dur) {
            c = Mathf.RoundToInt(Mathf.Lerp(0, countTo, t/dur));
            callback(c.ToString());

            yield return null;
            
            t+=Time.deltaTime;
        }
        c = countTo;
        callback(c.ToString());
    }
}
