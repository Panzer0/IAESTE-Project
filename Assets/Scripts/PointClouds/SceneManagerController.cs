using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneManagerController : MonoBehaviour
{
    public string buttonMethod;
    private PCSceneManager PCSceneManagerScript;
    
    public void activateButton()
    {
        switch(this.buttonMethod) 
        {
            case "Advance":
                this.PCSceneManagerScript.Advance();
                break;
            case "Retreat":
                this.PCSceneManagerScript.Retreat();
                break;
            case "StartOver":
                this.PCSceneManagerScript.StartOver();
                break;
            case "Respawn": 
                this.PCSceneManagerScript.Respawn();
                break;
        }  
    }
    

    // Start is called before the first frame update
    void Start()
    {
        this.PCSceneManagerScript = GameObject.Find("PCSceneManager").transform.GetChild(0).gameObject.GetComponent<PCSceneManager>();        
    }
}
