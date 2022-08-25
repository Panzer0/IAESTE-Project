using UnityEngine;

public class RotateScript : MonoBehaviour
{
    Rigidbody rb;
    Vector3 m_EulerAngleVelocity;

    Vector3 hardCodedCentre;
    Vector3 properCentre;
    void Start()
    {
        //Fetch the Rigidbody from the GameObject with this script attached
        rb = GetComponent<Rigidbody>();

        //Set the angular velocity of the Rigidbody (rotating around the Y axis, 100 deg/sec)
        m_EulerAngleVelocity = new Vector3(0, 100, 0);

        //print("transform.position = " + rb.transform.position + "\n centre of mass = " + rb.centerOfMass + "\n summed up = "
        //    + (rb.centerOfMass + rb.transform.position) + "\n correct: 1.60000002f, 0.850000024f, -1.5f");
        hardCodedCentre = new Vector3(1.60000002f, 0.850000024f, -1.5f);
        properCentre = rb.centerOfMass + rb.transform.position;

        print("Hard: " + hardCodedCentre + "\nProper: " + properCentre);
    }

    void FixedUpdate()
    {
        //Quaternion deltaRotation = Quaternion.Euler(m_EulerAngleVelocity * Time.fixedDeltaTime);
        //m_Rigidbody.MoveRotation(m_Rigidbody.rotation * deltaRotation);
        Quaternion q = Quaternion.AngleAxis(360 * Mathf.Sin(Time.deltaTime), Vector3.up);
        rb.MovePosition(q * (rb.transform.position - rb.centerOfMass + rb.transform.position) + rb.centerOfMass + rb.transform.position);
        rb.MoveRotation(rb.transform.rotation * q);
    }
}