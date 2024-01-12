using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HPPips : MonoBehaviour {

    public Unit unit;
    [SerializeField] protected Transform hpPips, emptyHpPips;
    [SerializeField] protected GameObject hpPipPrefab, emptyPipPrefab;

    public void Init(Unit u) {
        unit = u;
        InstantiateMaxPips();
    }

    public virtual void InstantiateMaxPips() {
        for (int i = emptyHpPips.transform.childCount - 1; i >= 0; i--)
            Destroy(emptyHpPips.transform.GetChild(i).gameObject);
        for (int i = hpPips.transform.childCount - 1; i >= 0; i--)
            Destroy(hpPips.transform.GetChild(i).gameObject);

        SizePipContainer(hpPips.transform.GetComponent<RectTransform>());
        for (int i = unit.hpMax - 1; i >= 0; i--) {
            Instantiate(emptyPipPrefab, emptyHpPips.transform);
            Instantiate(hpPipPrefab, hpPips.transform);
            RectTransform rect = hpPips.GetComponent<RectTransform>();
        }
        hpPips.gameObject.SetActive(true);
    }

    protected virtual void SizePipContainer(RectTransform rect) {
        rect.anchorMin = new Vector2(0.5f, 0.5f); rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        Vector3 delta = new Vector2((float)(14 * unit.hpMax + 2 * (unit.hpMax - 1)), 21);
        rect.sizeDelta = (delta);
    }

    public virtual void UpdatePips(int value) {
        for (int i = 0; i <= unit.hpMax - 1; i++) 
            hpPips.transform.GetChild(i).gameObject.SetActive(i < value);
    }

}
