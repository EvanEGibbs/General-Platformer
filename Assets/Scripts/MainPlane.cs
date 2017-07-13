using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainPlane : MonoBehaviour {

	Vector3 position;
	Vector3 size;
	Vector3 repositionVelocity;
	Vector3 resizeVelocity;
	float transitionTime = 0;

	private void Start() {
		position = transform.position;
		size = transform.localScale;
	}

	private void Update() {
		if (transform.position != position || transform.localScale != size) {
			transform.position = Vector3.SmoothDamp(transform.position, position, ref repositionVelocity, transitionTime);
			transform.localScale = Vector3.SmoothDamp(transform.localScale, size, ref resizeVelocity, transitionTime);
		}
	}

	public void NewSize(Vector3 newPosition, Vector3 newSize, float newTransitionTime) {
		position = newPosition;
		size = newSize;
		transitionTime = newTransitionTime;
	}
}
