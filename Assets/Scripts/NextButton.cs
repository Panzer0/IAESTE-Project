using System;
using System.Collections.Generic;
using UnityEngine;

public class NextButton : MonoBehaviour
{
    GameObject display;
    GameObject nextButton;
    Vector3 defaultPosition;
    public Material[] slides;



    //TEMPORARY, FOR TESTING PURPOSES
    //private GameObject PCRenderer;
    private GameObject PCSceneManager;
    
    private void OnTriggerEnter(Collider other)
    {
        if(other.name.Contains("ThumbCollider") || other.name.Contains("IndexCollider") || other.name.Contains("MiddleCollider"))
        {
            
            this.nextButton.transform.localPosition = this.defaultPosition - new Vector3(0, this.nextButton.transform.localScale.y * 0.75f, 0);
            try
            {
                int currentSlide = Int32.Parse(this.display.GetComponent<MeshRenderer>().material.name.Split(" ")[0]);
                if (currentSlide < slides.Length - 1)
                {
                    this.display.GetComponent<MeshRenderer>().material = slides[++currentSlide];
                }
            }
            catch (Exception)
            {
                this.display.GetComponent<MeshRenderer>().material = slides[0];
            }
        }
        if (other.name.Contains("MiddleCollider"))
        {
            this.PCSceneManager.GetComponent<PCSceneManager>().Advance();
            //List<String> requests = new()
            //{ "longdress.ply 2 0 2 25 550", "longdress.ply -2 0 2 50 550", "redandblack.ply -2.7 0 -2.7 75 550", "longdress.ply 2 0 -2 100 550" };
            //this.PCRenderer.GetComponent<ReadPointCloudRuntime>().RenderPointClouds(requests);
        }
    }



    private void OnTriggerStay(Collider other)
    {
    }

    private void OnTriggerExit(Collider other)
    {
        this.nextButton.transform.localPosition = this.defaultPosition;
    }

    private void Start()
    {
        this.PCSceneManager = GameObject.Find("PCSceneManager").transform.GetChild(0).gameObject;
        //this.PCRenderer = GameObject.Find("PCRenderer").transform.GetChild(0).gameObject;
        this.display = GameObject.Find("Display");
        this.nextButton = GameObject.Find("ButtonRight");
        this.defaultPosition = this.nextButton.transform.localPosition;
        GameObject tablet = GameObject.Find("TutorialPad");
        tablet.GetComponent<Rigidbody>().AddTorque(new Vector3(0, 2, 0), ForceMode.Force);
    }

    private void Update()
    {
    }
}
