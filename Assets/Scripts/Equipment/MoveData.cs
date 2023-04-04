using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Equipment/Move")]
[System.Serializable]
public class MoveData : EquipmentData
{
    enum AnimType { Lerp, Stepped }
    [SerializeField] AnimType animType;

    public override IEnumerator UseEquipment(GridElement user, GridElement target = null)
    {
        yield return base.UseEquipment(user);
        yield return user.StartCoroutine(MoveToCoord((Unit)user, target.coord));
        
    }

    public IEnumerator MoveToCoord(Unit unit, Vector2 moveTo) 
    {          
// Build frontier dictionary for stepped lerp
        Dictionary<Vector2, Vector2> fromTo = new Dictionary<Vector2, Vector2>();
        if (animType == AnimType.Stepped) 
            fromTo = EquipmentAdjacency.SteppedCoordAdjacency(unit.coord, moveTo, this);
        else if (animType == AnimType.Lerp)
            fromTo.Add(unit.coord, moveTo);

        Vector2 current = unit.coord;
        unit.coord = moveTo;

        AudioManager.PlaySound(AudioAtlas.Sound.moveSlide,moveTo);
// Lerp units position to target
        while (!Vector2.Equals(current, moveTo)) {
            float timer = 0;
            Vector3 toPos = FloorManager.instance.currentFloor.PosFromCoord(fromTo[current]);
// exposed UpdateElement() functionality to selectively update sort order
            if (unit.grid.SortOrderFromCoord(fromTo[current]) > unit.grid.SortOrderFromCoord(current))
                unit.UpdateSortOrder(fromTo[current]);
            current = fromTo[current];
            while (timer < animDur) {
                yield return null;
                unit.transform.position = Vector3.Lerp(unit.transform.position, toPos, timer/animDur);
                timer += Time.deltaTime;
            }
        }        
        unit.UpdateElement(moveTo);
// Check for shared space  
        foreach (GridElement ge in unit.grid.CoordContents(moveTo)) {
            ge.OnSharedSpace(unit);
        }

        yield return new WaitForSecondsRealtime(0.25f);
        if (!unit.targeted) unit.TargetElement(false);
    }

}
