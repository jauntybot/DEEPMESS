using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HPPips : MonoBehaviour {

    public Unit unit;
    [SerializeField] protected Transform hpPips, emptyHpPips;
    [SerializeField] protected GameObject hpPipPrefab, emptyPipPrefab;

    public void Init(Unit u) {
        unit = u;
        unit.ElementDamaged += UpdatePips;
        unit.ElementDestroyed += Unsub;
        UpdatePips();
    }

    public virtual void UpdatePips(GridElement ge = null) {
// Instiantate correct amount of hp pips
        if (emptyHpPips.childCount != unit.hpMax || hpPips.childCount != unit.hpMax) {
            for (int i = emptyHpPips.transform.childCount - 1; i >= 0; i--)
                DestroyImmediate(emptyHpPips.transform.GetChild(i).gameObject);
            for (int i = hpPips.transform.childCount - 1; i >= 0; i--)
                DestroyImmediate(hpPips.transform.GetChild(i).gameObject);

            for (int i = unit.hpMax - 1; i >= 0; i--) {
                Instantiate(emptyPipPrefab, emptyHpPips.transform);
                Instantiate(hpPipPrefab, hpPips.transform);
            }
            RectTransform rect = hpPips.GetComponent<RectTransform>();
            SizePipContainer(rect);
        }
// Set active pips
        SetToCurrentHP();
    }

    protected virtual void SizePipContainer(RectTransform rect) {
        rect.anchorMin = new Vector2(0.5f, 0.5f); rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        Vector3 delta = new Vector2((float)(14 * unit.hpMax + 2 * (unit.hpMax - 1)), 21);
        rect.sizeDelta = delta;
    }

    public virtual void SetToCurrentHP() {
        for (int i = 0; i <= hpPips.childCount - 1; i++) {
            hpPips.transform.GetChild(i).gameObject.SetActive(i < unit.hpCurrent);
        }
    }

    void Unsub(GridElement ge) {
        ge.ElementDamaged -= UpdatePips;
        ge.ElementDestroyed -= Unsub;
    }

}
