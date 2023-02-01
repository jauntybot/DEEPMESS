using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : GridElement {

    public enum Owner { Player, Enemy }

    [Header("Unit")]
    public Owner owner;

    public CardData attackCard, moveCard;

    public List<Vector2> validMoveCoords;
    public List<Vector2> validAttackCoords;

    [SerializeField] float animDur = 1f;

// Functions that will change depending on the class they're inherited from
#region Inherited Functionality
    public override void UpdateElement(Vector2 c) {
        base.UpdateElement(c);
    }
    public virtual void UpdateAction(int index = 0) {
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
                if (ge is Wall w) {
                    if (tempCoords[i] == w.coord)
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
            transform.position = Vector3.Lerp(transform.position, grid.PosFromCoord(moveTo), timer/animDur);

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

        StartCoroutine(unit.TakeDamage(1));
    }

    public override IEnumerator Defend(int value) 
    {
        float timer = 0;
        while (timer < animDur) {
            yield return null;

            timer += Time.deltaTime;
        }
        
        yield return base.Defend(value);
    }

    
    public virtual IEnumerator CollideOnDescent(Vector2 moveTo) {
        yield return new WaitForSecondsRealtime(1);
        float timer = 0;
        Vector3 bumpUp = transform.position + Vector3.up * 2;
        while (timer<animDur) {
            yield return null;
            transform.position = Vector3.Lerp(transform.position, bumpUp, timer/animDur);
            timer += Time.deltaTime;
        }
        timer = 0;
        StartCoroutine(TakeDamage(1));
        while (timer<animDur) {
            yield return null;
            transform.position = Vector3.Lerp(transform.position, grid.PosFromCoord(moveTo), timer/animDur);

            timer += Time.deltaTime;
        }   

        UpdateElement(moveTo);
    }


#endregion

// Equations used for calculating adjacent coordinates
// Should be refactored into it's own utility class - each instanced unit does not need these
#region Coordinate Adjacency

// Accessor function, pass all params here and it will use the appropriate equation
    public virtual List<Vector2> GetAdjacent(CardData card, Vector2 origin)
    {
        List<Vector2> _coords = new List<Vector2>();

        switch(card.adjacency) {
         case CardData.AdjacencyType.Diamond:
            _coords = DiamondAdjacency(origin, card);
         break;
         case CardData.AdjacencyType.Orthogonal:
            _coords = OrthagonalAdjacency(origin, card);   
         break;
         case CardData.AdjacencyType.Diagonal:
            _coords = DiagonalAdjacency(origin, card);   
         break;
         case CardData.AdjacencyType.Box:
            _coords = BoxAdjacency(origin, card.range);
         break;
        }

        return _coords;
    }

    protected virtual List<Vector2> DiamondAdjacency(Vector2 origin, CardData card) 
    {
        List<Vector2> _coords = new List<Vector2>();
        List<Vector2> e_coords = new List<Vector2>();
        e_coords.Add(origin);

        for (int r = 1; r <= card.range; r++) {
            for (int e = e_coords.Count - 1; e >= 0; e--) {
                Vector2 current = e_coords[e];
                e_coords.Remove(e_coords[e]);

                for (int x = -1; x < 2; x+=2) {
                    Vector2 coord = new Vector2(current.x + x, current.y);
                    if (!_coords.Contains(coord)) {
                        if (grid.CoordContents(coord) is GridElement ge) {
                            switch (card.action) {
                                case CardData.Action.Move:
                                    if (ge is Unit u) {
                                        if (u.owner != owner) break;
                                        else {
                                            e_coords.Add(new Vector2(current.x + x, current.y));
                                            _coords.Add(new Vector2(current.x + x, current.y));
                                        }
                                    }
                                break;
                                case CardData.Action.Attack:
                                    if (ge is Unit u2) {
                                        if (u2.owner == owner) break;
                                        else 
                                            _coords.Add(new Vector2(current.x + x, current.y));                                      
                                    }
                                break;
                            }
                        }
                        else {
                            e_coords.Add(new Vector2(current.x + x, current.y));
                            _coords.Add(new Vector2(current.x + x, current.y));
                        }
                    }
                }
                for (int y = -1; y < 2; y+=2) {    
                    Vector2 coord = new Vector2(current.x, current.y + y);
                    if (!_coords.Contains(coord)) {
                        if (grid.CoordContents(coord) is GridElement ge) {
                            switch (card.action) {
                                case CardData.Action.Move:
                                    if (ge is Unit u) {
                                        if (u.owner != owner) break;
                                        else {
                                            e_coords.Add(new Vector2(current.x, current.y + y));
                                            _coords.Add(new Vector2(current.x, current.y + y));
                                        }
                                    }
                                break;
                                case CardData.Action.Attack:
                                    if (ge is Unit u2) {
                                        if (u2.owner == owner) break;
                                        else 
                                            _coords.Add(new Vector2(current.x, current.y + y));                            
                                    }
                                break;
                            }
                        }
                        else {
                            e_coords.Add(new Vector2(current.x, current.y + y));
                            _coords.Add(new Vector2(current.x, current.y + y));
                        }
                    }
                }
            }            
        }
        _coords = RemoveOffGridCoords(_coords);
        return _coords;
    }

    protected virtual List<Vector2> OrthagonalAdjacency(Vector2 origin, CardData card) 
    {
        List<Vector2> _coords = new List<Vector2>();
        Vector2 dir = Vector2.zero;
        for (int d = 0; d < 4; d++) 
        {
            switch (d) {
                case 0: dir = Vector2.up; break;
                case 1: dir = Vector2.right; break;
                case 2: dir = Vector2.down; break;
                case 3: dir = Vector2.left; break;
            }
            bool blocked = false;
            for (int r = 1; r <= card.range; r++) 
            {
                if (blocked) break;
                Vector2 coord = origin + dir * r;
                if (grid.CoordContents(coord) is GridElement ge) {
                    switch (card.action) {
                        case CardData.Action.Move:
                            if (ge is Unit u) {
                                if (u.owner != owner) blocked = true;
                                else 
                                    _coords.Add(coord);                                       
                            } else
                                blocked = true;
                        break;
                        case CardData.Action.Attack:
                            if (ge is Unit u2) {
                                if (u2.owner == owner) blocked = true;
                                else 
                                    _coords.Add(coord);
                                    blocked = true;                                       
                            } else
                                blocked = true;
                        break;
                    }
                }
                else 
                    _coords.Add(coord);
            }
            _coords = RemoveOffGridCoords(_coords);
        }
        return _coords;
    }

    protected virtual List<Vector2> DiagonalAdjacency(Vector2 origin, CardData card) 
    {
        List<Vector2> _coords = new List<Vector2>();
        Vector2 dir = Vector2.zero;
        for (int d = 0; d < 4; d++) 
        {
            switch (d) {
                case 0: dir = new Vector2(1,1); break;
                case 1: dir = new Vector2(-1,1); break;
                case 2: dir = new Vector2(1,-1); break;
                case 3: dir = new Vector2(-1,-1); break;
            }
            bool blocked = false;
            for (int r = 1; r <= card.range; r++) 
            {
                if (blocked) break;
                Vector2 coord = origin + dir * r;
                if (grid.CoordContents(coord) is GridElement ge) {
                    switch (card.action) {
                        case CardData.Action.Move:
                            if (ge is Unit u) {
                                if (u.owner != owner) blocked = true;
                                else 
                                    _coords.Add(coord);                                       
                            } else
                                blocked = true;
                        break;
                        case CardData.Action.Attack:
                            if (ge is Unit u2) {
                                if (u2.owner == owner) blocked = true;
                                else 
                                    _coords.Add(coord);
                                    blocked = true;                                       
                            } else
                                blocked = true;
                        break;
                    }
                }
                else 
                    _coords.Add(coord);
            }
            _coords = RemoveOffGridCoords(_coords);
        }
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
            if (list[i].x > FloorManager.gridSize - 1 || list[i].x < 0 || list[i].y > FloorManager.gridSize - 1 || list[i].y < 0)
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