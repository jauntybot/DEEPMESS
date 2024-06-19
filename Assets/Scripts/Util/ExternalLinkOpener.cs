using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExternalLinkOpener : MonoBehaviour {

    [SerializeField] string link;

    public void OpenLink() {
        Application.OpenURL(link);
    }

}
