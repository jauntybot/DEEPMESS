using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EquipmentAdjacency {

// Equations used for calculating adjacent coordinates
#region Coordinate Adjacency

// Accessor function, pass all params here and it will use the appropriate equation
    public static List<Vector2> GetAdjacent(Vector2 from, int range, GearData data, List<GridElement> targetLast = null, Grid grid = null, bool offGrid = false) {
        List<Vector2> _coords = new();

        switch(data.adjacency) {
            default:
            case GearData.AdjacencyType.Diamond:
                _coords = DiamondAdjacency(from, range, data, data.filters, targetLast);
            break;
            case GearData.AdjacencyType.Orthogonal:
                _coords = OrthagonalAdjacency(from, range, data.filters, targetLast);
            break;
            case GearData.AdjacencyType.OfType:
                _coords = OfTypeOnBoardAdjacency(data.filters, grid);
            break;
            case GearData.AdjacencyType.OfTypeInRange:
                _coords = DiamondAdjacency(from, range, data, data.filters, targetLast, true);
            break;
            case GearData.AdjacencyType.Box:
                _coords = BoxAdjacency(from, range, targetLast);
            break;
        }
        if (offGrid == false) _coords = RemoveOffGridCoords(_coords);
        
        return _coords;
    }

    public static List<Vector2> DiamondAdjacency(Vector2 from, int range, GearData data = null, List<GridElement> filters = null, List<GridElement> targetLast = null, bool ofType = false) {
        List<Vector2> _coords = new();
        List<Vector2> frontier = new();
        frontier.Add(from);

        for (int r = 1; r <= range; r++) {
            for (int f = frontier.Count - 1; f >= 0; f--) {
                Vector2 current = frontier[f];
                frontier.Remove(frontier[f]);
// X Axis adjacency
                for (int x = -1; x < 2; x+=2) {
                    Vector2 coord = new(current.x + x, current.y);
                    if (coord.x < 0 || coord.x > 7 || coord.y < 0 || coord.y >7) continue;
                    if (!_coords.Contains(coord)) {
                        bool valid = true;
                        if (data is HammerData || data is AttackData) valid = false;
// If there is something already occupying this coord  
                        foreach (GridElement ge in FloorManager.instance.currentFloor.CoordContents(coord)) {
// Valid coord if element is not filtered
                            if (filters == null || filters.Find(f => f.GetType() == ge.GetType()) || filters.Find(f => ge.GetType().IsSubclassOf(f.GetType()))) {
                                valid = false;
                                if (targetLast != null) {
                                    foreach(GridElement target in targetLast) {
                                        if (ge.GetType() == target.GetType()) {
                                            _coords.Add(coord);
                                        }
                                    }
                                }
                            } 
                            if (valid == false) continue;
                        }
// Check if Tile is valid
                        if (filters != null) {
                            foreach (GridElement ge in filters) {
                                if (ge is Tile sqr) {
                                    Tile target = FloorManager.instance.currentFloor.tiles.Find(sqr => sqr.coord == coord);
                                    if (target != null) {
                                        if (target.tileType == sqr.tileType) {
                                            valid = false;
                                        }
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
                    Vector2 coord = new(current.x, current.y + y);
                    if (coord.x < 0 || coord.x > 7 || coord.y < 0 || coord.y >7) continue;
                    if (!_coords.Contains(coord)) {
                        bool valid = true;
                        if (data is HammerData || data is AttackData) valid = false;
// If there is something already occupying this coord                        
                        foreach (GridElement ge in FloorManager.instance.currentFloor.CoordContents(coord)) {
// Valid coord if element is not filtered
                            if (filters == null || filters.Find(f => f.GetType() == ge.GetType()) || filters.Find(f => ge.GetType().IsSubclassOf(f.GetType()))) {
                                valid = false;
                                if (targetLast != null) {
                                    foreach(GridElement target in targetLast) {
                                        if (ge.GetType() == target.GetType()) {
                                            _coords.Add(coord);
                                        }
                                    }
                                }
                            }
                            if (valid == false) continue;
                        }
// Check if Tile is valid
                        if (filters != null) {
                            foreach (GridElement ge in filters) {
                                if (ge is Tile sqr) {
                                    Tile target = FloorManager.instance.currentFloor.tiles.Find(sqr => sqr.coord == coord);
                                    if (target != null) {
                                        if (target.tileType == sqr.tileType) {
                                            valid = false;
                                        }
                                    }
                                }
                            }
                        }
// Coord is empty
                        if (valid) {
                            frontier.Add(coord);
                            _coords.Add(coord);
                        }
                        else if (ofType)
                            frontier.Add(coord);
                        
                    }
                }
            }            
        }

        return _coords;
    }

    public static Dictionary<Vector2, Vector2> SteppedCoordAdjacency(Vector2 from, Vector2 to, GearData data) {
        Dictionary<Vector2, Vector2> _toFrom = new();
        List<Vector2> frontier = new() { from };
        Vector2 current = from;
        
        while (!Vector2.Equals(current, to) && frontier.Count > 0) {
            for (int f = frontier.Count - 1; f >= 0; f--) {
                current = frontier[f];
                frontier.Remove(frontier[f]);
// X Axis adjacency
                for (int x = -1; x < 2; x+=2) {
                    Vector2 coord = new(current.x + x, current.y);
                    if (coord.x < 0 || coord.x > 7) continue;
                    if (!_toFrom.ContainsKey(coord)) {
// If there is something already occupying this coord  
                        bool occupied = false;
                        Tile tile = FloorManager.instance.currentFloor.tiles.Find(t => t.coord == coord);
                        if (data.filters.Find(f => f is Tile t && t.tileType == Tile.TileType.Bile) && tile.tileType == Tile.TileType.Bile) continue;
                        foreach (GridElement ge in FloorManager.instance.currentFloor.CoordContents(coord)) {
                            occupied = true;
// Valid coord if element is not filtered
                            if (data.filters == null || !data.filters.Find(f => f.GetType() == ge.GetType() || !ge.GetType().IsSubclassOf(f.GetType()))) {
                                frontier.Add(coord);
                                _toFrom.Add(coord,current);
                                continue;
                            }
                            if (Vector2.Equals(current, to)) break;
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
                    Vector2 coord = new(current.x, current.y + y);
                    if (coord.y < 0 || coord.y > 7) continue;
                    if (!_toFrom.ContainsKey(coord)) {
// If there is something already occupying this coord                        
                        bool occupied = false;
                        Tile tile = FloorManager.instance.currentFloor.tiles.Find(t => t.coord == coord);
                        if (data.filters.Find(f => f is Tile t && t.tileType == Tile.TileType.Bile) && tile.tileType == Tile.TileType.Bile) continue;
                        foreach (GridElement ge in FloorManager.instance.currentFloor.CoordContents(coord)) {
                            occupied = true;
// Valid coord if element is not filtered
                            if (data.filters == null || !data.filters.Find(f => f.GetType() == ge.GetType() || !ge.GetType().IsSubclassOf(f.GetType()))) {
                                frontier.Add(coord);
                                _toFrom.Add(coord, current);
                                continue;
                            } 
                            if (Vector2.Equals(current, to)) break;
                        }
// Coord is empty
                        if (!occupied) {
                            frontier.Add(coord);
                            _toFrom.Add(coord, current);
                            if (Vector2.Equals(current, to)) break;
                        }
                    }
                }
            }            
        }

        //if (!_toFrom.ContainsKey(to)) return null;

// Reverse dictionary
        current = to;
        Dictionary<Vector2, Vector2> _fromTo = new();
        while (!Vector2.Equals(current, from)) {
            _fromTo.Add(_toFrom[current], current);
            
            current = _toFrom[current];
            if (Vector2.Equals(current, from)) break;
        }

        return _fromTo;
    }

    
    public static Dictionary<Vector2, Vector2> ClosestSteppedCoordAdjacency(Vector2 from, Vector2 to, GearData data) {
        Dictionary<Vector2, Vector2> _toFrom = new();
        List<Vector2> frontier = new() { from };
        Vector2 current = from;
        
        while (!Vector2.Equals(current, to) && frontier.Count > 0) {
            for (int f = frontier.Count - 1; f >= 0; f--) {
                current = frontier[f];
                frontier.Remove(frontier[f]);
// X Axis adjacency
                for (int x = -1; x < 2; x+=2) {
                    Vector2 coord = new(current.x + x, current.y);
                    if (coord.x < 0 || coord.x > 7) continue;
                    if (!_toFrom.ContainsKey(coord)) {
// If there is something already occupying this coord  
                        bool occupied = false;
                        Tile tile = FloorManager.instance.currentFloor.tiles.Find(t => t.coord == coord);
                        if (data.filters.Find(f => f is Tile t && t.tileType == Tile.TileType.Bile) && tile.tileType == Tile.TileType.Bile) continue;
                        foreach (GridElement ge in FloorManager.instance.currentFloor.CoordContents(coord)) {
                            occupied = true;
// Valid coord if element is not filtered
                            if (ge is EnemyUnit) {
                                frontier.Add(coord);
                                _toFrom.Add(coord, current);
                                continue;
                            } else if (data.filters == null || !data.filters.Find(f => f.GetType() == ge.GetType() && !ge.GetType().IsSubclassOf(f.GetType()))) {
                                frontier.Add(coord);
                                _toFrom.Add(coord,current);
                                continue;
                            }
                            if (Vector2.Equals(current, to)) break;
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
                    Vector2 coord = new(current.x, current.y + y);
                    if (coord.y < 0 || coord.y > 7) continue;
                    if (!_toFrom.ContainsKey(coord)) {
// If there is something already occupying this coord         
                        bool occupied = false;
                        Tile tile = FloorManager.instance.currentFloor.tiles.Find(t => t.coord == coord);
                        if (data.filters.Find(f => f is Tile t && t.tileType == Tile.TileType.Bile) && tile.tileType == Tile.TileType.Bile) continue;               
                        foreach (GridElement ge in FloorManager.instance.currentFloor.CoordContents(coord)) {
                            occupied = true;
// Valid coord if element is not filtered
                            if (ge is EnemyUnit) {
                                frontier.Add(coord);
                                _toFrom.Add(coord, current);
                                continue;
                            } else if (data.filters == null || !data.filters.Find(f => f.GetType() == ge.GetType() && !ge.GetType().IsSubclassOf(f.GetType()))) {
                                frontier.Add(coord);
                                _toFrom.Add(coord, current);
                                continue;
                            }
                            if (Vector2.Equals(current, to)) break;
                        }
// Coord is empty
                        if (!occupied) {
                            frontier.Add(coord);
                            _toFrom.Add(coord, current);
                            if (Vector2.Equals(current, to)) break;
                        }
                    }
                }
            }
        }


        if (_toFrom.ContainsKey(to)) {
// Reverse dictionary
            current = to;
            Dictionary<Vector2, Vector2> _fromTo = new();
            while (!Vector2.Equals(current, from)) {
                _fromTo.Add(_toFrom[current], current);
                current = _toFrom[current];
                if (Vector2.Equals(current, from)) break;
            }
            return _fromTo;         
        } else return null;

    }

    public static List<Vector2> OrthagonalAdjacency(Vector2 from, int range, List<GridElement> filters = null, List<GridElement> targetLast = null) 
    {
        List<Vector2> _coords = new();
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
                Vector2 coord = from + dir * r;
                bool occupied = false;
                foreach (GridElement ge in FloorManager.instance.currentFloor.CoordContents(coord)) {
                    occupied = true;
// Valid coord if element is not filtered
                    if (filters == null || (!filters.Find(f => f.GetType() == ge.GetType()) && !filters.Find(f => ge.GetType().IsSubclassOf(f.GetType())))) {
                        _coords.Add(coord);
// Valid coord if element is target, but stops frontier
                    } else if (targetLast != null) {
                        blocked = true;
                        foreach(GridElement target in targetLast) {
                            if (ge.GetType() == target.GetType() || filters.Find(f => ge.GetType().IsSubclassOf(f.GetType()))) {
                                _coords.Add(coord);
                                blocked = true;
                            }
                        }
                    } else blocked = true;
                }
                if (!occupied)
                    _coords.Add(coord);
            }
        }
        return _coords;
    }

    
    public static List<Vector2> OfTypeOnBoardAdjacency(List<GridElement> elements, Grid grid) {
        List<Vector2> _coords = new();
        
        foreach (GridElement type in elements) {
            if (type is Tile) {
                foreach(Tile sqr in grid.tiles) {
                    bool occupied = false;
                    foreach (GridElement ge in grid.CoordContents(sqr.coord)) {
                        occupied = true;
                    }
                    if (!occupied && !_coords.Contains(sqr.coord)) _coords.Add(sqr.coord);
                }
            }
        }
        foreach (GridElement ge in grid.gridElements) {
            foreach (GridElement type in elements) {
                if (ge.GetType() == type.GetType() && !_coords.Contains(ge.coord)) {
                    _coords.Add(ge.coord);
                }
            }
        }
        return _coords;
    } 
    public static List<Vector2> BoxAdjacency(Vector2 from, int range, List<GridElement> targetLast = null) {
        List<Vector2> _coords = new();

        for (int x = -range; x <= range; x++) {
            for (int y = -range; y <= range; y++) {
                Vector3 coord = new Vector3(from.x+x, from.y+y);
                if (coord.x >= 0 && coord.x <= 7 && coord.y >= 0 && coord.y <= 7)
                    _coords.Add(coord);                
            }
        }

        return _coords;
    }

    public static List<Vector2> RemoveOffGridCoords(List<Vector2> list) {
        // check if coords are off the board
        for (int i = list.Count - 1; i >= 0; i--) 
        {
            if (list[i].x > FloorManager.gridSize - 1 || list[i].x < 0 || list[i].y > FloorManager.gridSize - 1 || list[i].y < 0)
                list.Remove(list[i]);
        }
        return list;
    }

/*
    protected static List<Vector2> DiagonalAdjacency(Vector2 origin, GearData e) 
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
                        case GearData.Action.Move:
                            if (ge is Unit u) {
                                if (u.owner != owner) blocked = true;
                                else 
                                    _coords.Add(coord);                                       
                            } else
                                blocked = true;
                        break;
                        case GearData.Action.Attack:
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

*/

#endregion
}
