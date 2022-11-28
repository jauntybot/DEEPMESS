using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Simple coroutine class for offsetting y pos
[System.Serializable]
public class OffsetOnHover : MonoBehaviour {
//Variables for offset calculation
	
	[SerializeField] SpriteRenderer[] spriteRenderers;
	[SerializeField] float yOffset = 0.25f;
	[SerializeField] float duration = 0.125f;

	[SerializeField] float sinAmplitude, sinFrequency;

	public bool active;
	[SerializeField] bool raising, raised, lowering;

	float yOrigin, scaleOrigin;



	public void UpdateOrigins() {
		yOrigin = transform.position.y;
		scaleOrigin = transform.localScale.x;
	}

	//Psuedo state machine, toggles coroutines for animations
	//Originally a coroutine, but I want to kill other coroutines within and had trouble stopping individual coroutines
    private void FixedUpdate() {
        if (active) {
			if (!raised && !raising) { 
				StopAllCoroutines();
				StartCoroutine(Activate());
				foreach (SpriteRenderer sr in spriteRenderers) 
					sr.sortingOrder = 2;
			}
			else if (raised && !raising) {
			// 	StopAllCoroutines();
			// 	StartCoroutine(SinWaveBob());
			}
        } else {
			if ((raising || raised) && !lowering) { 
				StopAllCoroutines();
				StartCoroutine(Deactivate());
				foreach (SpriteRenderer sr in spriteRenderers) 
					sr.sortingOrder = 0;
			}
        }

	}

	public void Selected() {
		foreach (SpriteRenderer sr in spriteRenderers) 
			sr.sortingOrder = 1;
	}
    
//Offset y 
    public IEnumerator Activate() {
		raising = true;
		lowering = false;

		float time = 0;
		while(time < duration) { //lerp y position to offset from origin
			transform.position = 
				new Vector3(
				transform.position.x, 
				Mathf.Lerp(yOrigin, yOrigin + yOffset, time/duration), 
				transform.position.z);			
			transform.localScale = Vector3.one * Mathf.Lerp(scaleOrigin, 1.5f, time/duration);
			
			time += Time.deltaTime;
			yield return null;
		}

		raised = true;
		raising = false;
	}

//Bob up and down along y offset once raised
	public IEnumerator SinWaveBob() {
		raising = true;
		lowering = false;

		float time = 0;
		while(true) {
			transform.position = 
				new Vector3(
				transform.position.x,
				yOrigin + yOffset + Mathf.Sin(time * sinFrequency) * yOffset / sinAmplitude, 
				transform.position.z);
			time += Time.deltaTime;
			yield return null;
		}
	}

//
//Return y to origin
	public IEnumerator Deactivate() {
		lowering = true;
		raising = false;

		float time = 0;
		while(time <= duration) {				
			transform.position = 
				new Vector3(
				transform.position.x, 
				Mathf.Lerp(transform.position.y, yOrigin, time/duration), 
				transform.position.z);
			transform.localScale = Vector3.one * Mathf.Lerp(transform.localScale.x, scaleOrigin, time/duration);


			time += Time.deltaTime;
			yield return null;
		}

		lowering = false;
		raised = false;
	}
//
}