using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(Controller2D))]
[RequireComponent (typeof(PlayerInput))]
public class Player : MonoBehaviour {

	public float maxJumpHeight = 4;
	public float minJumpHeight = 1;
	public float timeToJumpApex = .4f;
	public float accelerationTimeAirborne = .1f;
	public float accelerationTimeGrounded = .1f;
	public float decelerationTimeAirborne = 0f;
	public float decelerationTimeGrounded = 0f;
	public float moveSpeed = 6;

	public Vector2 wallJumpClimb;
	public Vector2 wallJumpOff;
	public Vector2 wallLeap;

	public float wallSlideSpeedMax = 3;
	public float wallStickTime = .25f;

	float timeToWallUnstick;
	float gravity; //calculated via jump height and timeToJumpApex
	float maxJumpVelocity;
	float minJumpVelocity;

	Vector3 velocity;
	float velocityXSmoothing;

	Controller2D controller;
	public Vector2 directionalInput;

	bool wallSliding;
	int wallDirX;
	bool jumpInputDown;

	void Start () {
		controller = GetComponent<Controller2D>();

		//explanation of the following math: https://www.youtube.com/watch?v=PlT44xr0iW0&t=9s at around 6 minutes
		//can move the following code to update to mess with parameters in real time for when getting the right gravity, jumpping, etc.
		gravity = -(2 * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
		maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
		//math here: https://www.youtube.com/watch?v=rVfR14UNNDo&t=2s
		minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpHeight);
		//Debug.Log("Gravity " + gravity + " jump velocity: " + maxJumpVelocity);
	}

	void Update() {

		CalculateVelocity();
		HandleWallSliding();

		controller.Move(velocity * Time.deltaTime, jumpInputDown, directionalInput);

		if (controller.collisions.above || controller.collisions.below) {
			if (controller.collisions.slidingDownMaxSlope) { //sliding down maximum slope
				velocity.y += controller.collisions.slopeNormal.y * -gravity * Time.deltaTime;
			} else { //reset velocity if hiting a block from above, or landing on one from below
				velocity.y = 0;
			}
		}
		jumpInputDown = false; //so the jumpInputDown variable is only set to be true for one frame when the jump button is pressed
	}

	//BUTTON INPUTS

	public void SetDirectionalInput(Vector2 input) { //from playerInput class
		directionalInput = input;
	}

	public void OnJumpInputDown() { //from playerInput class
		if (!controller.collisions.above) { //if there's nothing directly above you, you can jump
			jumpInputDown = true;
			//wall jumping
			if (wallSliding) { //Jump off of the wall
				if (wallDirX == directionalInput.x) { //Wall jump climb
					timeToWallUnstick = 0;
					velocity.x = -wallDirX * wallJumpClimb.x;
					velocity.y = wallJumpClimb.y;
				} else if (directionalInput.x == 0) { //jump off of wall
					timeToWallUnstick = 0;
					velocity.x = -wallDirX * wallJumpOff.x;
					velocity.y = wallJumpOff.y;
				} else { //leap off of wall
					timeToWallUnstick = 0;
					velocity.x = -wallDirX * wallLeap.x;
					velocity.y = wallLeap.y;
				}
			}

			if (controller.collisions.below) {
				//jump while sliding down a steep slope
				if (controller.collisions.slidingDownMaxSlope) {
					if (directionalInput.x != -Mathf.Sign(controller.collisions.slopeNormal.x)) { //not jumping against max slope
						velocity.y = maxJumpVelocity * controller.collisions.slopeNormal.y;
						velocity.x = maxJumpVelocity * controller.collisions.slopeNormal.x;

					}
				}
				//jump off of ground normally if not going to jump down a through platform
				else if (!controller.collisions.readyToFallThrough) {
					velocity.y = maxJumpVelocity;
				}
			}
		}
	}
	public void OnJumpInputUp() { //for variable jump height
		if (velocity.y > minJumpVelocity) {
			velocity.y = minJumpVelocity;
		}
	}

	private void CalculateVelocity() {
		float targetVelocity = directionalInput.x * moveSpeed;

		if (targetVelocity == 0 || Mathf.Sign(targetVelocity) != Mathf.Sign(velocity.x) && velocity.x != 0) { //if coming to a stop or moving in the opposite direction, use deceleration time
			velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocity, ref velocityXSmoothing, (controller.collisions.below) ? decelerationTimeGrounded : decelerationTimeAirborne);
		} else { //otherwise, use acceleration time
			velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocity, ref velocityXSmoothing, (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);
		}
		
		velocity.y += gravity * Time.deltaTime;
	}

	private void HandleWallSliding() {
		wallDirX = (controller.collisions.left) ? -1 : 1;
		wallSliding = false;
		if ((controller.collisions.left || controller.collisions.right) && !controller.collisions.below) {
			wallSliding = true;
			controller.collisions.fallingThroughPlatform = null; //if fell through a platform then started a wall slide, can jump on the platform again.

			if (velocity.y < -wallSlideSpeedMax) {
				velocity.y = -wallSlideSpeedMax;
			}

			if (timeToWallUnstick > 0) { //stick to wall if not holding the button long enough
				velocityXSmoothing = 0;
				velocity.x = 0;

				if (directionalInput.x != wallDirX && directionalInput.x != 0) { //holding button to unstick from wall
					timeToWallUnstick -= Time.deltaTime;
				} else { //restick to wall if not holding button towards wall
					timeToWallUnstick = wallStickTime;
				}
			} else { //restick to wall if not pressing either direction
				timeToWallUnstick = wallStickTime;
			}
		}
	}
}
