using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Token : GridElement {

    public enum Owner { Player, Enemy }
    public Owner owner;

// Three possible equations used for calculating adjacent coordinates
    public enum AdjacnecyType { Diamond, Directional, Box }

    [Header("--- Move Vars ---")]
    [SerializeField] protected AdjacnecyType moveAdjacency;
    [SerializeField] protected int moveRange;
    [SerializeField] protected List<Vector2> moveDirections; // Only used for directional adjacency
    public List<Vector2> validMoveCoords;


    [Header("--- Attack Vars ---")]
    [SerializeField] protected AdjacnecyType attackAdjacency;
    [SerializeField] protected int attackRange;

    [SerializeField] protected List<Vector2> attackDirections; // Only used for directional adjacency
    public List<Vector2> validAttackCoords;
    

    [SerializeField] protected int defense;
    [SerializeField] float animDur = 1f;

// Functions that will change depending on the class they're inherited from
#region Inherited Functionality
    public override void UpdateElement(Vector2 c) {
        base.UpdateElement(c);
    }
    public virtual void EnableSelection(bool state) {}
    public virtual void UpdateAction(Card card = null) {}
    public virtual void UpdateValidMovement() {}
    public virtual void UpdateValidAttack() {}
#endregion

// The basics of being a token; movement, HP, attacking
#region Token Functionality
    public IEnumerator JumpToCoord(Vector2 moveTo) 
    {
        coord = moveTo;
        transform.position = Grid.PosFromCoord(coord);

        float timer = 0;
        while (timer < animDur) {
            yield return new WaitForEndOfFrame();


            timer += Time.deltaTime;
        }
    }

    public IEnumerator AttackCoord(Vector2 attackAt) 
    {
        float timer = 0;
        while (timer < animDur) {
            yield return new WaitForEndOfFrame();


            timer += Time.deltaTime;
        }
    }

    public void TakeDamage(int dmg) {
        
    }

#endregion

// Equations used for calculating adjacent coordinates
#region Coordinate Adjacency

// Accessor function, pass all params here and it will use the appropriate equation
    public virtual List<Vector2> GetAdjacent(
        AdjacnecyType adj, // Which equation to use
        Vector2 origin, // Origin coordinate
        int range, // Range of adjacency
        List<Vector2> dirs = null)  // Override to specify directional adjacency
    {
        List<Vector2> _coords = new List<Vector2>();

        switch(adj) {
         case AdjacnecyType.Diamond:
            _coords = DiamondAdjacency(origin, range);
         break;
         case AdjacnecyType.Directional:
            _coords = DirectionalAdjacency(origin, range, dirs);   
         break;
         case AdjacnecyType.Box:
            _coords = BoxAdjacency(origin, range);
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

#endregion

}