using SG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class pointerTrigger : MonoBehaviour
{
    public Material activeMaterial;
    public Material inactiveMaterial;

    private GameObject leftGestureLayer;
    private SG_BasicGesture leftGunSingleGesture;
    private SG_BasicGesture leftFingerPointGesture;
    private SG_BasicGesture leftThumbHiddenGesture;

    private GameObject rightHand;
    private GameObject rightGestureLayer;
    private SG_BasicGesture rightThumbUpGesture;
    private SG_BasicGesture rightGunSingleGesture;
    private SG_BasicGesture rightGunDoubleGesture;

    private GameObject triggerPointer;
    private GameObject tablet;
    private GameObject mainCamera;

    Transform cameraTransform;

    Vector3 thinVector;
    Vector3 defaultVector;
    Vector3 thickVector;
    
    private float rightHandStartY = 0;
    private float rightHandStartX = 0;

    private bool isActive = true;

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Interactable"))
        {
            triggerPointer.GetComponent<MeshRenderer>().material = activeMaterial;
            Vector3 rightHandStartPosition = rightHand.transform.position;
            rightHandStartY = rightHandStartPosition.y;

            // Find relative position
            this.cameraTransform = mainCamera.transform;
            var relativePos = cameraTransform.InverseTransformPoint(rightHand.transform.position);

            // Get the angle in radians for cosine
            float cosAngle = Mathf.Atan2(relativePos.z, relativePos.x);

            // Get offset on object A's x axis
            rightHandStartX = Mathf.Cos(cosAngle) * relativePos.magnitude;
        }
        else if(other.CompareTag("Button") || other.CompareTag("HapticButton"))
        {
            triggerPointer.GetComponent<MeshRenderer>().material = activeMaterial;
        }
        else if (other.CompareTag("SliderButton"))
        {
            triggerPointer.GetComponent<MeshRenderer>().material = activeMaterial;  
            if(!leftGunSingleGesture.IsGesturing && leftFingerPointGesture.IsGesturing)
            {
                int sliderSegment = Convert.ToInt32(other.name.Split(" ")[1]);
                other.transform.parent.transform.parent.GetComponent<Slider>().value = sliderSegment;
            }
        }
    }

    private void MakeStand(Collider other)
    {
        Vector3 targetPosition = other.transform.position;
        targetPosition.y = 0.1f;
        Rigidbody rgbody = other.GetComponent<Rigidbody>();
        // Fixing point clouds if they've been grabbed
        if(other.name.StartsWith("Point Cloud"))
        {
            RigidbodyConstraints stationaryConstraints = 
                RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePositionY;
            if(rgbody.constraints != stationaryConstraints)
            {
                other.transform.position = targetPosition;
                other.transform.rotation = new Quaternion(0, 0, 0, 0);
                rgbody.constraints = stationaryConstraints;
            }
        }
        rgbody.velocity = Vector3.zero;
        rgbody.angularVelocity = Vector3.zero;
        rgbody.angularDrag = 0.05f;
    }

    private Vector3 NormaliseVector3(Vector3 inputVector)
    {
        if(inputVector.Equals(Vector3.zero))
        {
            return inputVector;
        }
        List<float> list = new() { inputVector.x, 0, inputVector.z};
        List<float> absoluteList = list.Select(x => Math.Abs(x)).ToList();
        list = list.Select(x => x / Math.Abs(absoluteList.Max())).ToList();
        return new Vector3(list[0], list[1], list[2]);
    }

    private void ScaleByGesture(Collider other)
    {
        float currentY = rightHand.transform.position.y;
        // Dead zone, no interaction
        float handDistance = Math.Abs(currentY - rightHandStartY);
        if (handDistance < 0.1)
        {
            triggerPointer.transform.localScale = this.defaultVector;
        }
        // Above, upscaling
        else if (currentY > rightHandStartY)
        {
            triggerPointer.transform.localScale = this.thickVector;
            other.transform.localScale *= handDistance > 0.25 ? 1.003f : 1.001f;
        }
        // Below, downscaling
        else if (currentY < rightHandStartY)
        {
            triggerPointer.transform.localScale = this.thinVector;
            other.transform.localScale *= handDistance > 0.25 ? 0.997f : 0.999f;
        }
    }

    private void RotateByGesture(Collider other)
    {
        var relativePos = cameraTransform.InverseTransformPoint(rightHand.transform.position);
        float cosAngle = Mathf.Atan2(relativePos.z, relativePos.x);
        float currentOffset = Mathf.Cos(cosAngle) * relativePos.magnitude;

        float xDifference = currentOffset - this.rightHandStartX;

        // Only rotate when outside the dead zone
        if (Math.Abs(xDifference) > 0.1)
        {
            // Force is relative to the distance from the starting point
            float rotForce = xDifference * -3;
            // Increased angular drag accounts for the fact that torque is delivered continuously
            other.GetComponent<Rigidbody>().angularDrag = 10;
            // Apply torque continuously to spin the object
            other.GetComponent<Rigidbody>().AddTorque(new Vector3(0, rotForce, 0), ForceMode.Force);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("HapticButton") && leftGunSingleGesture.GestureStopped && leftFingerPointGesture.IsGesturing)
        {
            other.GetComponent<SceneManagerController>().activateButton();
        }
        if (other.CompareTag("Button") && leftGunSingleGesture.GestureStopped && leftFingerPointGesture.IsGesturing)
        {
            print("ACTIVATED BUTTON " + other.name);
            Button button = other.GetComponent<Button>();
            ExecuteEvents.Execute(button.gameObject, new BaseEventData(EventSystem.current), ExecuteEvents.submitHandler);
            button.Select();
        }
        if (other.CompareTag("Interactable"))
        {
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // THUMB UP, INDEX CONTROLS - PUSHING AND PULLING
            bool isThumbUp = rightThumbUpGesture.IsGesturing;
            bool isGunSingle = rightGunSingleGesture.IsGesturing;
            if (isGunSingle)
            {
                other.GetComponent<Rigidbody>().AddForce(NormaliseVector3(other.transform.position - rightHand.transform.position) * 10);
            }
            else if (isThumbUp)
            {
                other.GetComponent<Rigidbody>().AddForce(NormaliseVector3(other.transform.position - rightHand.transform.position) * -10);
            }
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // THUMB UP, DOUBLE BARRELED FINGER GUN - STAND UP
            bool isGunDouble = rightGunDoubleGesture.IsGesturing;
            if(isGunDouble)
            {
                MakeStand(other);
            }

            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // VERTICAL AXIS - SCALING
            ScaleByGesture(other);

            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // HORIZONTAL AXIS - ROTATION
            RotateByGesture(other);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Interactable"))
        {
            // Hide the pointer
            triggerPointer.transform.localScale = this.defaultVector;
            triggerPointer.GetComponent<MeshRenderer>().material = inactiveMaterial;
            // Ensure that objects abandoned in the middle of a rotation gesture continue to rotate
            other.GetComponent<Rigidbody>().angularDrag = 0;
        }
        if (other.CompareTag("Button") || other.CompareTag("HapticButton"))
        {
            triggerPointer.transform.localScale = this.defaultVector;
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
        // Find needed game objects
        this.triggerPointer = GameObject.Find("FingerPointer");
        this.tablet= GameObject.Find("TutorialPad");
        this.mainCamera= GameObject.Find("Main Camera");

        // Initialise the left hand's gestures
        leftGestureLayer = GameObject.Find("SGHand Left").transform.Find("Gesture Layer").gameObject;
        leftGunSingleGesture = leftGestureLayer.transform.Find("FingerGunSingle").gameObject.GetComponent<SG_BasicGesture>();
        leftFingerPointGesture = leftGestureLayer.transform.Find("FingerPoint").gameObject.GetComponent<SG_BasicGesture>();
        leftThumbHiddenGesture = leftGestureLayer.transform.Find("ThumbHiddenGesture").gameObject.GetComponent<SG_BasicGesture>();

        // Initialise the right hand and its gestures
        rightHand = GameObject.Find("SGHand Right").transform.Find("HandModel").gameObject.transform.Find("HandBones").gameObject;
        rightGestureLayer = GameObject.Find("SGHand Right").transform.Find("Gesture Layer").gameObject;
        rightThumbUpGesture = rightGestureLayer.transform.Find("ThumbsUpGesture").gameObject.GetComponent<SG_BasicGesture>();
        rightGunSingleGesture = rightGestureLayer.transform.Find("FingerGunSingle").gameObject.GetComponent<SG_BasicGesture>();
        rightGunDoubleGesture = rightGestureLayer.transform.Find("FingerGunDouble").gameObject.GetComponent<SG_BasicGesture>();

        ToggleActive();

        this.thinVector = new Vector3(0.00001f, 0.05f, 0.00001f);
        this.defaultVector = new Vector3(0.00005f, 0.05f, 0.00005f);
        this.thickVector = new Vector3(0.0001f, 0.05f, 0.0001f);

        print("Start sequence completed");
    }

    private void SummonTablet()
    {
        Rigidbody tabletRB = this.tablet.GetComponent<Rigidbody>();
        // Remove all velocity applied to the object
        tabletRB.velocity = Vector3.zero;
        tabletRB.angularVelocity = Vector3.zero;
        // Move in front of the player's eyes
        this.tablet.transform.position = 
            mainCamera.transform.position + mainCamera.transform.forward * 0.75f;
        // Make vertical
        this.tablet.transform.localRotation = new Quaternion(-0.707106829f, 0, 0, 0.707106829f);
        // Make rotate around the Y axis
        tabletRB.AddTorque(new Vector3(0, 2, 0), ForceMode.Force);
    }

    private void Update()
    {
        if(leftThumbHiddenGesture.GestureMade)
        {
            this.SummonTablet();
        }
        if ((this.isActive && leftFingerPointGesture.GestureStopped) || (!this.isActive && leftFingerPointGesture.GestureMade))
        {
            if(this.isActive && leftFingerPointGesture.GestureStopped)
            {
                print("Turning off because active = " + this.isActive + " and stopped = " + leftFingerPointGesture.GestureStopped );
                DeactivatePointer();
            }
            else if(!this.isActive && leftFingerPointGesture.GestureMade)
            {
                print("Turning on because active = " + this.isActive + " and made = " + leftFingerPointGesture.GestureMade);
                ActivatePointer();
            }
            else
            {
                print("Another thread is messing with my variables");
            }
        }
    }
}
