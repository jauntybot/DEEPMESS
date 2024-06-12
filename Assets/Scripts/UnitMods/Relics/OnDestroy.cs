using System.Collections;
using System.Collections.Generic;
using Relics;
using UnityEngine;

namespace Relics {

    [CreateAssetMenu(menuName = "Relics/On Destroy")]
    public class OnDestroy : RelicData {

        enum RelicType { FlaminZeetos };
        [SerializeField] RelicType relicType;

        [SerializeField] SFX sfx;
        [SerializeField] GameObject prefab;
        

        public override void Init() {
            ObjectiveEventManager.AddListener<GridElementDestroyedEvent>(OnElementDestroyed);
            base.Init();
        }

        protected virtual void OnElementDestroyed(GridElementDestroyedEvent evt) {
            switch (relicType) {
                default: break;
                case RelicType.FlaminZeetos:
                    if (evt.element is Unit u && u.manager is PlayerManager) {
                        u.StartCoroutine(Detonate(u, 1));
                    }
                break;
            }
        }


        public virtual IEnumerator Detonate(Unit u, int dmg) {
            u.StartCoroutine(ExplosionVFX(u));
            u.PlaySound(sfx);
            //u.gfx[0].enabled = false;
// Apply damage to units in AOE
            List<Vector2> aoe = EquipmentAdjacency.BoxAdjacency(u.coord, 1);
            u.grid.DisplayValidCoords(aoe, 2);
            yield return new WaitForSecondsRealtime(0.5f);
            List<Coroutine> affectedCo = new();
            u.grid.DisableGridHighlight();
            foreach (Vector2 coord in aoe) {
                if (u.grid.CoordContents(coord).Count > 0) {
                    foreach (GridElement ge in u.grid.CoordContents(coord)) {
                        if ((ge is Unit || ge is Wall) && ge != u) 
                            affectedCo.Add(ge.StartCoroutine(ge.TakeDamage(dmg, Unit.DamageType.Explosion, u)));                        
                    }
                }
            }
            
            for (int i = affectedCo.Count - 1; i >= 0; i--) {
                if (affectedCo[i] != null) {
                    yield return affectedCo[i];
                }
                else
                    affectedCo.RemoveAt(i);
            }
            yield return new WaitForSecondsRealtime(0.15f);
        }


        IEnumerator ExplosionVFX(Unit u) {
// Explosion VFX
            GameObject go = Instantiate(prefab, u.grid.PosFromCoord(u.coord), Quaternion.identity);
            go.GetComponentInChildren<SpriteRenderer>().sortingOrder = u.grid.SortOrderFromCoord(u.coord);
            float t = 0;
            while (t <= 0.125f) { t += Time.deltaTime; yield return null; }
            List<Vector2> secondWave = new();
            for (int x = -1; x <= 1; x++) {
                for (int y = -1; y <= 1; y++) {
                    Vector2 c = u.coord + new Vector2(x,y);
                    if (c == u.coord || c.x < 0 || c.x > 7 || c.y < 0 || c.y > 7) continue;
                    if (x != 0 && y != 0) {
                        secondWave.Add(c);
                        continue;
                    }
                    GameObject g = Instantiate(prefab, u.grid.PosFromCoord(c), Quaternion.identity);
                    g.GetComponentInChildren<SpriteRenderer>().sortingOrder = u.grid.SortOrderFromCoord(c);
                }
            }
            t = 0;
            while (t <= 0.125f) { t += Time.deltaTime; yield return null; }
            foreach (Vector2 c in secondWave) {
                GameObject g = Instantiate(prefab, u.grid.PosFromCoord(c), Quaternion.identity);
                g.GetComponentInChildren<SpriteRenderer>().sortingOrder = u.grid.SortOrderFromCoord(c);
            }
        }
        public override void UnsubRelic() {
            base.UnsubRelic();
            ObjectiveEventManager.RemoveListener<GridElementDestroyedEvent>(OnElementDestroyed);
        }

    }

}
