using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameUnitFrame : MonoBehaviour {

    [SerializeField] Image friendlyGear, enemyGear;
    [SerializeField] GameObject friendlyFrame, enemyFrame;
    public void Init(GridElement ge) {
        bool friendly = ge is not EnemyUnit;        
        friendlyFrame.SetActive(friendly);
        enemyFrame.SetActive(!friendly);
        Image gear = friendly ? friendlyGear : enemyGear;
        
    }

}
