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
		player.SetDirectionalInput(directionalInput);
		//jump button down
		if (Input.GetKeyDown(KeyCode.Space)) {
			player.OnJumpInputDown();
		}
		//jump button up
		if (Input.GetKeyUp(KeyCode.Space)) {
			player.OnJumpInputUp();
		}
	}
}
