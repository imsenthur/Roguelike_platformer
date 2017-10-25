using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotSpawner : MonoBehaviour {

    public Map MapData;
    public GameObject Bot;
    public GameObject Player;
    public int BotCount;
    public Transform[] spawnpoints;

	// Use this for initialization
	void OnEnable()
    {
        int x = Random.Range(1, 5);
        for (int i=1;i<=BotCount;i++)
        {

            if(i==x)
            {
                spawnplayer(i);
            }
            else
            {
                spawnazombie(i);
            }
        }

	}


    public void spawnazombie(int i)
    {
        GameObject go = Instantiate(Bot, spawnpoints[i - 1].position, this.gameObject.transform.rotation);
        go.tag = "Enemy";
        go.name = "Zombie" + i;
        go.GetComponent<Bot>().mMap = MapData;
    }

    public void spawnplayer(int i)
    {
        GameObject go = Instantiate(Player, spawnpoints[i - 1].position, this.gameObject.transform.rotation);
        go.tag = "Player";
        go.name = "Player";
        go.GetComponent<AudioListener>().enabled = true;
    }

}
