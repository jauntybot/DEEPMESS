using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Token : GridElement {

    public enum Owner { Red, Blue }
    public Owner owner;

    [SerializeField] float animDur = 1f;

    public List<Vector2> validMoveCoords;

    public override void UpdateElement(Vector2 c) {
        base.UpdateElement(c);
    }

    public virtual void EnableSelection(bool state) {}

    public virtual void UpdateValidMoves(Card card = null) {}

    public IEnumerator JumpToCoord(Vector2 moveTo) {
        float timer = 0;
        coord = moveTo;
        transform.position = Grid.PosFromCoord(coord);
        while (timer < animDur) {
            yield return new WaitForEndOfFrame();


            timer += Time.deltaTime;
        }
    }

    public IEnumerator AttackCoord(Vector2 attackAt) {
        float timer = 0;

        while (timer < animDur) {
            yield return new WaitForEndOfFrame();


            timer += Time.deltaTime;
        }
    }

    public void TakeDamage(int dmg) {
        
    }
}