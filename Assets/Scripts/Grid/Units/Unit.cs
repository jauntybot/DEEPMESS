using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : GridElement {

    public enum Owner { Player, Enemy }

    [Header("Unit")]
    public Owner owner;

    public List<Vector2> validMoveCoords;
    public List<Vector2> validAttackCoords;

    [SerializeField] float animDur = 1f;

// Functions that will change depending on the class they're inherited from
#region Inherited Functionality
    public override void UpdateElement(Vector2 c) {
        base.UpdateElement(c);
    }
    public virtual void UpdateAction(Card card = null) {
// Clear data
        validAttackCoords = null;
        validMoveCoords = null;
        grid.DisableGridHighlight();
    }

// Called when a selected token previews a move action
    public virtual void UpdateValidMovement(CardData card) 
    {
// Get adjacent coords based on action card        
        List<Vector2> tempCoords = GetAdjacent(card, coord);
// loop through complete coord list to remove invalid moves
        for (int i = tempCoords.Count - 1; i >= 0; i--) {
            bool valid = true;

// check if another grid element occupies this space
            foreach (GridElement ge in grid.gridElements) {
                if (tempCoords[i] == ge.coord) valid = false;
            }
// remove from list if invalid
            if (!valid) tempCoords.Remove(tempCoords[i]);
        }
        validMoveCoords = tempCoords;
    }

// Called when a selected token previews an attack action
    public virtual void UpdateValidAttack(CardData card) 
    {
// Get adjacent coords based on action card
        List<Vector2> tempCoords = GetAdjacent(card, coord);
// loop through complete coord list to remove invalid moves
        for (int i = tempCoords.Count - 1; i >= 0; i--) {
            bool valid = true;

// check if a friendly grid element occupies this space
            foreach (GridElement ge in grid.gridElements) {
                if (ge is Unit t) {
                    if (t.owner == owner && tempCoords[i] == ge.coord) 
                        valid = false;
                }
            }

// remove from list if invalid
            if (!valid) tempCoords.Remove(tempCoords[i]);
        }
        validAttackCoords = tempCoords;
    }
#endregion

// The basics of being a unit; movement, HP, attacking
#region Unit Functionality

    public IEnumerator JumpToCoord(Vector2 moveTo) 
    {
        float timer = 0;

        while (timer < animDur) {
            yield return null;
            transform.position = Vector3.Lerp(transform.position, Grid.PosFromCoord(moveTo), timer/animDur);

            timer += Time.deltaTime;
        }
        
        UpdateElement(moveTo);


    }

    public IEnumerator AttackUnit(Unit unit) 
    {
        float timer = 0;
        while (timer < animDur) {
            yield return null;


            timer += Time.deltaTime;
        }

        unit.TakeDamage(1);
    }

    public override IEnumerator Defend(int value) 
    {
        hpDisplay.defenseSlider.gameObject.SetActive(true);
        float timer = 0;
        while (timer < animDur) {
            yield return null;

            timer += Time.deltaTime;
        }
        
        yield return base.Defend(value);

    }

#endregion

// Equations used for calculating adjacent coordinates
#region Coordinate Adjacency

// Accessor function, pass all params here and it will use the appropriate equation
    public virtual List<Vector2> GetAdjacent(CardData card, Vector2 origin)
    {
        List<Vector2> _coords = new List<Vector2>();

        switch(card.adjacency) {
         case CardData.AdjacencyType.Diamond:
            _coords = DiamondAdjacency(origin, card.range);
         break;
         case CardData.AdjacencyType.Orthogonal:
            _coords = OrthagonalAdjacency(origin, card.range);   
         break;
         case CardData.AdjacencyType.Diagonal:
            _coords = DiagonalAdjacency(origin, card.range);   
         break;
         case CardData.AdjacencyType.Box:
            _coords = BoxAdjacency(origin, card.range);
         break;
        }

        return _coords;
    }

    protected virtual List<Vector2> DiamondAdjacency(Vector2 origin, int range) 
    {
        List<Vector2> _coords = new List<Vector2>();

        for (int i = 1; i <= range; i++) 
        {
            _coords.Add(new Vector2 (origin.x + i, origin.y));
            _coords.Add(new Vector2 (origin.x - i, origin.y)); 
            _coords.Add(new Vector2 (origin.x, origin.y + i)); 
            _coords.Add(new Vector2 (origin.x, origin.y - i));
            for (int r = 1; r < i; r++) 
            {
                _coords.Add(new Vector2 (origin.x + r, origin.y + i - r));
                _coords.Add(new Vector2 (origin.x + i - r, origin.y - r)); 
                _coords.Add(new Vector2 (origin.x - r, origin.y - i + r)); 
                _coords.Add(new Vector2 (origin.x - i + r, origin.y + r));
            }
        }
        _coords = RemoveOffGridCoords(_coords);

        return _coords;
    }

    protected virtual List<Vector2> OrthagonalAdjacency(Vector2 origin, int range) 
    {
        List<Vector2> _coords = new List<Vector2>();
        
        for (int i = 0; i <= range; i++) 
        {
            _coords.Add(new Vector2 (origin.x + i, origin.y));
            _coords.Add(new Vector2 (origin.x - i, origin.y));
            _coords.Add(new Vector2 (origin.x, origin.y + i));
            _coords.Add(new Vector2 (origin.x, origin.y - i));
        }
        _coords = RemoveOffGridCoords(_coords);

        return _coords;
    }

    protected virtual List<Vector2> DiagonalAdjacency(Vector2 origin, int range) 
    {
        List<Vector2> _coords = new List<Vector2>();
        
        for (int i = 0; i <= range; i++) 
        {
            _coords.Add(new Vector2 (origin.x + i, origin.y + i));
            _coords.Add(new Vector2 (origin.x - i, origin.y + i));
            _coords.Add(new Vector2 (origin.x + i, origin.y - i));
            _coords.Add(new Vector2 (origin.x - i, origin.y - i));
        }
        _coords = RemoveOffGridCoords(_coords);

        return _coords;
    }

    protected virtual List<Vector2> BoxAdjacency(Vector2 origin, int range) {
        List<Vector2> _coords = new List<Vector2>();

        for (int i = 1; i <= range; i++) 
        {
            _coords.Add(new Vector2(origin.x + i, origin.y));
            _coords.Add(new Vector2(origin.x - i, origin.y));
            _coords.Add(new Vector2(origin.x, origin.y + i));
            _coords.Add(new Vector2(origin.x, origin.y - i));
            _coords.Add(new Vector2(origin.x + i, origin.y + i));
            _coords.Add(new Vector2(origin.x - i, origin.y + i));
            _coords.Add(new Vector2(origin.x + i, origin.y - i));
            _coords.Add(new Vector2(origin.x - i, origin.y - i));        
        }
        _coords = RemoveOffGridCoords(_coords);

        return _coords;
    }

    protected virtual List<Vector2> RemoveOffGridCoords(List<Vector2> list) {
        // check if coords are off the board
        for (int i = list.Count - 1; i >= 0; i--) 
        {
            if (list[i].x > Grid.gridSize - 1 || list[i].x < 0 || list[i].y > Grid.gridSize - 1 || list[i].y < 0)
                list.Remove(list[i]);
        }
        return list;
    }

    /*
    protected virtual List<Vector2> DirectionalAdjacency(Vector2 origin, int range, List<Vector2> dirs) 
    {    
        List<Vector2> _coords = new List<Vector2>();

        foreach (Vector2 dir in dirs) 
        {
            for (int i = 1; i <= range; i++) 
            {
                if (dir.magnitude > 1)  _coords.Add(origin + dir * (i - 1));
                else _coords.Add(origin + dir * i);
            }
        }
        _coords = RemoveOffGridCoords(_coords);

        return _coords;
    }
*/

#endregion

}