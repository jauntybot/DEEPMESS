using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentAdjacency : MonoBehaviour
{

// Equations used for calculating adjacent coordinates
// Should be refactored into it's own utility class - each instanced unit does not need these
#region Coordinate Adjacency

// Accessor function, pass all params here and it will use the appropriate equation
    public static List<Vector2> GetAdjacent(GridElement from, int range, EquipmentData data, List<GridElement> targetLast = null)
    {
        List<Vector2> _coords = new List<Vector2>();

        switch(data.adjacency) {
        case EquipmentData.AdjacencyType.Diamond:
            _coords = DiamondAdjacency(from, range, data, targetLast);
        break;
        case EquipmentData.AdjacencyType.Orthogonal:
            _coords = OrthagonalAdjacency(from, range, data.filters, targetLast);
        break;
        case EquipmentData.AdjacencyType.OfType:
            _coords = OfTypeOnBoardAdjacency(from, data.filters, from.coord);
        break;
        case EquipmentData.AdjacencyType.OfTypeInRange:
            _coords = DiamondAdjacency(from, range, data, targetLast, true);
        break;
        
        }

        return _coords;
    }

    protected static List<Vector2> DiamondAdjacency(GridElement from, int range, EquipmentData data, List<GridElement> targetLast, bool ofType = false) 
    {
        List<Vector2> _coords = new List<Vector2>();
        List<Vector2> frontier = new List<Vector2>();
        frontier.Add(from.coord);

        for (int r = 1; r <= range; r++) {
            for (int f = frontier.Count - 1; f >= 0; f--) {
                Vector2 current = frontier[f];
                frontier.Remove(frontier[f]);
// X Axis adjacency
                for (int x = -1; x < 2; x+=2) {
                    Vector2 coord = new Vector2(current.x + x, current.y);
                    if (!_coords.Contains(coord)) {
                        bool valid = true;
                        if (data is HammerData || data is AttackData) valid = false;
// If there is something already occupying this coord  
                        foreach (GridElement ge in FloorManager.instance.currentFloor.CoordContents(coord)) {
                            valid = false;
// Valid coord if element is not filtered
                            if (!data.filters.Find(f => f.GetType() == ge.GetType())
                            || data.filters == null) {
                                valid = true;
                            } 
                            if (targetLast != null) {
                                foreach(GridElement target in targetLast) {
                                    if (ge.GetType() == target.GetType()) {
                                        _coords.Add(coord);
                                    }
                                }
                            }
                        }
// Check if GridSquare is valid
                        foreach (GridElement ge in data.filters) {
                            if (ge is GridSquare sqr) {
                                GridSquare target = FloorManager.instance.currentFloor.sqrs.Find(sqr => sqr.coord == coord);
                                if (target != null) {
                                    if (target.tileType == sqr.tileType) {
                                        valid = false;
                                        Debug.Log(target.tileType + " @" + target.coord);
                                    }
                                }
                            }
                        }
// Coord is valid
                        if (valid) {
                            frontier.Add(coord);
                            _coords.Add(coord);
                        }
                        else if (ofType)
                            frontier.Add(coord);
                    }
                }
// Y Axis adjacency
                for (int y = -1; y < 2; y+=2) {    
                    Vector2 coord = new Vector2(current.x, current.y + y);
                    if (!_coords.Contains(coord)) {
// If there is something already occupying this coord                        
                        bool valid = true;
                        foreach (GridElement ge in FloorManager.instance.currentFloor.CoordContents(coord)) {
                            valid = false;
// Valid coord if element is not filtered
                            if (!data.filters.Find(f => f.GetType() == ge.GetType())
                            || data.filters == null) {
                                valid = true;
// Valid coord if element is target, but stops frontier
                            } else if (targetLast != null) {
                                foreach(GridElement target in targetLast) {
                                    if (ge.GetType() == target.GetType()) {
                                        _coords.Add(coord);
                                    }
                                }
                            }
                        }
// Check if GridSquare is valid
                        foreach (GridElement ge in data.filters) {
                            if (ge is GridSquare sqr) {
                                GridSquare target = FloorManager.instance.currentFloor.sqrs.Find(sqr => sqr.coord == coord);
                                if (target != null) {
                                    if (target.tileType == sqr.tileType) {
                                        valid = false;
                                        Debug.Log(target.tileType + " @" + target.coord);
                                    }
                                }
                            }
                        }
// Coord is empty
                        if (valid) {
                            frontier.Add(coord);
                            _coords.Add(coord);
                        }
                    }
                }
            }            
        }
        _coords = RemoveOffGridCoords(_coords);
        return _coords;
    }

    public static Dictionary<Vector2, Vector2> SteppedCoordAdjacency(Vector2 from, Vector2 to, EquipmentData data) {
        Dictionary<Vector2, Vector2> _toFrom = new Dictionary<Vector2, Vector2>();
        List<Vector2> frontier = new List<Vector2>();
        frontier.Add(from);
        Vector2 current = from;
        int traveled = Mathf.RoundToInt(Mathf.Abs(to.x - from.x) + Mathf.Abs(to.y - from.y));

        while (!Vector2.Equals(current, to)) {
            for (int f = frontier.Count - 1; f >= 0; f--) {
                current = frontier[f];
                frontier.Remove(frontier[f]);
// X Axis adjacency
                for (int x = -1; x < 2; x+=2) {
                    Vector2 coord = new Vector2(current.x + x, current.y);
                    if (!_toFrom.ContainsKey(coord)) {
// If there is something already occupying this coord  
                        bool occupied = false;
                        foreach (GridElement ge in FloorManager.instance.currentFloor.CoordContents(coord)) {
                            occupied = true;
// Valid coord if element is not filtered
                            if (!data.filters.Find(f => f.GetType() == ge.GetType())
                            || data.filters == null) {
                                frontier.Add(coord);
                                _toFrom.Add(coord,current);
                                if (Vector2.Equals(current, to)) break;
                            }
                        }
// Coord is empty
                        if (!occupied) {
                            frontier.Add(coord);
                            _toFrom.Add(coord, current);
                            if (Vector2.Equals(current, to)) break;
                        }
                    }
                }
// Y Axis adjacency
                for (int y = -1; y < 2; y+=2) {    
                    Vector2 coord = new Vector2(current.x, current.y + y);
                    if (!_toFrom.ContainsKey(coord)) {
// If there is something already occupying this coord                        
                        bool occupied = false;
                        foreach (GridElement ge in FloorManager.instance.currentFloor.CoordContents(coord)) {
                            occupied = true;
// Valid coord if element is not filtered
                            if (!data.filters.Find(f => f.GetType() == ge.GetType())
                            || data.filters == null) {
                                frontier.Add(coord);
                                _toFrom.Add(coord, current);
                                if (Vector2.Equals(current, to)) break;
// Valid coord if element is target, but stops frontier
                            }
                        }
// Coord is empty
                        if (!occupied) {
                            frontier.Add(coord);
                            _toFrom.Add(coord, current);
                            if (Vector2.Equals(current, to)) break;
                        }
                    }
                }
                if (Vector2.Equals(current, to)) break;
            }            
        }
        Dictionary<Vector2, Vector2> _fromTo = new Dictionary<Vector2, Vector2>();
        current = to;
        while (!Vector2.Equals(current, from)) {
            _fromTo.Add(_toFrom[current], current);
            current = _toFrom[current];
            if (Vector2.Equals(current, from)) break;
        }
        return _fromTo;
    }

    public static List<Vector2> OrthagonalAdjacency(GridElement from, int range, List<GridElement> filters, List<GridElement> targetLast) 
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
            for (int r = 1; r <= range; r++) 
            {
                if (blocked) break;
                Vector2 coord = from.coord + dir * r;
                bool occupied = false;
                foreach (GridElement ge in FloorManager.instance.currentFloor.CoordContents(coord)) {
                    occupied = true;
// Valid coord if element is not filtered
                    if (!filters.Find(f => f.GetType() == ge.GetType())
                    || filters == null) {
                        _coords.Add(coord);
// Valid coord if element is target, but stops frontier
                    } else if (targetLast != null) {
                        blocked = true;
                        foreach(GridElement target in targetLast) {
                            if (ge.GetType() == target.GetType()) {
                                _coords.Add(coord);
                                blocked = true;
                            }
                        }
                    } else blocked = true;
                }
                if (!occupied)
                    _coords.Add(coord);
            }
            _coords = RemoveOffGridCoords(_coords);
        }
        return _coords;
    }

    
        public static List<Vector2> OfTypeOnBoardAdjacency(GridElement from, List<GridElement> elements, Vector2 origin) {
        List<Vector2> _coords = new List<Vector2>();
        
        foreach (GridElement type in elements) {
            if (type is GridSquare) {
                foreach(GridSquare sqr in from.grid.sqrs) {
                    bool occupied = false;
                    foreach (GridElement ge in from.grid.CoordContents(sqr.coord)) {
                        if (ge is not GroundElement) occupied = true;
                    }
                    if (!occupied && !_coords.Contains(sqr.coord)) _coords.Add(sqr.coord);
                }
            }
        }
        foreach (GridElement ge in from.grid.gridElements) {
            foreach (GridElement type in elements) {
                if (ge.GetType() == type.GetType() && !_coords.Contains(ge.coord)) {
                    _coords.Add(ge.coord);
                }
            }
        }
        foreach(Vector2 coord in _coords) {

            Debug.Log(coord);
        }
        return _coords;
    } 

    protected static List<Vector2> RemoveOffGridCoords(List<Vector2> list) {
        // check if coords are off the board
        for (int i = list.Count - 1; i >= 0; i--) 
        {
            if (list[i].x > FloorManager.gridSize - 1 || list[i].x < 0 || list[i].y > FloorManager.gridSize - 1 || list[i].y < 0)
                list.Remove(list[i]);
        }
        return list;
    }
/*
    protected static List<Vector2> DiagonalAdjacency(Vector2 origin, EquipmentData e) 
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
            for (int r = 1; r <= e.range; r++) 
            {
                if (blocked) break;
                Vector2 coord = origin + dir * r;
                if (FloorManager.instance.currentFloor.CoordContents(coord) is GridElement ge) {
                    switch (e.action) {
                        case EquipmentData.Action.Move:
                            if (ge is Unit u) {
                                if (u.owner != owner) blocked = true;
                                else 
                                    _coords.Add(coord);                                       
                            } else
                                blocked = true;
                        break;
                        case EquipmentData.Action.Attack:
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

    protected static List<Vector2> BoxAdjacency(Vector2 origin, int range) {
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
*/

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
