using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Card : MonoBehaviour {

    public CardData cardData;
    public GameObject obj;

    public Card(CardData cd, GameObject go) {
        this.cardData=cd;
        this.obj=go;
    }

    public void Initialize(GameObject go) {
        this.obj=go;
    }
}