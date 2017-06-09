using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class RaycastController : MonoBehaviour {

	public LayerMask collisionMask;

	public const float SKIN_WIDTH = .015f;
	public int horizontalRayCount = 4;
	public int verticalRayCount = 4;

	[HideInInspector]
	public float horizontalRaySpacing;
	[HideInInspector]
	public float verticalRaySpacing;

	[HideInInspector]
	public BoxCollider2D myCollider;
	[HideInInspector]
	public RaycastOrigins raycastOrigins;

	public virtual void Awake() {
		myCollider = GetComponent<BoxCollider2D>();
	}

	public virtual void Start() {
		CalculateRaySpacing();
	}

	public void UpdateRaycastOrigins() {
		Bounds bounds = myCollider.bounds;
		bounds.Expand(SKIN_WIDTH * -2);

		raycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
		raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
		raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
		raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);
	}

	public void CalculateRaySpacing() {
		Bounds bounds = myCollider.bounds;
		bounds.Expand(SKIN_WIDTH * -2);

		horizontalRayCount = Mathf.Clamp(horizontalRayCount, 2, int.MaxValue);
		verticalRayCount = Mathf.Clamp(verticalRayCount, 2, int.MaxValue);

		horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
		verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
	}

	public struct RaycastOrigins {
		public Vector2 topLeft, topRight;
		public Vector2 bottomLeft, bottomRight;
	}
}
