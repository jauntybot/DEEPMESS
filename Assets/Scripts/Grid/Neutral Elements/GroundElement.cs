using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundElement : GridElement
{

    
    public override IEnumerator CollideFromBelow(GridElement above) {
        yield return base.CollideFromBelow(above);
        OnSharedSpace(above);
    }

    

}
