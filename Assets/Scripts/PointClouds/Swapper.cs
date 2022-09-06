using UnityEngine;
using System.Collections.Generic;

public class Swapper : MonoBehaviour
{
    private PCSceneManager PCSceneManagerScript;
    // The indices of the objects to cycle through
    public List<int> indices;
    // The amount of seconds it takes to cycle through the requested objects
    public float cycleSeconds = 10;
    private float frequency;
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
        this.InvokeRepeating(nameof(Advance), this.frequency, this.frequency);
        this.paused = false;
    }

    public void UpdateParameters(List<int> indices, float cycleSeconds)
    {
        this.PauseCycle();
        this.cycleIndex = 0;
        this.cycleSeconds = cycleSeconds;
        this.indices = indices;
        this.frequency = cycleSeconds / indices.Count;
        List<int> reversedCenter = indices.GetRange(1, indices.Count - 1);
        reversedCenter.Reverse();
        this.indices.AddRange(reversedCenter);
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
        this.PCSceneManagerScript.Swap(indices[cycleIndex], indices[(cycleIndex + 1) % indices.Count]);
        cycleIndex++;
    }
}