using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Cards/New Card")]
[System.Serializable]
public class CardData : ScriptableObject {

    public Sprite graphic;
    public List<Path> paths;

    public CardData(CardData c) {
        this.graphic=c.graphic;
        this.paths=c.paths;
    }
}

[System.Serializable]
public class Path {
    public List<Vector2> moveTo;
}