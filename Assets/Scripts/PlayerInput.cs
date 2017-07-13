using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Player))]
public class PlayerInput : MonoBehaviour {

	Player player;

	void Start () {
		player = GetComponent<Player>();
	}
	
	void Update () {
		//directional input (x-1 = left, x1 = right, y-1 = up, y1 = down)
		Vector2 directionalInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
		directionalInputJoystickFix(ref directionalInput);
		player.SetDirectionalInput(directionalInput);
		//jump button down
		if (Input.GetButtonDown("Jump")) {
			player.OnJumpInputDown();
		}
		//jump button up
		if (Input.GetButtonUp("Jump")) {
			player.OnJumpInputUp();
		}
	}

	void directionalInputJoystickFix(ref Vector2 directionalInput) {
		if (directionalInput.x > -.5 && directionalInput.x < 0 || directionalInput.x < .5 && directionalInput.x > 0) {
			directionalInput.x = 0;
		}
		if (directionalInput.x <= -.5) {
			directionalInput.x = -1;
		}
		if (directionalInput.x > .5) {
			directionalInput.x = 1;
		}

		if (directionalInput.y > -.5 && directionalInput.y < 0 || directionalInput.y < .5 && directionalInput.y > 0) {
			directionalInput.y = 0;
		}
		if (directionalInput.y <= -.5) {
			directionalInput.y = -1;
		}
		if (directionalInput.y > .5) {
			directionalInput.y = 1;
		}
	}
}
