using UnityEngine;
using System.Collections.Generic;

// Starting in 2 seconds.
// a projectile will be launched every 0.3 seconds

public class Swapper : MonoBehaviour
{
    private PCSceneManager PCSceneManagerScript;
    // The indices of the objects to cycle through
    public List<int> indices;
    // The amount of seconds it takes to cycle through the requested objects
    public float cycleSeconds = 10;
    private int cycleIndex = 0;
    private bool paused = true;

    void Start()
    {
        this.PCSceneManagerScript = GameObject.Find("SceneManagerScript").GetComponent<PCSceneManager>();
    }

    public void PauseCycle()
    {
        this.CancelInvoke();
        this.paused = true;
    }
    public void ResumeCycle()
    {
        this.InvokeRepeating(nameof(Advance), cycleSeconds / indices.Count, cycleSeconds / indices.Count);
        this.paused = false;
    }

    public void UpdateParameters(List<int> indices, float cycleSeconds)
    {
        this.PauseCycle();
        this.cycleIndex = 0;
        this.cycleSeconds = cycleSeconds;
        this.indices = indices;
        this.ResumeCycle();
    }
    public void TogglePaused()
    {
        if (paused)
        {
            ResumeCycle();
        }
        else
        {
            PauseCycle();
        }
    }


    void Advance()
    {
        cycleIndex %= indices.Count;
        this.PCSceneManagerScript.Swap(indices[cycleIndex], indices[(cycleIndex + 1) % indices.Count]);
        cycleIndex++;
    }
}