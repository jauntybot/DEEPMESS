using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Custom data class, stores card type enum, sprite, and other things to come

[CreateAssetMenu(menuName = "Cards/New Card")]
[System.Serializable]
public class CardData : ScriptableObject {

    public Sprite graphic;
    public enum Action { Move, Attack, Defend };
    public Action action;

    public CardData(CardData c) {
        this.graphic=c.graphic;
        this.action=c.action;
    }
}