using System;
using UnityEngine;

public class PrevButton : MonoBehaviour
{
    GameObject display;
    GameObject nextButton;
    Vector3 defaultPosition;
    public Material[] slides;

    //TEMPORARY, FOR TESTING PURPOSES
    private GameObject PCRenderer;
    private void OnTriggerEnter(Collider other)
    {
        if (other.name.Contains("ThumbCollider") || other.name.Contains("IndexCollider") || other.name.Contains("MiddleCollider"))
        {
            this.nextButton.transform.localPosition = this.defaultPosition - new Vector3(0, this.nextButton.transform.localScale.y * 0.75f, 0);
            try
            {
                int currentSlide = Int32.Parse(this.display.GetComponent<MeshRenderer>().material.name.Split(" ")[0]);
                if (currentSlide > 0)
                {
                    this.display.GetComponent<MeshRenderer>().material = slides[--currentSlide];
                }
            }
            catch (Exception)
            {
                this.display.GetComponent<MeshRenderer>().material = slides[0];
            }
        }
        if (other.name.Contains("MiddleCollider"))
        {
            this.PCRenderer.GetComponent<ReadPointCloudRuntime>().ClearPointClouds();
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
        this.PCRenderer = GameObject.Find("PCRenderer").transform.GetChild(0).gameObject;
        this.display = GameObject.Find("Display");
        this.nextButton = GameObject.Find("ButtonLeft");
        this.defaultPosition = this.nextButton.transform.localPosition;
    }

    private void Update()
    {
    }
}
