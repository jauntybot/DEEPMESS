using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Relics {
    
    public abstract class RelicData : ScriptableObject {
        
        public new string name;
        public string description;
        public Sprite sprite;
        public enum Rarity { Common, Uncommon, Rare };
        public Rarity rarity;
        public int scrapValue;

        public virtual void Init() {}


        public virtual void UnsubRelic() {}
    }
}
