using System.Collections;
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

	public GameObject levelPlane;
	MeshCollider levelPlaneCollider;
	public ParallaxBackground parallaxBackground;

	float originalVerticalOffset;
	float verticalOffsetTimer = 0;
	float currentLookAheadX;
	float targetLookAheadX;
	float lookAheadDirectionX;
	float smoothLookVelocityX;
	float smoothVelocityY;
	float originalZ;

	bool lookAheadStopped;
	bool lookAheadStopped2;

	Camera cameraComponent;

	private void Start() {
		focusArea = new FocusArea(target.myCollider.bounds, focusAreaSize);
		levelPlaneCollider = levelPlane.GetComponent<MeshCollider>();
		cameraComponent = GetComponent<Camera>();
		originalZ = transform.position.z;
		originalVerticalOffset = verticalOffset;
	}

	private void LateUpdate() { //late update so the camera adjusts itself after all movement is finished

		ResetVerticalOffset();

		Vector3 oldCoordinates = transform.position;

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
		transform.position = (Vector3)focusPosition + Vector3.forward * originalZ;

		ConstrictCameraToBoundaries();

		Vector3 moveAmount = transform.position - oldCoordinates;
		if (parallaxBackground != null) {
			parallaxBackground.ParallaxMove(moveAmount);
		}
	}

	public void AdjustVerticalOffset(float newVertOffset, float newVertOffsetTimer) {
		verticalOffset = newVertOffset;
		verticalOffsetTimer = newVertOffsetTimer;
	}

	void ResetVerticalOffset() {
		if (verticalOffsetTimer > 0) {
			verticalOffsetTimer -= Time.deltaTime;
			if (verticalOffsetTimer <= 0) {
				verticalOffsetTimer = 0;
				verticalOffset = originalVerticalOffset;
			}
		}
	}

	void ConstrictCameraToBoundaries() {

		//explanation of the code: http://answers.unity3d.com/questions/1011399/fix-boundries-for-perspective-camera.html

		Vector3 levelExtents = levelPlaneCollider.bounds.extents;
		Vector3 topRightEdge = new Vector3 (levelPlane.transform.position.x + levelExtents.x, levelPlane.transform.position.y + levelExtents.y, levelPlane.transform.position.z + transform.position.z);
		Vector3 downLeftEdge = new Vector3 (levelPlane.transform.position.x - levelExtents.x, levelPlane.transform.position.y - levelExtents.y, levelPlane.transform.position.z + transform.position.z);

		Vector3 topRightEdgeScreen = cameraComponent.WorldToScreenPoint(topRightEdge);
		Vector3 downLeftEdgeScreen = cameraComponent.WorldToScreenPoint(downLeftEdge);

		//Debug.Log(downLeftEdgeScreen + "  " + Screen.height);

		// Is the camera out of the map bounds?
		if (topRightEdgeScreen.x < Screen.width || topRightEdgeScreen.y < Screen.height || downLeftEdgeScreen.x > 0 || downLeftEdgeScreen.y > 0) {
			//smack a big plane at the camera position that covers more than the screen is showing
			Plane cameraPositionFixPlane = new Plane(Vector3.forward * 10, transform.position);

			//move the top right edge back so its inside the screen again
			Vector3 topRightEdgeScreenFixed = cameraComponent.ScreenToWorldPoint(new Vector3(Mathf.Max(Screen.width, topRightEdgeScreen.x), Mathf.Max(Screen.height, topRightEdgeScreen.y), topRightEdgeScreen.z));
			//now we know the offset the camera should move at distance z to fix the top right edge
			Vector3 topRightOffsetAtDistance = topRightEdgeScreenFixed - topRightEdge;

			//this time for the down left edge
			Vector3 downLeftEdgeScreenFixed = Camera.main.ScreenToWorldPoint(new Vector3(Mathf.Min(0, downLeftEdgeScreen.x), Mathf.Min(0, downLeftEdgeScreen.y), downLeftEdgeScreen.z));
			//now we know the offset the camera should move at distance z to fix the down left edge
			Vector3 downLeftOffsetAtDistance = downLeftEdgeScreenFixed - downLeftEdge;

			//Debug.Log("offset: " + downLeftOffsetAtDistance);


			//where is the center of the screen translated at given distance
			Vector3 cameraCenterAtDistance = cameraComponent.ScreenToWorldPoint(new Vector3(Screen.width / 2.0f, Screen.height / 2.0f, topRightEdge.z));
			//now lets offset the center of the screen with the offset we found
			Vector3 cameraCenterAtDistanceFixed = new Vector3(cameraCenterAtDistance.x - topRightOffsetAtDistance.x - downLeftOffsetAtDistance.x, cameraCenterAtDistance.y - topRightOffsetAtDistance.y - downLeftOffsetAtDistance.y, cameraCenterAtDistance.z);

			//here we generate a ray at the camera center at distance pointing back to the camera
			Ray rayFromFixedDistanceToCameraPlane = new Ray(cameraCenterAtDistanceFixed, -cameraComponent.transform.forward);

			//this is where the magic happens, lets raycast back to the plane i smacked infront of the  camera
			float d;
			cameraPositionFixPlane.Raycast(rayFromFixedDistanceToCameraPlane, out d);

			//where did the raycast hit the camera plane?
			Vector3 planeHitPoint = rayFromFixedDistanceToCameraPlane.GetPoint(d);

			//position camera at the hitpoint we found
			transform.position = new Vector3(planeHitPoint.x, planeHitPoint.y, transform.position.z);

		}

		//if the level plane's width is thinner than the camera width, snap the camera's x to the center of the level plane
		if (topRightEdgeScreen.x - downLeftEdgeScreen.x < Screen.width) {
			transform.position = new Vector3(levelPlane.transform.position.x, transform.position.y, transform.position.z);
		}
		//if the level plane's height is smaller than the camera's height, snap the camera's y to the center of the level plane
		if (topRightEdgeScreen.y - downLeftEdgeScreen.y < Screen.height) {
			transform.position = new Vector3(transform.position.x, levelPlane.transform.position.y, transform.position.z);
		}

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
