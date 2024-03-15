using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NodeVideoPlayer : MonoBehaviour {


    public static IEnumerator FadeVideoPlayerAlpha(UnityEngine.Video.VideoPlayer vp, bool inOut, float fadeSpeed)
        {
            if (vp.renderMode == UnityEngine.Video.VideoRenderMode.CameraNearPlane || vp.renderMode == UnityEngine.Video.VideoRenderMode.CameraFarPlane || vp.renderMode == UnityEngine.Video.VideoRenderMode.RenderTexture)
            {
                RawImage rawImage = null;
                if (vp.renderMode == UnityEngine.Video.VideoRenderMode.RenderTexture) {
                    vp.gameObject.TryGetComponent<RawImage>(out rawImage);
                    if (!rawImage) Debug.LogWarning("No RawImage on the VideoPlayer GameObject found. -> (" + vp.gameObject.name + ")");
                }
 
                float alpha = inOut ? 1f : 0f;
                float fadeEndValue = inOut ? 0f : 1f;
// Fade video out
                if (!inOut) {
                    while (alpha >= fadeEndValue) {
                        alpha -= Time.deltaTime * fadeSpeed;
                        if (rawImage) rawImage.color = new Color(rawImage.color.r, rawImage.color.g, rawImage.color.b, alpha);
                        else vp.targetCameraAlpha = alpha;
                        yield return null;
                    }
                    vp.Stop();
                    if (rawImage) rawImage.enabled = false;
                }
                else {
// Fade video in
                    if (rawImage) rawImage.color = new Color(rawImage.color.r, rawImage.color.g, rawImage.color.b, alpha);
                    else vp.targetCameraAlpha = alpha;
 
                    //Enable the RawImage and start the player
                    if (rawImage) rawImage.enabled = true;
                    vp.Play();
 
                    //Delay - to make sure the Image has the correct Texture
                    yield return new WaitForSeconds(0.1f);
 
                    while (alpha <= fadeEndValue) {
                        alpha += Time.deltaTime * fadeSpeed;
                        if (rawImage) rawImage.color = new Color(rawImage.color.r, rawImage.color.g, rawImage.color.b, alpha);
                        else vp.targetCameraAlpha = alpha;
                        yield return null;
                    }
                }
            }
            else {
                Debug.LogWarning("VideoRenderMode (for alpha) must be RenderTexture, CameraFarPlane or CameraNearPlane. GameObject -> (" + vp.gameObject.name + ")");
            }
        }

}
