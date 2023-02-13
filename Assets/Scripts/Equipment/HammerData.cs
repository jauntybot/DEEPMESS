using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Equipment/Hammer")]
[System.Serializable]
public class HammerData : EquipmentData
{

    public GameObject hammerPrefab;
    public GameObject hammer;
    public enum Action { Lob, Strike };
    public Action action;

    public override List<Vector2> TargetEquipment(GridElement user) {
        List<Vector2> validCoords = EquipmentAdjacency.GetAdjacent(user.coord, this);

        return validCoords;
    }

    public override IEnumerator UseEquipment(GridElement user, GridElement target = null)
    {
        yield return base.UseEquipment(user, target);
        switch (action) {
            case Action.Lob:
                yield return user.StartCoroutine(LobHammer(user, target));
            break;
            case Action.Strike:

            break;
        }
    }

    public override void EquipEquipment(GridElement user)
    {
        base.EquipEquipment(user);
        hammer = Instantiate(hammerPrefab, user.transform);
        hammer.transform.position = user.grid.PosFromCoord(user.coord);
    }

    public IEnumerator LobHammer(GridElement ge, GridElement passTo) {

        float timer = 0;

        while (timer < animDur) {

            yield return null;

            timer += Time.deltaTime;
        }

    }

}
