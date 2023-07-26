using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BulbEquipmentData : EquipmentData
{

    [SerializeField] protected GameObject bulbPrefab;

    public override void EquipEquipment(GridElement user)
    {
        base.EquipEquipment(user);
        PlayerUnit pu = (PlayerUnit)user;
        pu.ui.UpdateEquipmentButtons();
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
        PlayerUnit pu = (PlayerUnit)user;

        pu.equipment.Remove(this);
        pu.ui.UpdateEquipmentButtons();
    }

}
