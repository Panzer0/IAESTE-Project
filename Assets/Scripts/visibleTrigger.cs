using SG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class visibleTrigger : MonoBehaviour
{
    public Material activeMaterial;
    public Material inactiveMaterial;

    private GameObject leftGestureLayer;
    private GameObject leftGunSingle;
    private SG_BasicGesture leftGunSingleGesture;

    private GameObject rightHand;
    private GameObject rightGestureLayer;
    private SG_BasicGesture rightThumbUpGesture;
    private SG_BasicGesture rightGunSingleGesture;
    private SG_BasicGesture rightGunDoubleGesture;

    private GameObject triggerPointer;

    Vector3 thinVector;
    Vector3 defaultVector;
    Vector3 thickVector;


    private bool isActive = true;

    private void OnTriggerEnter(Collider other)
    {
        if(other.name.StartsWith("Particle System") || other.CompareTag("Interactable"))
        {
            print("TRIG Triggered by " + other.name);
            triggerPointer.GetComponent<MeshRenderer>().material = activeMaterial;
        }
    }

    private void OnTriggerStay(Collider other)
    {
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.name.StartsWith("Particle System") || other.tag == "Interactable")
        {
            triggerPointer.GetComponent<MeshRenderer>().material = inactiveMaterial;
        }
    }
    private void ToggleActive()
    {
        this.isActive = !this.isActive;
        triggerPointer.transform.localScale = this.isActive ? this.defaultVector : Vector3.zero;
        print("Toggled pointer, now it is" + this.triggerPointer.activeSelf);
    }
    private void ActivatePointer()
    {
        this.isActive = true;
        triggerPointer.transform.localScale = this.defaultVector;
        print("Activated pointer, now it is" + this.triggerPointer.activeSelf);
    }
    private void DeactivatePointer()
    {
        this.isActive = false;
        triggerPointer.transform.localScale = Vector3.zero;
        print("Deactivated pointer, now it is" + this.triggerPointer.activeSelf);
    }

    private void Start()
    {
        triggerPointer = GameObject.Find("VisiblePointer");
        leftGestureLayer = GameObject.Find("SGHand Left").transform.Find("Gesture Layer").gameObject;
        leftGunSingle = leftGestureLayer.transform.Find("FingerGunSingle").gameObject;
        leftGunSingleGesture = leftGunSingle.GetComponent<SG_BasicGesture>();


        ToggleActive();

        this.thinVector = new Vector3(0.001f, 5f, 0.001f);
        this.defaultVector = new Vector3(0.005f, 5f, 0.005f);
        this.thickVector = new Vector3(0.01f, 5f, 0.01f);

        print("Start sequence completed");
    }

    private void Update()
    {
        if ((this.isActive && leftGunSingleGesture.GestureStopped) || (!this.isActive && leftGunSingleGesture.GestureMade))
        {
            if(this.isActive && leftGunSingleGesture.GestureStopped)
            {
                print("Turning off because active = " + this.isActive + " and stopped = " + leftGunSingleGesture.GestureStopped );
                DeactivatePointer();
            }
            else if(!this.isActive && leftGunSingleGesture.GestureMade)
            {
                print("Turning on because active = " + this.isActive + " and made = " + leftGunSingleGesture.GestureMade);
                ActivatePointer();
            }
            else
            {
                print("Another thread is messing with my variables");
            }
        }
    }
}
