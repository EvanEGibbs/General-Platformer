using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxBackground : MonoBehaviour {

	List<GameObject> backgroundElements = new List<GameObject>();

	// Use this for initialization
	void Start () {
		ListBackgroundElements();
	}

	void ListBackgroundElements() {
		foreach (Transform child in transform) {
			backgroundElements.Add(child.gameObject);
		}
	}
	
	public void ParallaxMove(Vector3 cameraMove) {
		foreach(GameObject backgroundElement in backgroundElements) {
			float zPosition = backgroundElement.transform.position.z;
			float movementMultiplier = (Mathf.Sign(zPosition) == 1) ? zPosition / 100 : zPosition / -100 + 1;
			backgroundElement.transform.Translate(cameraMove * movementMultiplier);
		}
	}
}
