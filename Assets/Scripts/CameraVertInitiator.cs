using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraVertInitiator : MonoBehaviour {

	public float verticalOffset;
	public float readjustmentTime;

	public Camera mainCamera;

	private void OnTriggerStay2D(Collider2D collision) {
		if (collision.gameObject.tag == "Player") {
			mainCamera.GetComponent<CameraFollow>().AdjustVerticalOffset(verticalOffset, readjustmentTime);
		}
	}
}
