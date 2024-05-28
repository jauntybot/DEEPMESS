using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace Relics {

    public class Relic : MonoBehaviour {

        RelicData data;
        Image relicSprite;

        public void Init(RelicData _data) {
            data = _data;
            data.Init();

            relicSprite = GetComponent<Image>();
            relicSprite.sprite = data.sprite;
            gameObject.name = data.name;

            TooltipTrigger trigger = GetComponent<TooltipTrigger>();
            trigger.header = data.name;
            trigger.content = data.description;
        }

        public void ScrapRelic() {
            data.UnsubRelic();
            Destroy(gameObject);
        }
    }
}
