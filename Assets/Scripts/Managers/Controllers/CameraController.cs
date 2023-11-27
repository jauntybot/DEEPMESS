using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{

    public static CameraController instance;
    private void Awake() {
        if (CameraController.instance) DestroyImmediate(this);
        CameraController.instance = this;
    }

    [SerializeField] AnimationCurve screenShakeCurve;
    [SerializeField] float screenShakeDur = 1;


    public IEnumerator ScreenShake(float dur, float str = 1) {

        Vector3 startPosition = transform.position;
        float timer = 0;
        while (timer <= dur) {

            timer += Time.deltaTime;
            float strength = screenShakeCurve.Evaluate(timer / dur) * str;
            transform.position = startPosition + (Vector3)Random.insideUnitCircle * (strength + .1f);
            yield return null;
        }
        transform.position = startPosition;

    }
}
