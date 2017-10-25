using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomeScreenMgr : MonoBehaviour
{
    public GameObject[] thingstoenable;
    public GameObject Playercanvas;
    public GameObject[] panels;
	// Use this for initialization

	void Start ()
    {
        panels[0].SetActive(true);
    }

    #region panel1
    
    public void startwithwhat()
    {
        panels[0].SetActive(false);
        panels[1].SetActive(true);

    }

    public void options()
    {
        //yet to implement

    }

    public void quitgame()
    {
        Application.Quit();
    }

    #endregion

    #region panel2

    public void chooseyourprefer()
    {
        foreach(GameObject go in panels)
        {
            go.SetActive(false);
        }
        panels[2].SetActive(true);
    }

    public void onlinemode()
    {
        //yet to be done

    }

    public void back()
    {
        panels[1].SetActive(false);
        panels[0].SetActive(true);

    }

    #endregion

    #region panel3

    public void mapbutton()
    {
        //yet to be done


    }

    public void startgame()
    {
        Camera.main.GetComponent<CameraFollow>().enabled = true;
        Camera.main.GetComponent<AudioListener>().enabled = false;

        foreach (GameObject go in panels)
        {
            go.SetActive(false);
        }

        foreach (GameObject go in thingstoenable)
        {
            go.SetActive(true);
        }

        Instantiate(Playercanvas, this.transform.position,this.transform.rotation);
        //Debug.Log("done");
    }

    public void character()
    {
        //yet to be done


    }

    public void backtopanel2()
    {
        panels[2].SetActive(false);
        panels[1].SetActive(true);
    }

    #endregion

    #region panel4

    //playagain -- previous fnc used

    public void exittomenu()
    {
        panels[3].SetActive(false);
        panels[0].SetActive(true);
    }

    //quit -- previous fnc used

    #endregion

    public void Gameover()
    {
        GameObject controls = GameObject.FindGameObjectWithTag("OnscreenControls");
        Destroy(controls);
        Camera.main.GetComponent<CameraFollow>().enabled = false;
        Camera.main.GetComponent<AudioListener>().enabled = true;

        //do the opp since gameover
        foreach (GameObject go in thingstoenable)
        {
            go.SetActive(false);
        }

        panels[3].SetActive(true);

    }

}
