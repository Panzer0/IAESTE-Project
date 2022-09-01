using SG;
using System;
using System.Collections.Generic;
using System.Linq;
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

    private Transform cameraTransform;
    
    private Vector3 thinVector;
    private Vector3 defaultVector;
    private Vector3 thickVector;
    
    private float rightHandStartY = 0;
    private float rightHandStartX = 0;

    private bool isActive = true;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Interactable") || other.CompareTag("Button") || other.CompareTag("HapticButton") || other.CompareTag("SliderButton"))
        {
            triggerPointer.GetComponent<MeshRenderer>().material = activeMaterial;
            if (other.CompareTag("Interactable"))
            {
                Vector3 rightHandStartPosition = rightHand.transform.position;
                rightHandStartY = rightHandStartPosition.y;

                // Find relative position
                this.cameraTransform = mainCamera.transform;
                Vector3 relativePos = cameraTransform.InverseTransformPoint(rightHand.transform.position);

                // Get the angle in radians for cosine
                float cosAngle = Mathf.Atan2(relativePos.z, relativePos.x);

                // Get offset on object A's x axis
                rightHandStartX = Mathf.Cos(cosAngle) * relativePos.magnitude;
            }
            if (other.CompareTag("SliderButton"))
            {
                if (!leftGunSingleGesture.IsGesturing && leftFingerPointGesture.IsGesturing)
                {
                    int sliderSegment = Convert.ToInt32(other.name.Split(" ")[1]);
                    other.transform.parent.transform.parent.GetComponent<Slider>().value = sliderSegment;
                }
            }
        }
    }
    /*
     * Removes all of the forces affecting the object, effectively freezing it.
     * New forces can still influence it normally.
     * If the target's name starts with "Point Cloud" and doesn't have the appropriate constraints, 
     * the method resets the point cloud's rotation and vertical position, effectively making it stationary.
     * The aforementioned doesn't apply to the tests and was historically applied in a context where
     * point clouds had no constraints and could be displaced freely.
     */
    private void MakeStand(Collider other)
    {
        Vector3 targetPosition = other.transform.position;
        targetPosition.y = 0.1f;
        Rigidbody rgbody = other.GetComponent<Rigidbody>();
        // Fixing point clouds if they've been grabbed
        if (other.name.StartsWith("Point Cloud"))
        {
            RigidbodyConstraints stationaryConstraints =
                RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePositionY;
            if (rgbody.constraints != stationaryConstraints)
            {
                other.transform.SetPositionAndRotation(targetPosition, new Quaternion(0, 0, 0, 0));
                rgbody.constraints = stationaryConstraints;
            }
        }
        rgbody.velocity = Vector3.zero;
        rgbody.angularVelocity = Vector3.zero;
        rgbody.angularDrag = 0.05f;
    }
    /*
     * Normalise the passed Vector3 to the <-1, 1> range.
     * A zero vector will remain unaffected.
     */
    private Vector3 NormaliseVector3(Vector3 inputVector)
    {
        if (inputVector.Equals(Vector3.zero))
        {
            return inputVector;
        }
        List<float> list = new() { inputVector.x, 0, inputVector.z };
        List<float> absoluteList = list.Select(x => Math.Abs(x)).ToList();
        list = list.Select(x => x / Math.Abs(absoluteList.Max())).ToList();
        return new Vector3(list[0], list[1], list[2]);
    }

    /*
     *  Scale the object proportionally.
     *  The hand's starting height is calculated when the pointer comes in touch with the object;
     *  as such one can point away and back to the object in order to restart the starting position.
     *  Due to the fact objects affected by a swapper are separate objects, each swap resets the starting position;
     *  a solution to this problem has yet to be found.
     */
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
    /*
     *  Rotate the other object around the other axis. 
     *  The rotation is applied by torque.
     *  The other object must contain a rigidbody component.
     *  Due to the fact the hand movement is calculated relative to the headset's position, 
     *  moving the headset will achieve results opposite of the equivalent hand movement.
     *  The hand's relative starting position is calculated when the pointer comes in touch with the object;
     *  as such one can point away and back to the object in order to restart the starting position.
     *  Due to the fact objects affected by a swapper are separate objects, each swap resets the starting position;
     *  a solution to this problem has yet to be found.
     */
    private void RotateByGesture(Collider other)
    {
        Vector3 relativePos = cameraTransform.InverseTransformPoint(rightHand.transform.position);
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
        // If the other object is a haptic button, i.e. a totem button.
        if (other.CompareTag("HapticButton") && leftGunSingleGesture.GestureStopped && leftFingerPointGesture.IsGesturing)
        {
            other.GetComponent<SceneManagerController>().activateButton();
        }
        // If the other object is an UI button, i.e. a canvas slider or button
        if (other.CompareTag("Button") && leftGunSingleGesture.GestureStopped && leftFingerPointGesture.IsGesturing)
        {
            print("ACTIVATED BUTTON " + other.name);
            Button button = other.GetComponent<Button>();
            ExecuteEvents.Execute(button.gameObject, new BaseEventData(EventSystem.current), ExecuteEvents.submitHandler);
            button.Select();
        }
        // If the other object is an interactable, i.e. contains a rigidbody
        if (other.CompareTag("Interactable"))
        {
            // THUMB UP, INDEX CONTROLS - PUSHING AND PULLING
            bool isThumbUp = rightThumbUpGesture.IsGesturing;
            bool isGunSingle = rightGunSingleGesture.IsGesturing;
            if (isGunSingle)
            {
                // Pull
                other.GetComponent<Rigidbody>().AddForce(NormaliseVector3(other.transform.position - rightHand.transform.position) * 10);
            }
            else if (isThumbUp)
            {
                // Push
                other.GetComponent<Rigidbody>().AddForce(NormaliseVector3(other.transform.position - rightHand.transform.position) * -10);
            }
       
            // THUMB UP, DOUBLE BARRELED FINGER GUN - STAND UP
            bool isGunDouble = rightGunDoubleGesture.IsGesturing;
            if (isGunDouble)
            {
                MakeStand(other);
            }

            // VERTICAL AXIS - SCALING
            ScaleByGesture(other);

            // HORIZONTAL AXIS - ROTATION
            RotateByGesture(other);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Interactable") || other.CompareTag("Button") || other.CompareTag("HapticButton") || other.CompareTag("SliderButton"))
        {
            // If the exit was not a result of the gesture stopping
            if (this.leftFingerPointGesture.IsGesturing)
            {
                // Hide the pointer
                triggerPointer.transform.localScale = this.defaultVector;
                triggerPointer.GetComponent<MeshRenderer>().material = inactiveMaterial;
            }
            if (other.CompareTag("Interactable"))
            {
                // Ensure that objects abandoned in the middle of a rotation gesture continue to rotate
                other.GetComponent<Rigidbody>().angularDrag = 0;
            }
        }
    }
    private void ToggleActive()
    {
        this.isActive = !this.isActive;
        triggerPointer.transform.localScale = this.isActive ? this.defaultVector : Vector3.zero;
    }
    private void ActivatePointer()
    {
        this.isActive = true;
        triggerPointer.transform.localScale = this.defaultVector;
    }
    private void DeactivatePointer()
    {
        this.isActive = false;
        triggerPointer.transform.localScale = Vector3.zero;
    }

    private void Start()
    {
        // Find needed game objects
        this.triggerPointer = GameObject.Find("FingerPointer");
        this.tablet = GameObject.Find("TutorialPad");
        this.mainCamera = GameObject.Find("Main Camera");

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
    }

    // Teleports the tutorial tablet in front of the player's head
    private void SummonTablet()
    {
        Rigidbody tabletRB = this.tablet.GetComponent<Rigidbody>();
        // Move in front of the player's eyes
        this.tablet.transform.position =
            mainCamera.transform.position + (mainCamera.transform.forward * 0.75f);
        // Make vertical
        this.tablet.transform.localRotation = new Quaternion(-0.707106829f, 0, 0, 0.707106829f);
        // Remove all velocity applied to the object
        tabletRB.velocity = Vector3.zero;
        tabletRB.angularVelocity = Vector3.zero;
        // Make rotate around the Y axis
        tabletRB.AddTorque(new Vector3(0, 2, 0), ForceMode.Force);
    }

    private void Update()
    {
        // Summon the tablet if the appropriate gesture is performed
        if (leftThumbHiddenGesture.GestureMade)
        {
            this.SummonTablet();
        }

        // If the 
        if (this.isActive && !leftFingerPointGesture.IsGesturing)
        {
            //print("Turning off because active = " + this.isActive + " and stopped = " + leftFingerPointGesture.GestureStopped );
            DeactivatePointer();
        }
        else if (!this.isActive && leftFingerPointGesture.IsGesturing)
        {
            //print("Turning on because active = " + this.isActive + " and made = " + leftFingerPointGesture.GestureMade);
            ActivatePointer();
        }
    }
}
