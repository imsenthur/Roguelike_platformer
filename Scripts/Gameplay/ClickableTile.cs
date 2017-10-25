using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class ClickableTile : MonoBehaviour {
    
    public int TileX=0;
    public int TileY=0;
    public GameObject[] Bots;

    public Map levelmap;
    
    private void Start()
    {

        //Bots = GameObject.FindGameObjectsWithTag("Enemy");
        //levelmap = BotAiScript1.mMap;

    }
   
    private void OnMouseUp()
    {
       // Debug.Log("clicked on (" + TileX + ", " + TileY + ")");
       // BotAiScript5.TappedOnTile(new Vector2i(TileX, TileY));
    }
    
}
