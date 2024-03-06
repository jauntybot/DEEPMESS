using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Relic : MonoBehaviour {

    RelicData data;
    Image relicSprite;

    public void Init(RelicData _data) {
        relicSprite = GetComponent<Image>();
        relicSprite.sprite = data.sprite;
    }

}
