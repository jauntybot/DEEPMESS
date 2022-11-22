using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RedManager : TokenManager {

    
    public IEnumerator TakeTurn() {

        yield return new WaitForSecondsRealtime(1/Util.fps);

    }

    
}
