using System;
using UnityEngine;

public class NextButton : MonoBehaviour
{
    GameObject display;
    GameObject nextButton;
    Vector3 defaultPosition;
    public Material[] slides;
    public GameObject extraDisplay;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.name.Contains("ThumbCollider") || other.name.Contains("IndexCollider") || other.name.Contains("MiddleCollider"))
        {
            this.nextButton.transform.localPosition = this.defaultPosition - new Vector3(0, this.nextButton.transform.localScale.y * 0.75f, 0);
            try
            {
                int currentSlide = int.Parse(this.display.GetComponent<MeshRenderer>().material.name.Split(" ")[0]);
                if (currentSlide < slides.Length - 1)
                {
                    this.display.GetComponent<MeshRenderer>().material = slides[++currentSlide];
                    if (this.extraDisplay != null)
                    {
                        this.extraDisplay.GetComponent<MeshRenderer>().material = slides[currentSlide];
                    }
                }
            }
            catch (Exception)
            {
                this.display.GetComponent<MeshRenderer>().material = slides[0];
                if (this.extraDisplay != null)
                {
                    this.extraDisplay.GetComponent<MeshRenderer>().material = slides[0];
                }
            }
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
