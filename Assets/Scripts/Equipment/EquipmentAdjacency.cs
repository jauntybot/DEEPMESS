using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentAdjacency : MonoBehaviour
{

// Equations used for calculating adjacent coordinates
// Should be refactored into it's own utility class - each instanced unit does not need these
#region Coordinate Adjacency

// Accessor function, pass all params here and it will use the appropriate equation
    public static List<Vector2> GetAdjacent(GridElement from, EquipmentData data, List<GridElement> targetLast = null)
    {
        List<Vector2> _coords = new List<Vector2>();

        switch(data.adjacency) {
        case EquipmentData.AdjacencyType.Diamond:
            _coords = DiamondAdjacency(from, data, targetLast);
        break;
        case EquipmentData.AdjacencyType.Orthogonal:
            _coords = OrthagonalAdjacency(from, data, targetLast);
        break;
        case EquipmentData.AdjacencyType.OfType:
            _coords = OfTypeOnBoardAdjacency(from, data.filters[0], from.coord);
        break;
        
        }

        return _coords;
    }

    protected static List<Vector2> DiamondAdjacency(GridElement from, EquipmentData data, List<GridElement> targetLast) 
    {
        List<Vector2> _coords = new List<Vector2>();
        List<Vector2> frontier = new List<Vector2>();
        frontier.Add(from.coord);

        for (int r = 1; r <= data.range; r++) {
            for (int f = frontier.Count - 1; f >= 0; f--) {
                Vector2 current = frontier[f];
                frontier.Remove(frontier[f]);
// X Axis adjacency
                for (int x = -1; x < 2; x+=2) {
                    Vector2 coord = new Vector2(current.x + x, current.y);
                    if (!_coords.Contains(coord)) {
// If there is something already occupying this coord
                        if (FloorManager.instance.currentFloor.CoordContents(coord) is GridElement ge) {
// Valid coord if element is not filtered
                            if (!data.filters.Find(f => f.GetType() == ge.GetType())
                            || data.filters == null) {
                                frontier.Add(coord);
                                _coords.Add(coord);
                            } else if (targetLast != null) {
                                foreach(GridElement target in targetLast) {
                                    if (ge.GetType() == target.GetType()) {
                                        _coords.Add(coord);
                                    }
                                }
                            }
                        }
// Coord is empty
                        else {
                            frontier.Add(coord);
                            _coords.Add(coord);
                        }
                    }
                }
// Y Axis adjacency
                for (int y = -1; y < 2; y+=2) {    
                    Vector2 coord = new Vector2(current.x, current.y + y);
                    if (!_coords.Contains(coord)) {
// If there is something already occupying this coord                        
                        if (FloorManager.instance.currentFloor.CoordContents(coord) is GridElement ge) {
// Valid coord if element is not filtered
                            if (!data.filters.Find(f => f.GetType() == ge.GetType())
                            || data.filters == null) {
                                frontier.Add(coord);
                                _coords.Add(coord);
// Valid coord if element is target, but stops frontier
                            } else if (targetLast != null) {
                                foreach(GridElement target in targetLast) {
                                    if (ge.GetType() == target.GetType()) {
                                        _coords.Add(coord);
                                    }
                                }
                            }
                        }
// Coord is empty
                        else {
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



    protected static List<Vector2> OrthagonalAdjacency(GridElement from, EquipmentData data, List<GridElement> targetLast) 
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
            for (int r = 1; r <= data.range; r++) 
            {
                if (blocked) break;
                Vector2 coord = from.coord + dir * r;
                if (FloorManager.instance.currentFloor.CoordContents(coord) is GridElement ge) 
                {
// Valid coord if element is not filtered
                    if (!data.filters.Find(f => f.GetType() == ge.GetType())
                    || data.filters == null) {
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
                else 
                    _coords.Add(coord);
            }
            _coords = RemoveOffGridCoords(_coords);
        }
        return _coords;
    }

    
        protected static List<Vector2> OfTypeOnBoardAdjacency(GridElement from, GridElement element, Vector2 origin) {
        List<Vector2> _coords = new List<Vector2>();

        foreach (GridElement ge in from.grid.gridElements) {
            if (ge.GetType() == element.GetType() && ge.coord != origin) {
                _coords.Add(ge.coord);
            }
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
