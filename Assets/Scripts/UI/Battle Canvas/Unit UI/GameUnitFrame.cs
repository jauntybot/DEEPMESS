using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameUnitFrame : MonoBehaviour {

    [SerializeField] Image frame, miniBG, mini;
    [SerializeField] Sprite enemyFrame, enemyMiniBG, slagFrame, slagMiniBG;
    
    public void Init(GridElement ge) {
        if (ge is EnemyUnit) {
            frame.sprite = enemyFrame;
            miniBG.sprite = enemyMiniBG;
        } else {
            frame.sprite = slagFrame;
            miniBG.sprite = slagMiniBG;
        }
        mini.sprite = ge.gfx[0].sprite;
    }

}
