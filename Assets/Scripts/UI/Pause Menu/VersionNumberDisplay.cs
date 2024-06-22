using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class VersionNumberDisplay : MonoBehaviour {

    void Awake() {
        if (TryGetComponent(out TMP_Text tmp))
            tmp.text = "v" + Application.version + "_DEMO";
    }

}
