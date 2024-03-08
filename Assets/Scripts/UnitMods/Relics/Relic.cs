using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Relic : MonoBehaviour {

    Relics.RelicData data;
    Image relicSprite;

    public void Init(Relics.RelicData _data) {
        data = _data;

        relicSprite = GetComponent<Image>();
        relicSprite.sprite = data.sprite;
        gameObject.name = data.name;

        
    }
}
