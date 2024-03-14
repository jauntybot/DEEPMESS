using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Relics {

    [CreateAssetMenu(menuName = "Relics/On Damage")]
    public class OnDamageRelic : RelicData {
        
        enum RelicType { SelfDestructButtons, IronDeficiency, SpringedNeck };
        [SerializeField] RelicType relicType;
        

        public override void Init() {
            ObjectiveEventManager.AddListener<GridElementDamagedEvent>(OnElementDamaged);
            base.Init();
        }

        protected virtual void OnElementDamaged(GridElementDamagedEvent evt) {
            switch (relicType) {
                default:
                case RelicType.SelfDestructButtons:
                    if (evt.element is Unit u1 && u1.manager is PlayerManager && u1.hpCurrent - evt.dmg <= 0) {

                    }
                break;
                case RelicType.IronDeficiency:
                    bool a = false, b = false, c = false, d = false;
                    if (evt.element is Unit u3) {
                        a = true;
                        if (u3.shield) b = true;
                        if (evt.damageType is GridElement.DamageType.Fall) c = true;
                        if (evt.source is Wall) d = true;
                    }
                    Debug.Log(a + ", " + b + ", " + c + ", " + d);

                    if (evt.element is Unit u2 && !u2.shield && evt.damageType is GridElement.DamageType.Fall && evt.source is Wall) {
                        Debug.Log("apply condition");
                        u2.ApplyCondition(Unit.Status.Stunned);
                    }
                break;
                case RelicType.SpringedNeck:
                    if (evt.sourceEquip is HammerData && 
                    evt.element is EnemyUnit && evt.element is not EnemyStaticUnit && evt.element is not BossUnit)
                        evt.element.StartCoroutine(PushUnit(evt.element, (evt.element.coord - evt.source.coord).normalized));
                break;
            }
        }

        IEnumerator PushUnit(GridElement pushed, Vector2 dir) {
            Vector3 startPos = pushed.transform.position;
            Vector2 toCoord = pushed.coord + dir;
            if (toCoord.x >= 0 && toCoord.x <= 7 && toCoord.y >= 0 && toCoord.y <= 7 && pushed.grid.CoordContents(toCoord).Count == 0) {
                float timer = 0;
                while (timer < 0.2f) {
                    pushed.transform.position = Vector3.Lerp(startPos, pushed.grid.PosFromCoord(toCoord), timer/0.2f);
                    yield return null;
                    timer += Time.deltaTime;
                }
                pushed.UpdateElement(toCoord);
            } else yield return null;
        }

        public override void UnsubRelic() {
            base.UnsubRelic();
            ObjectiveEventManager.RemoveListener<GridElementDamagedEvent>(OnElementDamaged);
        }
    }
}
