using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DescentVFX : MonoBehaviour
{

    [SerializeField] SpriteRenderer sr;
    [SerializeField] Color boneTile, bileTile, bloodTile, monophic, wall;

    public void SetColor(int index = 0) {
        switch (index) {
            default:
            case 0:
                sr.color = boneTile;
            break;
            case 1:
                sr.color = bileTile;
            break;
            case 2:
                sr.color = bloodTile;
            break;
            case 3:
                sr.color = monophic;
            break;
            case 4:
                sr.color = wall;
            break;
        }
    }

}
