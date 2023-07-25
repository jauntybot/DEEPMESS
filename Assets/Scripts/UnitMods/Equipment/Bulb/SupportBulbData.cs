using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Equipment/Bulb/Support")]
public class SupportBulbData : BulbEquipmentData
{

    public enum SupportType { Heal, Surge };
    public SupportType supportType;

    public override List<Vector2> TargetEquipment(GridElement user, int mod = 0)
    {

        List<Vector2> validCoords = EquipmentAdjacency.GetAdjacent(user, range + mod, this);

        if (user is PlayerUnit u) {
            u.ui.ToggleEquipmentButtons();
            u.inRangeCoords = validCoords;
        }

        user.grid.DisplayValidCoords(validCoords, gridColor);
        for (int i = validCoords.Count - 1; i >= 0; i--) {
            if (user.grid.CoordContents(validCoords[i]).Count != 0) {
                foreach(GridElement ge in user.grid.CoordContents(validCoords[i])) {
                    if (ge is not PlayerUnit)
                        validCoords.Remove(validCoords[i]);
                }
            } else 
                validCoords.Remove(validCoords[i]);
        }
        return validCoords;
    }


    public override IEnumerator UseEquipment(GridElement user, GridElement target = null)
    {
        GameObject bulb = Instantiate(bulbPrefab, user.transform.position, Quaternion.identity, user.transform);
        bulb.GetComponent<SpriteRenderer>().sortingOrder = user.grid.SortOrderFromCoord(target.coord);
        Vector3 origin = user.grid.PosFromCoord(user.coord);
        Vector3 dest = user.grid.PosFromCoord(target.coord);
        float h = 0.25f + Vector2.Distance(user.coord, target.coord) / 2;
        float throwDur = 0.25f + animDur * Vector2.Distance(user.coord, target.coord);
        float timer = 0;
        while (timer < throwDur) {
            bulb.transform.position = Util.SampleParabola(origin, dest, h, timer/throwDur);

            yield return new WaitForSecondsRealtime(1/Util.fps);
            timer += Time.deltaTime;    
        }
        
        bulb.GetComponent<Animator>().SetTrigger("Apply");
        
        yield return base.UseEquipment(user, target);
        
        switch (supportType) {
            default:
            case SupportType.Heal:
                target.StartCoroutine(target.TakeDamage(-2));

            break;
            case SupportType.Surge:
                PlayerUnit pu = (PlayerUnit)target;
                pu.moved = false; pu.energyCurrent = pu.energyMax;
            break;
        }
    }
}
