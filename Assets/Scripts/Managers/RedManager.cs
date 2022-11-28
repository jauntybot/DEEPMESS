using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RedManager : TokenManager {

    
    public IEnumerator TakeTurn() {

        yield return new WaitForSecondsRealtime(1/Util.fps);
        foreach (Token token in tokens) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            CalculateAction(token);
        }
    }

    public void CalculateAction(Token input) {
        input.UpdateValidAttack();
    }

}
