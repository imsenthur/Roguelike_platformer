using UnityEngine;
using System.Collections;

public class MoveForward : MonoBehaviour {

	public float maxSpeed = 5f;
    public float localscale;
    private Vector2 bulletdirection;
    private Transform startpos;
    private Transform playerpos;
    public Transform hand;
    // Update is called once per frame
    void Update ()
    {
       
        Vector3 pos = transform.position;

        Vector3 velocity = new Vector3(localscale* maxSpeed * Time.deltaTime,0, 0);

        pos += transform.rotation * velocity;

		transform.position = pos;
	}
    

}
