using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class PCSceneManager : MonoBehaviour
{
    // todo: Figure out a way to apply a value to this through Unity, hard coding makes this messy
    public List<List<string>> scenes;
    public List<List<string>> swapArguments;
    public int testIndex = 0;
    public int sceneIndex = -1;
    public float defaultHeight = 0.1f;
    public List<GameObject> swapperObjects = null;
    
    private ReadPointCloudRuntime PCRenderer;
    private GameObject canvases;
    private Results results;
    public Text message;

    // The message which will be displayed in the presence of the point clouds
    private string activeInstructions;

    // The pool of activeInstructions messages for different scenes
    private readonly string ACTIVE_INSTRUCTIONS_1 =
                "Point clouds A, B and C have different qualities. " +
                "Use the scales to rate them from worst to best. " +
                "You can only get closer to objects A, B and C. " +
                "Try to guess which object resembles object D the most by clicking on either A, B or C. " +
                "Once you're sure of your answers, perform the \"Confirm\" gesture on the blue totem button to proceed.";

    private readonly string ACTIVE_INSTRUCTIONS_2 =
                "Each of the point clouds has a different set of qualities that it cycles through. " +
                "Use the scales to rate their overall quality and the noticeability of the changes. " +
                "Once you're sure of your answers, perform the \"Confirm\" gesture on the blue totem button to proceed.";

    private readonly string ACTIVE_INSTRUCTIONS_3 =
                "Point clouds A and B represent different subjects. " +
                "Use the buttons to choose the object which you believe to be higher quality. " +
                "If you believe their qualities are equal, click the \"Same\" button. " +
                "Once you're sure of your answers, perform the \"Confirm\" gesture on the blue totem button to proceed.";

    // The message which will be displayed after cycling through each of the point clouds
    private readonly string POST_INSTRUCTIONS =
                "Good job! " +
                "You can now take off your headset and answer the follow-up questionnaire.";
    // The message which will be displayed before starting to cycle through the point clouds
    private readonly string PRE_INSTRUCTIONS =
                "Thank you for taking part in this experiment! " +
                "In order to cycle between mesh scenes, point at the red sphere with your controller. " +
                "Once the light beam turns white, press the select button. " +
                "In order to cycle between point cloud scenes, review the tutorial on the wall to the right, then point at the blue totem button and perform the \"Confirm\" gesture. " +
                "Let's give it a try! ";

    // Saves the results as a .xlx file
    private void SaveNewResults()
    {
        switch (testIndex)
        {
            case 1:
                this.results.SaveResultsTest1("Part" + sceneIndex + 1);
                break;
            case 2:
                this.results.SaveResultsTest2("Part" + sceneIndex + 1);
                break;
            case 3:
                this.results.SaveResultsTest3("Part" + sceneIndex + 1);
                break;
        }
    }

    // Restarts the state of each canvas and/or button
    private void ClearCanvases()
    {
        switch (testIndex)
        {
            case 1:
                this.results.ClearTest1();
                break;
            case 2:
                this.results.ClearTest2();
                break;
            case 3:
                this.results.ClearTest3();
                break;
        }
    }
    
    // Randomly rearranges the list of point clouds
    private void ShuffleScenes()
    {
        var joined = this.scenes.Zip(this.swapArguments, (x, y) => (x, y)).ToList();

        System.Random rand = new();
        var shuffled = joined.OrderBy(x => rand.Next()).ToList();


        this.scenes = shuffled.Select(pair => pair.x).ToList();
        this.swapArguments = shuffled.Select(pair => pair.y).ToList();
    }
    /*
     * Proceeds to the following step of the scene list.
     * Updates class variables, replaces existing point cloud with new ones and updates swappers if any exist
     */
    public void Advance()
    {
        // Save current results
        if (sceneIndex >= 0)
        {
            SaveNewResults();
        }
        sceneIndex++;
        this.PCRenderer.ClearPointClouds();
        
        // All scenes have been processed, the test concludes. 
        if (sceneIndex >= this.scenes.Count)
        {
            sceneIndex--;
            canvases.SetActive(false);
            message.text = POST_INSTRUCTIONS;

            // If the current test utilises swappers
            if (this.swapperObjects != null)
            {
                // Stop each of the swappers
                foreach (GameObject swapperObject in this.swapperObjects)
                {
                    swapperObject.GetComponent<Swapper>().PauseCycle();
                }
            }
        }
        // Some scenes still remain, proceed to the next one
        else 
        {
            this.PCRenderer.RenderPointClouds(this.scenes[sceneIndex]);
            canvases.SetActive(true);
            message.text = activeInstructions;

            // If the current test utilises swappers
            if (this.swapperObjects != null)
            {
                // Give the swappers new directives. The former ones are aborted.
                foreach (var zipped in swapperObjects.Zip(this.swapArguments[sceneIndex], (o, a) => new { swapperObject = o, stringArgs = a }))
                {
                    List<int> newArguments = zipped.stringArgs
                        .Split(' ')
                        .Where(x => int.TryParse(x, out _))
                        .Select(int.Parse)
                        .ToList();
                    zipped.swapperObject.GetComponent<Swapper>().UpdateParameters(newArguments.OfType<int>().Skip(1).ToList(), newArguments[0]);
                }
            }
        }
    }


    // Functions the same as Proceed(), except in the opposite direction and without updating the .xlx file
    public void Retreat()
    {
        sceneIndex -= 1;
        this.PCRenderer.ClearPointClouds();
        if (sceneIndex < -1)
        {
            sceneIndex = -1;
        }

        // All scenes have been reverted, return to the initial state
        if (sceneIndex == -1)
        {
            canvases.SetActive(false);
            this.message.text = PRE_INSTRUCTIONS;

            // If the current test utilises swappers
            if (this.swapperObjects != null)
            {
                // Stop each of the swappers
                foreach (GameObject swapperObject in this.swapperObjects)
                {
                    swapperObject.GetComponent<Swapper>().PauseCycle();
                }
            }
        }
        // Some scenes still remain, proceed to the previous one
        else
        {
            this.PCRenderer.RenderPointClouds(this.scenes[sceneIndex]);
            canvases.SetActive(true);
            ClearCanvases();
            message.text = activeInstructions;

            // If the current test utilises swappers
            if (this.swapperObjects != null)
            {
                // Give the swappers new directives. The former ones are aborted.
                foreach (var zipped in swapperObjects.Zip(this.swapArguments[sceneIndex], (o, a) => (swapperObject: o, stringArgs: a)))
                {
                    List<int> newArguments = zipped.stringArgs
                        .Split(' ')
                        .Where(x => int.TryParse(x, out _))
                        .Select(int.Parse)
                        .ToList();
                    zipped.swapperObject.GetComponent<Swapper>().UpdateParameters(newArguments.OfType<int>().Skip(1).ToList(), newArguments[0]);
                }
            }
        }
    }
    /*
     * Return to the initial state
     * Obsolete due to the removal of looping.
     */
    public void StartOver()
    {
        sceneIndex = -1;
        this.PCRenderer.ClearPointClouds();
        canvases.SetActive(false);
        this.message.text = PRE_INSTRUCTIONS;
    }

    // Clears the existing point clouds and renders them anew
    public void Respawn()
    {
        if (sceneIndex >= 0)
        {
            this.PCRenderer.ClearPointClouds();
            this.PCRenderer.RenderPointClouds(this.scenes[sceneIndex]);
        }
    }

    // Swap the parametres of point clouds at the given indices in a seamless manner
    public void Swap(int indexA, int indexB)
    {
        Transform transformA = this.PCRenderer.existingPointClouds[indexA].transform;
        Transform transformB = this.PCRenderer.existingPointClouds[indexB].transform;
        Rigidbody rigidbodyA = this.PCRenderer.existingPointClouds[indexA].GetComponent<Rigidbody>();
        Rigidbody rigidbodyB = this.PCRenderer.existingPointClouds[indexB].GetComponent<Rigidbody>();

        Vector3 tempScale = transformA.localScale;
        Quaternion tempRotation = transformA.localRotation;
        Vector3 tempPosition = transformA.localPosition;
        Vector3 tempVelocity = rigidbodyA.velocity;
        Vector3 tempAngVelocity = rigidbodyA.angularVelocity;

        transformA.localScale = transformB.localScale;
        transformA.localRotation = transformB.localRotation;
        transformA.localPosition = transformB.localPosition;
        rigidbodyA.velocity = rigidbodyB.velocity;
        rigidbodyA.angularVelocity = rigidbodyB.angularVelocity;

        transformB.localScale = tempScale;
        transformB.localRotation = tempRotation;
        transformB.localPosition = tempPosition;
        rigidbodyB.velocity = tempVelocity;
        rigidbodyB.angularVelocity = tempAngVelocity;
    }

    // Initialises the activeInstructions variable depending on the active test
    private void InitialiseInstructions()
    {
        this.activeInstructions = this.testIndex switch
        {
            1 => ACTIVE_INSTRUCTIONS_1,
            2 => ACTIVE_INSTRUCTIONS_2,
            3 => ACTIVE_INSTRUCTIONS_3,
            _ => ACTIVE_INSTRUCTIONS_1,
        };
    }

    // Initialises the scenes list depending on the active test
    private void InitialiseScenes()
    {
        this.scenes = new();
        switch (this.testIndex)
        {
            case 1:
                this.scenes.Add(new()
                {
                    $"longdress_0001.ply 2 {defaultHeight} 2 25 550",
                    $"longdress_0001.ply -2 {defaultHeight} 2 50 550",
                    $"longdress_0001.ply -2.7 {defaultHeight} -2.7 75 550",
                    $"longdress_0001.ply 2 {defaultHeight} -2 100 550"
                });
                this.scenes.Add(new()
                {
                    $"soldier_0001.ply 2 {defaultHeight} 2 25 550",
                    $"soldier_0001.ply -2 {defaultHeight} 2 50 550",
                    $"soldier_0001.ply -2.7 {defaultHeight} -2.7 75 550",
                    $"soldier_0001.ply 2 {defaultHeight} -2 100 550"
                });
                this.scenes.Add(new()
                {
                    $"redandblack_0001.ply 2 {defaultHeight} 2 25 550",
                    $"redandblack.ply -2 {defaultHeight} 2 50 550",
                    $"redandblack.ply -2.7 {defaultHeight} -2.7 75 550",
                    $"redandblack.ply 2 {defaultHeight} -2 100 550"
                });
                this.scenes.Add(new()
                {
                    $"loot.ply 2 {defaultHeight} 2 25 550",
                    $"loot.ply -2 {defaultHeight} 2 50 550",
                    $"loot.ply -2.7 {defaultHeight} -2.7 75 550",
                    $"loot.ply 2 {defaultHeight} -2 100 550"
                });
                break;
            case 2:
                this.scenes.Add(new()
                {
                    $"longdress.ply 2 {defaultHeight} 2 25 550",
                    $"longdress.ply -2 {defaultHeight} 2 50 550",
                    $"longdress.ply -2 {defaultHeight} -3 75 550",
                    $"longdress.ply 2 {defaultHeight} -2 100 550",
                    $"longdress.ply 2 {defaultHeight + 1000} -2 15 550",
                    $"longdress.ply 2 {defaultHeight + 1000} -2 150 550",
                    $"longdress.ply 2 {defaultHeight + 1000} -2 15 550",
                    $"longdress.ply 2 {defaultHeight + 1000} -2 90 550",
                    $"longdress.ply 2 {defaultHeight + 1000} -2 20 550"
                });
                this.scenes.Add(new()
                {
                    $"soldier.ply 2 {defaultHeight} 2 25 550",
                    $"soldier.ply -2 {defaultHeight} 2 50 550",
                    $"soldier.ply -2 {defaultHeight} -3 75 550",
                    $"soldier.ply 2 {defaultHeight} -2 100 550",
                    $"soldier.ply 2 {defaultHeight + 1000} -2 15 550",
                    $"soldier.ply 2 {defaultHeight + 1000} -2 80 550",
                    $"soldier.ply 2 {defaultHeight + 1000} -2 55 550",
                    $"soldier.ply 2 {defaultHeight + 1000} -2 15 550",
                    $"soldier.ply 2 {defaultHeight + 1000} -2 200 550"
                });
                this.scenes.Add(new()
                {
                    $"redandblack.ply 2 {defaultHeight} 2 25 550",
                    $"redandblack.ply -2 {defaultHeight} 2 50 550",
                    $"redandblack.ply -2 {defaultHeight} -3 75 550",
                    $"redandblack.ply 2 {defaultHeight} -2 100 550",
                    $"redandblack.ply 2 {defaultHeight + 1000} -2 15 550",
                    $"redandblack.ply 2 {defaultHeight + 1000} -2 20 550",
                    $"redandblack.ply 2 {defaultHeight + 1000} -2 40 550",
                    $"redandblack.ply 2 {defaultHeight + 1000} -2 10 550",
                    $"redandblack.ply 2 {defaultHeight + 1000} -2 60 550"
                });
                this.scenes.Add(new()
                {
                    $"loot.ply 2 {defaultHeight} 2 25 550",
                    $"loot.ply -2 {defaultHeight} 2 50 550",
                    $"loot.ply -2 {defaultHeight} -3 75 550",
                    $"loot.ply 2 {defaultHeight} -2 100 550",
                    $"loot.ply 2 {defaultHeight + 10000} -2 15 550",
                    $"loot.ply 2 {defaultHeight + 1000} -2 500 550",
                    $"loot.ply 2 {defaultHeight + 1000} -2 60 550",
                    $"loot.ply 2 {defaultHeight + 1000} -2 15 550",
                    $"loot.ply 2 {defaultHeight + 1000} -2 80 550"
                });
                break;
            case 3:
                this.scenes.Add(new()
                {
                    $"longdress.ply 2 {defaultHeight} 2 25 550",
                    $"loot.ply -2 {defaultHeight} 2 50 550"
                });
                this.scenes.Add(new()
                {
                    $"soldier.ply 2 {defaultHeight} 2 25 550",
                    $"longdress.ply -2 {defaultHeight} 2 50 550"
                });
                this.scenes.Add(new()
                {
                    $"soldier.ply 2 {defaultHeight} 2 25 550",
                    $"redandblack.ply -2 {defaultHeight} 2 50 550"
                });
                this.scenes.Add(new()
                {
                    $"loot.ply 2 {defaultHeight} 2 25 550",
                    $"redandblack.ply -2 {defaultHeight} 2 50 550"
                });
                break;
        }

    }


    // Initialises the swapArguments list depending on the active test
    private void InitialiseSwapArguments()
    {
        this.swapArguments = new();
        this.swapArguments.Add(new()
        {
            "10 0 4",
            "10 1 5",
            "10 2 6",
            "10 3 7 8"
        });
        this.swapArguments.Add(new()
        {
            "10 0 4",
            "10 1 5",
            "10 2 6 8",
            "10 3 7"
        });
        this.swapArguments.Add(new()
        {
            "10 0 4",
            "10 1 5 8",
            "10 2 6",
            "10 3 7"
        });
        this.swapArguments.Add(new()
        {
            "10 0 4",
            "10 1 5 8",
            "10 2 6",
            "10 3 7"
        });
    }

    // Start is called before the first frame update
    void Start()
    {
        this.PCRenderer = GameObject.Find("RendererScript").GetComponent<ReadPointCloudRuntime>();
        canvases = GameObject.Find("Questions");
        results = GameObject.FindGameObjectWithTag("Results").GetComponent<Results>();

        canvases.SetActive(false);

        this.InitialiseInstructions();
        this.InitialiseScenes();
        this.InitialiseSwapArguments();

        this.ShuffleScenes();
        this.Advance();
    }
}
