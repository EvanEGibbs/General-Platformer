﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {

	//explanation of whole class: https://www.youtube.com/watch?v=WL_PaUyRAXQ&t=1s

	public Controller2D target; //usually the player
	public float verticalOffset;
	public float lookAheadDstX;
	public float lookSmoothTimeX;
	public float verticalSmoothTime;
	public Vector2 focusAreaSize;

	FocusArea focusArea;

	float currentLookAheadX;
	float targetLookAheadX;
	float lookAheadDirectionX;
	float smoothLookVelocityX;
	float smoothVelocityY;

	bool lookAheadStopped;
	bool lookAheadStopped2;

	private void Start() {
		focusArea = new FocusArea(target.myCollider.bounds, focusAreaSize);
	}

	private void LateUpdate() { //late update so the camera adjusts itself after all movement is finished

		focusArea.Update(target.myCollider.bounds);

		Vector2 focusPosition = focusArea.center + Vector2.up * verticalOffset;

		//if (focusArea.velocity.x != 0) {
		if (focusArea.touchingRight || focusArea.touchingLeft) {
			lookAheadDirectionX = (focusArea.touchingLeft) ? -1 : 1;
			if (Mathf.Sign(target.directionalInput.x) == lookAheadDirectionX && target.directionalInput.x != 0) {
				lookAheadStopped = false;
				targetLookAheadX = lookAheadDirectionX * lookAheadDstX;
			} else {
				if (!lookAheadStopped) {
					lookAheadStopped = true;
					targetLookAheadX = currentLookAheadX + (lookAheadDirectionX * lookAheadDstX - currentLookAheadX) / 4f;
				}
			}
		}

		currentLookAheadX = Mathf.SmoothDamp(currentLookAheadX, targetLookAheadX, ref smoothLookVelocityX, lookSmoothTimeX);

		focusPosition.y = Mathf.SmoothDamp(transform.position.y, focusPosition.y, ref smoothVelocityY, verticalSmoothTime);
		focusPosition += Vector2.right * currentLookAheadX;
		transform.position = (Vector3)focusPosition + Vector3.forward * -10;
	}

	private void OnDrawGizmos() {
		Gizmos.color = new Color(1, 0, 0, .5f);
		Gizmos.DrawCube(focusArea.center, focusAreaSize);
	}

	struct FocusArea {
		public Vector2 center;
		public Vector2 velocity;
		public bool touchingLeft;
		public bool touchingRight;
		float left, right;
		float top, bottom;

		public FocusArea(Bounds targetBounds, Vector2 size) {
			left = targetBounds.center.x - size.x / 2;
			right = targetBounds.center.x + size.x / 2;
			bottom = targetBounds.min.y;
			top = targetBounds.min.y + size.y;
			touchingLeft = false;
			touchingRight = false;

			velocity = Vector2.zero;
			center = new Vector2((left + right) / 2, (top + bottom) / 2);
		}

		public void Update(Bounds targetBounds) {
			float shiftX = 0;
			touchingLeft = false;
			touchingRight = false;
			if (targetBounds.min.x <= left) {
				touchingLeft = true;
				shiftX = targetBounds.min.x - left;
			}
			else if (targetBounds.max.x >= right) {
				touchingRight = true;
				shiftX = targetBounds.max.x - right;
			}
			left += shiftX;
			right += shiftX;

			float shiftY = 0;
			if (targetBounds.min.y < bottom) {
				shiftY = targetBounds.min.y - bottom;
			} else if (targetBounds.max.y > top) {
				shiftY = targetBounds.max.y - top;
			}
			top += shiftY;
			bottom += shiftY;
			center = new Vector2((left + right) / 2, (top + bottom) / 2);
			velocity = new Vector2(shiftX, shiftY);
		}
	}
}
