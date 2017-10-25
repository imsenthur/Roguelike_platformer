using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour {

	public Sprite[] hearts;
	public Image HeartUI;
    int maxhealth = 5;
	private DamageHandler player;

	// Use this for initialization
	void OnEnable () 
	{
        HeartUI.sprite = hearts[maxhealth];
        player = GameObject.FindGameObjectWithTag ("Player").GetComponent<DamageHandler>();


	}
	
	// Update is called once per frame
	void Update () 
	{
		HeartUI.sprite=hearts[player.health];


	}
}
