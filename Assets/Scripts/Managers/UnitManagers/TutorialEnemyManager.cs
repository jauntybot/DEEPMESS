using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialEnemyManager : EnemyManager
{

    public override IEnumerator TakeTurn(bool scatter) {
        yield return null;
    }

    
    public override void DescentTriggerCheck(GridElement ge = null) {
        if (scenario.currentEnemy == this && scenario.currentTurn != ScenarioManager.Turn.Descent) {
            if (units.Count <= 0) {
                EndTurnEarly();
                scenario.player.TriggerDescent();
            }
        }
    }

    public IEnumerator MoveInOrder(List<Vector2> moves, List<Vector2> attacks = null) {
        for (int i = 0; i < moves.Count; i++) {
            if (!units[i].moved) {
                units[i].UpdateAction(units[i].equipment[0], units[i].moveMod);
                SelectUnit(units[i]);
                currentGrid.DisplayValidCoords(units[i].validActionCoords, units[i].selectedEquipment.gridColor);
                yield return new WaitForSecondsRealtime(0.5f);
                Coroutine co = StartCoroutine(units[i].selectedEquipment.UseEquipment(units[i], currentGrid.sqrs.Find(sqr => sqr.coord == moves[i])));
                currentGrid.UpdateSelectedCursor(false, Vector2.one * -32);
                currentGrid.DisableGridHighlight();
                yield return co;
                DeselectUnit();
            }
            if (attacks != null) {
                units[i].UpdateAction(units[i].equipment[1]);
                SelectUnit(units[i]);
                GridElement target = null;
                foreach (GridElement ge in selectedUnit.grid.CoordContents(attacks[i]))
                    target = ge;
                currentGrid.DisplayValidCoords(units[i].validActionCoords, units[i].selectedEquipment.gridColor);
                yield return new WaitForSecondsRealtime(0.5f);
                Coroutine co = StartCoroutine(units[i].selectedEquipment.UseEquipment(units[i], target));
                currentGrid.UpdateSelectedCursor(false, Vector2.one * -32);
                currentGrid.DisableGridHighlight();
                yield return co;
                yield return new WaitForSecondsRealtime(0.125f);
                DeselectUnit();
            }
        }
        
    }
}


