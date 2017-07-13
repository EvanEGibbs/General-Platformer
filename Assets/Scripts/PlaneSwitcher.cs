using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneSwitcher : MonoBehaviour {

	public GameObject mainPlane;
	public GameObject newPlane;
	public float transitionTime;

	private void OnTriggerEnter2D(Collider2D collision) {
		if (collision.gameObject.tag == "Player") {
			mainPlane.GetComponent<MainPlane>().NewSize(newPlane.transform.position, newPlane.transform.localScale, transitionTime);
		}
	}
}
