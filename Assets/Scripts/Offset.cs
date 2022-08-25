using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Offset : MonoBehaviour
{
    private bool oriented = false;
    private GameObject cam = null;

    void Start()
    {
        cam = GameObject.FindWithTag("MainCamera");
    }

    // Update is called once per frame
    void Update()
    {
        if (!oriented && cam.transform.rotation != Quaternion.Euler(Vector3.zero)){
            transform.Rotate(0.0f, - cam.transform.eulerAngles[1], 0.0f, Space.Self);
            transform.Translate(- cam.transform.position.x, 0.0f, - cam.transform.position.z, Space.World);
            oriented = true;
        }
    }
}
