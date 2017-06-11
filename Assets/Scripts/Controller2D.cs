using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller2D : RaycastController {

	public float maxSlopeAngle = 80;
	//collision info holds all the information about the objects collissions, resets every frame and then re-calculates
	public CollisionInfo collisions;
	[HideInInspector]
	public Vector2 directionalInput;

	public override void Start() {
		base.Start();
		//default to facing right
		collisions.faceDirection = 1;
	}
	//move function for objects that are not controlled, calls the next move function wiith zeroed out controller information
	public void Move(Vector2 moveAmount, bool standingOnPlatform) {
		Move(moveAmount, false, Vector2.zero, standingOnPlatform);
	}

	public void Move(Vector2 moveAmount, bool jumpInputDown, Vector2 _directionalInput, bool standingOnPlatform = false) {
		UpdateRaycastOrigins();
		collisions.Reset();
		collisions.moveAmountOld = moveAmount;
		directionalInput = _directionalInput;
		
		//check for decending slopes
		if (moveAmount.y < 0) {
			DescendSlope(ref moveAmount);
		}
		//update which direction facing
		if (moveAmount.x != 0) {
			collisions.faceDirection = (int)Mathf.Sign(moveAmount.x);
		}
		//Horizontal Collisions
		HorizontalCollisions(ref moveAmount, jumpInputDown);
		//Vertical Collisions
		if (moveAmount.y != 0) {
			VerticalCollisions(ref moveAmount, jumpInputDown);
		}

		transform.Translate(moveAmount);

		//used for moving platforms, tells the player he's standing on a platform
		if (standingOnPlatform) {
			collisions.below = true;
		}
	}

	void HorizontalCollisions(ref Vector2 moveAmount, bool jumpInputDown) {
		float directionX = collisions.faceDirection;
		float rayLength = Mathf.Abs(moveAmount.x) + SKIN_WIDTH;
		//if not moving, check for collisions on the side by a minor amount (usually for wall sliding)
		if (Mathf.Abs(moveAmount.x) < SKIN_WIDTH){
			rayLength = 2 * SKIN_WIDTH;
		}
		//the jumpinputdown variable is for the frame that the player jumps, a ray is not cast from the bottom of the collision, so they won't collide with any slopes and stutter their jump
		//each of the for loops in these sections is to check collissions on each of the rays that are drawn on the colllider
		for (int i = (!jumpInputDown)?0:1; i < horizontalRayCount; i++) {
			Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
			rayOrigin += Vector2.up * (horizontalRaySpacing * i);
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

			Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.red);

			if (hit) {
				//if hitting a through obstacle from the side, continue (go to the next part of the for loop, in this case, the next ray
				if (hit.collider.tag == "Through") {
					continue;
				}
				//if inside the obstacle, usually in the case of a moving block that pushes you into a solid one
				if (hit.distance == 0) {
					continue;
				}
				//slopes that the user can climb
				float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
				if (i == 0 && slopeAngle <= maxSlopeAngle) {
					if (collisions.descendingSlope) {
						collisions.descendingSlope = false;
						moveAmount = collisions.moveAmountOld;
					}
					float distanceToSlopeStart = 0;
					if (slopeAngle != collisions.slopeAngleOld) {
						distanceToSlopeStart = hit.distance - SKIN_WIDTH;
						moveAmount.x -= distanceToSlopeStart * directionX;
					}
					ClimbSlope(ref moveAmount, slopeAngle, hit.normal);
					moveAmount.x += distanceToSlopeStart * directionX;
				}
				//normal collision
				if (!collisions.climbingSlope || slopeAngle > maxSlopeAngle) {
					moveAmount.x = (hit.distance - SKIN_WIDTH) * directionX;
					rayLength = hit.distance;

					if (collisions.climbingSlope) {
						//math here: https://www.youtube.com/watch?v=cwcC2tIKObU
						moveAmount.y = Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(moveAmount.x);
					}

					collisions.left = directionX == -1;
					collisions.right = directionX == 1;
				}
			}
		}
	}

	void ClimbSlope(ref Vector2 moveAmount, float slopeAngle, Vector2 slopeNormal) { //climbing a normal slup under max angle
		//math here: https://www.youtube.com/watch?v=cwcC2tIKObU at around 4 minutes
		float moveDistance = Mathf.Abs(moveAmount.x);
		float climbmoveAmountY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;

		if (moveAmount.y <= climbmoveAmountY) {
			moveAmount.y = climbmoveAmountY;
			moveAmount.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(moveAmount.x);
			collisions.below = true;
			collisions.climbingSlope = true;
			collisions.slopeAngle = slopeAngle;
			collisions.slopeNormal = slopeNormal;
		}
	}

	void DescendSlope(ref Vector2 moveAmount) { //descending a slope that is under max angle. Check video after the one posted in climb slope for explanation

		RaycastHit2D maxSlopeHitLeft = Physics2D.Raycast(raycastOrigins.bottomLeft, Vector2.down, Mathf.Abs(moveAmount.y) + SKIN_WIDTH, collisionMask);
		RaycastHit2D maxSlopeHitRight = Physics2D.Raycast(raycastOrigins.bottomRight, Vector2.down, Mathf.Abs(moveAmount.y) + SKIN_WIDTH, collisionMask);
		if (maxSlopeHitLeft ^ maxSlopeHitRight) {
			SlideDownMaxSlope(maxSlopeHitLeft, ref moveAmount);
			SlideDownMaxSlope(maxSlopeHitRight, ref moveAmount);
		}

		if (!collisions.slidingDownMaxSlope) {
			float directionX = Mathf.Sign(moveAmount.x);
			Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomRight : raycastOrigins.bottomLeft;
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin, -Vector2.up, Mathf.Infinity, collisionMask);
			if (hit) {
				float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
				if (slopeAngle != 0 && slopeAngle <= maxSlopeAngle) {
					if (Mathf.Sign(hit.normal.x) == directionX) {
						if (hit.distance - SKIN_WIDTH <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(moveAmount.x)) {
							float moveDistance = Mathf.Abs(moveAmount.x);
							float descendmoveAmountY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;
							moveAmount.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(moveAmount.x);
							moveAmount.y -= descendmoveAmountY;

							collisions.slopeAngle = slopeAngle;
							collisions.descendingSlope = true;
							collisions.below = true;
							collisions.slopeNormal = hit.normal;
						}
					}
				}
			}
		}
	}

	void SlideDownMaxSlope(RaycastHit2D hit, ref Vector2 moveAmount) { //if the slope is too steep to climb, slide down it

		if (hit) {
			float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
			if (slopeAngle > maxSlopeAngle) {
				moveAmount.x = hit.normal.x * (Mathf.Abs(moveAmount.y) - hit.distance) / Mathf.Tan(slopeAngle * Mathf.Deg2Rad);

				collisions.slopeAngle = slopeAngle;
				collisions.slidingDownMaxSlope = true;
				collisions.slopeNormal = hit.normal;
			}
		}
	}

	void VerticalCollisions(ref Vector2 moveAmount, bool jumpInputDown) { //similiar to horizontal but with a few differences, especially with through platforms. Also stores standing on top of if on one
		float directionY = Mathf.Sign(moveAmount.y);
		float rayLength = Mathf.Abs(moveAmount.y) + SKIN_WIDTH;

		if (Mathf.Abs(moveAmount.y) < SKIN_WIDTH) {
			rayLength = 2 * SKIN_WIDTH;
		}

		for (int i = 0; i < verticalRayCount; i++) {
			Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
			rayOrigin += Vector2.right * (verticalRaySpacing * i + moveAmount.x);
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);

			Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.red);
			
			if (hit) {
				if (hit.collider.tag == "Through") {
					if (directionY == 1 || hit.distance == 0) { //if moving up, continue to next ray. (only collides of moving down)
						continue;
					}
					if (collisions.fallingThrough == true && collisions.fallingThroughPlatform == hit.collider) { //if the user if falling through the platform
						continue;
					}
					if (directionalInput.y == -1) { //enables the user to drop through the platform after they press down
						collisions.readyToFallThrough = true;
					}
					if (directionalInput.y == -1 && jumpInputDown) { //drops through the platform
						if (hit.collider == collisions.fallingThroughPlatform) {
							collisions.fallingThrough = true;
							continue;
						}
					}
				}

				collisions.fallingThrough = false; //if made it through the last if statement, the user isn't falling through the platform
				collisions.fallingThroughPlatform = hit.collider; //sets the platform the user is standing on top of, important for if the user does fall through it, it stores it and resets it when landing on another platform
				moveAmount.y = (hit.distance - SKIN_WIDTH) * directionY;
				rayLength = hit.distance;

				if (collisions.climbingSlope) {
					//math here: https://www.youtube.com/watch?v=cwcC2tIKObU
					moveAmount.x = moveAmount.y / Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Sign(moveAmount.x);
				}

				collisions.below = directionY == -1;
				collisions.above = directionY == 1;
			}
		}
		//chicking for collisions when climbing a slope...?
		if (collisions.climbingSlope) {
			float directionX = Mathf.Sign(moveAmount.x);
			rayLength = Mathf.Abs(moveAmount.x) + SKIN_WIDTH;
			Vector2 rayOrigin = ((directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight) + Vector2.up * moveAmount.y;
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

			if (hit) {
				float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
				if (slopeAngle != collisions.slopeAngle) {
					moveAmount.x = (hit.distance - SKIN_WIDTH) * directionX;
					collisions.slopeAngle = slopeAngle;
					collisions.slopeNormal = hit.normal;
				}
			}
		}
	}

	public struct CollisionInfo {
		public bool above, below;
		public bool left, right;

		public bool climbingSlope;
		public bool descendingSlope;
		public bool slidingDownMaxSlope;
		public float slopeAngle, slopeAngleOld;
		public Vector2 slopeNormal;

		public Vector2 moveAmountOld;
		public int faceDirection;
		public bool fallingThrough;
		public bool readyToFallThrough;
		public Collider2D fallingThroughPlatform;

		public void Reset() {
			above = below = false;
			left = right = false;
			climbingSlope = false;
			descendingSlope = false;
			slidingDownMaxSlope = false;
			readyToFallThrough = false;
			slopeAngleOld = slopeAngle;
			slopeAngle = 0;
			slopeNormal = Vector2.zero;
		}
	}
}
