using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(SpriteRenderer))]
public class ParallaxImageScroll : MonoBehaviour
{

    SpriteRenderer _image;
    [SerializeField] Vector2 scrollSpeed;
    public GameObject slimeHub;
    void Start() {
        _image = GetComponent<SpriteRenderer>();
        //_image.material = new Material(_image.material);
    }

    public void ScrollParallax(float pos) {
        Vector2 current = _image.material.GetVector("_Offset");
        current += scrollSpeed * pos;
        if (current.y > 1 || current.y < -1 ) 
            current = Vector2.zero;
        _image.material.SetVector("_Offset", current);
    }

    public void ToggleSlimeHub(bool state) {
        slimeHub.SetActive(state);
    }
}
