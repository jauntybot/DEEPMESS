using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeMovementCard : Card
{

    protected override void Start()
    {
        base.Start();
        Initialize(data);
    }
    public void Resupply() {

        gameObject.SetActive(true);
        EnableInput(true);

    }

    public void PlayCard() {
        gameObject.SetActive(false);
        EnableInput(false);
    }


}
