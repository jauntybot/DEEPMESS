using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class UnitConditionVFX : MonoBehaviour
{

    Unit unit;

    [SerializeField] GameObject weakenedPrefab, stunnedPrefab;

    List<GameObject> activeStatus = new List<GameObject>();

    public void Init(Unit u) {
        unit = u;
    }

    public void UpdateCondition(Unit.Status condition) {
        
        GameObject s;
        switch (condition) {
            default: break;
            case Unit.Status.Weakened:
                s = Instantiate(weakenedPrefab, this.transform);
                activeStatus.Add(s);
                unit.gfx.Add(s.GetComponent<SpriteRenderer>());
            break;
            case Unit.Status.Stunned:
                s = Instantiate(stunnedPrefab, this.transform);
                activeStatus.Add(s);
                unit.gfx.Add(s.GetComponent<SpriteRenderer>());
            break;
        }

    }

    public void RemoveCondition( Unit.Status condition) {

        switch (condition) {
            default: break;
            case Unit.Status.Weakened:
                for (int i = activeStatus.Count - 1; i >=0; i--) {
                    if (activeStatus[i].GetComponent<Animator>().runtimeAnimatorController == weakenedPrefab.GetComponent<Animator>().runtimeAnimatorController) {
                        GameObject s = activeStatus[i];
                        activeStatus.Remove(s);
                        unit.gfx.Remove(s.GetComponent<SpriteRenderer>());
                        Destroy(s);
                    }
                }
            break;
            case Unit.Status.Stunned:
                for (int i = activeStatus.Count - 1; i >=0; i--) {
                    if (activeStatus[i].GetComponent<Animator>().runtimeAnimatorController == stunnedPrefab.GetComponent<Animator>().runtimeAnimatorController) {
                        GameObject s = activeStatus[i];
                        activeStatus.Remove(s);
                        unit.gfx.Remove(s.GetComponent<SpriteRenderer>());
                        Destroy(s);
                    }
                }
            break;
        }
    }

}
