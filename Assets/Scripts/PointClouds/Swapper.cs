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
    private bool reverse = false;

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

    // todo: Fix the odd stutter during the reversal step
    void Advance()
    {
        cycleIndex %= indices.Count;
        if (!this.reverse)
        {
            this.PCSceneManagerScript.Swap(indices[cycleIndex], indices[(cycleIndex + 1) % indices.Count]);
        }
        else
        {
            print($"Going to swap {(indices.Count - cycleIndex - 1) % indices.Count} and {(indices.Count - cycleIndex) % indices.Count} while the count is {indices.Count}");
            this.PCSceneManagerScript.Swap(indices[(indices.Count - cycleIndex - 1) % indices.Count], indices[(indices.Count - cycleIndex) % indices.Count]);
        }
        if(cycleIndex == indices.Count -1)
        {
            this.reverse = !this.reverse;
            
        }
        cycleIndex++;
    }
}