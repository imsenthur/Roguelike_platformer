using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Meleeattack : MonoBehaviour {
    private void OnTriggerEnter2D(Collider2D collision)
    {
        collision.gameObject.GetComponent<DamageHandler>().health--;
    }
}
