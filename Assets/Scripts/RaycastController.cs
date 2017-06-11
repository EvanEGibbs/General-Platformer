using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class RaycastController : MonoBehaviour {

	public LayerMask collisionMask;
	[HideInInspector]
	public BoxCollider2D myCollider;

	public const float SKIN_WIDTH = .015f;
	const float dstBetweenRays = .25f;
	[HideInInspector]
	public int horizontalRayCount;
	[HideInInspector]
	public int verticalRayCount;
	[HideInInspector]
	public float horizontalRaySpacing;
	[HideInInspector]
	public float verticalRaySpacing;

	[HideInInspector]
	public RaycastOrigins raycastOrigins;

	public virtual void Awake() {
		myCollider = GetComponent<BoxCollider2D>();
	}

	public virtual void Start() {
		CalculateRaySpacing();
	}

	public void UpdateRaycastOrigins() { //updates each corner every frame in controller2D
		Bounds bounds = myCollider.bounds;
		bounds.Expand(SKIN_WIDTH * -2);

		raycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
		raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
		raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
		raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);
	}

	public void CalculateRaySpacing() { //calculates how many rays based on size of the collider and spaces them evenly horizontally and vertically
		Bounds bounds = myCollider.bounds;
		bounds.Expand(SKIN_WIDTH * -2);

		float boundsWidth = bounds.size.x;
		float boundsHeight = bounds.size.y;
		//calculate how many rays
		horizontalRayCount = Mathf.RoundToInt(boundsHeight / dstBetweenRays);
		verticalRayCount = Mathf.RoundToInt(boundsWidth / dstBetweenRays);

		horizontalRayCount = Mathf.Clamp(horizontalRayCount, 2, int.MaxValue);
		verticalRayCount = Mathf.Clamp(verticalRayCount, 2, int.MaxValue);
		//spaces rays
		horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
		verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
	}

	public struct RaycastOrigins { //struct to store each corner of the collider
		public Vector2 topLeft, topRight;
		public Vector2 bottomLeft, bottomRight;
	}
}
