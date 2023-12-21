using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameUnitFrame : MonoBehaviour {

    [SerializeField] Image frame, mini;
    [SerializeField] Sprite enemyFrame, slagFrame;
    
    public void Init(GridElement ge) {
        if (ge is EnemyUnit) {
            frame.sprite = enemyFrame;
        } else {
            frame.sprite = slagFrame;
        }
        mini.sprite = ge.gfx[0].sprite;
    }

}
