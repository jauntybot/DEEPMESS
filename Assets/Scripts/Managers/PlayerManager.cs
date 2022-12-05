using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Class that controls the player's functionality, recieves input from PlayerController and ScenarioManager
// tbh I'm not commenting this one bc it's the most heavily edited and refactors are incoming

[RequireComponent(typeof(Deck))]
[RequireComponent(typeof(PlayerController))]
public class PlayerManager : UnitManager {
    
    PlayerController pc;


// Turn vars
    public int currentEnergy, maxEnergy;
    public TMPro.TMP_Text energyText;
    public GameObject energyWarning;

// Card vars
    public List<Card> hand;
    [HideInInspector] public Deck deck;
    public Card selectedCard;
    [SerializeField] protected int handLimit;


    protected override void Start() {
        base.Start();
        pc = GetComponent<PlayerController>();
        deck=GetComponent<Deck>();
    }

    public override IEnumerator Initialize()
    {
        deck.InitializeDeck();
        yield return base.Initialize();
    }

    public void StartEndTurn(bool start) {
        for (int i = 0; i <= units.Count - 1; i++) 
            units[i].EnableSelection(start);
        
        ToggleHandSelect(start);

        if (start) {
            DrawToHandLimit();
            currentEnergy = maxEnergy;
            UpdateEnergyDisplay();

            StartCoroutine(pc.GridInput());
            StartCoroutine(UpdateHandDisplay());
        } else {
            DeselectCard();
            DeselectUnit();
            DiscardHand();
        }
    }

// Overriden functionality
    public override Unit SpawnUnit(Vector2 coord, int index) {
        Unit t = base.SpawnUnit(coord, index);
        t.owner = Unit.Owner.Player;
        return t;
    }

    public override void SelectUnit(Unit t) {
        base.SelectUnit(t); 
        
        if (selectedCard) 
            selectedUnit.UpdateAction(selectedCard);
              

    }
 
    public override void DeselectUnit() {
        if (selectedUnit) {
            selectedUnit.UpdateAction();

            base.DeselectUnit(); 
        }
    }
    
    public override IEnumerator MoveUnit(Vector2 moveTo) 
    {
        if (PlayCard())
            yield return base.MoveUnit(moveTo);
    }

    public override IEnumerator AttackWithUnit(Vector2 attackAt) 
    {
        if (PlayCard())
            yield return base.AttackWithUnit(attackAt);
    }

    public override IEnumerator DefendUnit(int value)
    {
        if (PlayCard())
            yield return base.DefendUnit(value);
    }



    public void ToggleHandSelect(bool state) {
        foreach (Card card in hand) {
            card.hover.active = false;
            card.selectable = state;
        }
        if (selectedCard) selectedCard.hover.active = true;
        
        if (state) {
            StartCoroutine(pc.HandInput());
        } else 
            DeselectCard();
    }

// Hand mgmt
    public IEnumerator UpdateHandDisplay() {
        var height = 2*Camera.main.orthographicSize;
        var width = height*Camera.main.aspect/2;
        int i=0;
        foreach (Card card in hand) {
            yield return new WaitForSeconds(Util.initD);

            card.gameObject.SetActive(true);
            card.EnableInput(true);

            float scale = Mathf.Clamp(width/hand.Count/3f, 0.25f, 1.5f);
            card.transform.localScale = Vector3.one * scale;
            
            float w = Util.cardSize * card.transform.localScale.x * hand.Count;
            card.transform.position = new Vector2(
                -w + Camera.main.transform.position.x + (Util.cardSize * card.transform.localScale.x * i) , 
                -9);
            card.hover.UpdateOrigins();

            i++;
        }
    }

    public void UpdateEnergyDisplay() {
        energyText.text = currentEnergy.ToString();
        if (currentEnergy <= 0)
            StartCoroutine(EnergyWarning());
        else
            energyWarning.SetActive(false);
    }

    protected virtual void DrawToHandLimit() {
        int toDraw = handLimit - hand.Count;
        for (int i = 0; i < toDraw; i++) {
            hand.Add(deck.DrawCard());
        }
    }

    protected virtual void DiscardHand() {
        for (int i = hand.Count - 1; i >= 0; i--) {
            hand[i].gameObject.SetActive(false);
            hand[i].EnableInput(false);
            deck.discard.Add(hand[i]);
            hand.Remove(hand[i]);
        }
    }

    public virtual void SelectCard(Card c) {
        if (selectedCard) 
            DeselectCard();

        selectedCard = c;
        selectedCard.SelectCard();

        if (selectedUnit)
            selectedUnit.UpdateAction(selectedCard);

        c.EnableInput(false, true);
    }

    public virtual void DeselectCard() {
        if (selectedUnit)
            selectedUnit.UpdateAction();
        if (selectedCard) {        
            selectedCard.EnableInput(true);
            selectedCard = null;
        }
    }

    public bool PlayCard() {
        if (currentEnergy >= selectedCard.data.energyCost)
        {
            currentEnergy -= selectedCard.data.energyCost;
            UpdateEnergyDisplay();

            hand.Remove(selectedCard);
            deck.discard.Add(selectedCard);

            selectedCard.gameObject.SetActive(false);
            selectedCard.EnableInput(false);
            
            DeselectCard();
            
            StartCoroutine(UpdateHandDisplay());
            return true;
        }
        else 
        {
            StartCoroutine(EnergyWarning());
            return false;
        }
    }

// Get grid input from player controller, translate it to functionality
    public void GridInput(GridElement input) {
        if (input is Unit t) 
        {
            if (t.owner == Unit.Owner.Player) 
            {
                Debug.Log(t.name);
                if (selectedUnit)
                    Debug.Log(selectedUnit.name);
                if (t == selectedUnit) 
                {
                    if (selectedCard) 
                    {
                        if (selectedCard.data.action == CardData.Action.Defend) 
                        {
                            StartCoroutine(DefendUnit(selectedCard.data.shield));
                        } else
                            DeselectUnit();
                    } else                    
                        DeselectUnit();                 
                } else 
                    SelectUnit(t);
            }
            else if (t.owner == Unit.Owner.Enemy) 
            {
                if (selectedCard && selectedUnit) 
                {
                    if (selectedCard.data.action == CardData.Action.Attack) 
                        StartCoroutine(AttackWithUnit(t.coord));                    
                }
            }
        }
        else if (input is GridSquare sqr) 
        {
// Recurse this function with reference to square contents if found
            GridElement contents = grid.CoordContents(sqr.coord);
            if (contents)
                GridInput(contents);
            else {
                if (selectedCard && selectedUnit) {
                    switch (selectedCard.data.action) {
                        case CardData.Action.Move:
                            if (selectedUnit.validMoveCoords.Find(coord => coord == sqr.coord) != null)
                                StartCoroutine(MoveUnit(sqr.coord));
                        break;
                        case CardData.Action.Attack:

                        break;
                    }
                }
            }            
        }


    }

    protected override void RemoveUnit(GridElement ge)
    {
        base.RemoveUnit(ge);
        if (units.Count <= 0) {
            scenario.Lose();            
        }
    }

    protected virtual IEnumerator EnergyWarning() {
        for (int i = 0; i < 3; i++) {
            energyWarning.SetActive(false);
            yield return new WaitForSecondsRealtime(.2f);
            energyWarning.SetActive(true);
            yield return new WaitForSecondsRealtime(.2f);
        }
    }
}
