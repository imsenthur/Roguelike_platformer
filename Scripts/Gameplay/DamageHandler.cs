using UnityEngine;
using System.Collections;

public class DamageHandler : MonoBehaviour {

	public int health = 1;
    private BotSpawner botspawnerscript;
    private bool Damagetaken;
    private float Gametimer;
    private int seconds = 0;
    public int UpdateRate = 2;
    public HomeScreenMgr managerscreenscript;
    private GameObject[] zombies;
    public float invulnPeriod = 0;
	float invulnTimer = 0;
    public Map mapdata;
    public int x, y;

	SpriteRenderer spriteRend;

	void Start()
    {


        if(this.gameObject.tag=="Player")
        {
            managerscreenscript = GameObject.FindGameObjectWithTag("Canvas").GetComponent<HomeScreenMgr>();
        }

        botspawnerscript = GameObject.FindGameObjectWithTag("Respawn").GetComponent<BotSpawner>();

       if (this.gameObject.tag == "Bullet")
            Destroy(this.gameObject, 2);
	}

	void OnTriggerEnter2D(Collider2D trigger)
    {
        if (this.gameObject.tag == "Bullet") health--;

        else if (this.gameObject.tag == "Player")
        {
            health--;
        }

        else if (this.gameObject.tag == "Enemy")
        {
            if(trigger.gameObject.tag=="Bullet")
            health--;

        }
        else if (this.gameObject.tag == "Head")
        {
            if (trigger.gameObject.tag == "Bullet")
                health--;
        }
        else if(this.gameObject.tag == "Environment")
        {
            if(trigger.gameObject.tag=="Bullet")
            health--;
            //Display an effect later by changing the sprite
        }

        else
        {
            health--;
        }

        if (invulnPeriod > 0) {
			invulnTimer = invulnPeriod;
			gameObject.layer = 10;
		}
	}

	void Update() {

		if(invulnTimer > 0) invulnTimer -= Time.deltaTime;

        if (Time.time > Gametimer + 1)
        {
            Gametimer = Time.time;
            seconds++;
        }

        if (seconds == UpdateRate)
        {
            seconds = 0;
            Damagetaken = false;
        }

        if (health <= 0) {
			Die();
		}
	}

    void thingstodoforzombie()
    {
        int i = Random.Range(1, 5);
        botspawnerscript.spawnazombie(i);
        return;
    }

	void Die()
    {
        if (this.gameObject.tag == "Enemy")
        {
            thingstodoforzombie();
            Destroy(gameObject);
        }
        else if(this.gameObject.tag=="Environment")
        {
            mapdata.SetTile(x, y, TileType.Empty);
            Destroy(gameObject);
        }
        else if (this.gameObject.tag == "Head")
        {
            Destroy(this.transform.parent.gameObject);
            thingstodoforzombie();
            Debug.Log("Headshot");
        }
        else if(this.gameObject.tag=="Player")
        {
            Destroy(gameObject);
            zombies = GameObject.FindGameObjectsWithTag("Enemy");

            foreach(GameObject go in zombies)
            {
                Destroy(go);
            }
            managerscreenscript.Gameover();
            
        }
        else
        {
            Destroy(gameObject);
        }

	}

    public void Takedamageplayer()
    {
        if (!Damagetaken)
        {
            health--;
            Damagetaken = true;
        }

    }

}
