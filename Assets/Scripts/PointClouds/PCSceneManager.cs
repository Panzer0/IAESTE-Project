using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PCSceneManager : MonoBehaviour
{
    // todo: Figure out a way to apply a value to this through Unity, hard coding makes this messy
    public List<List<string>> scenes;
    public int testIndex = 0;
    public int sceneIndex = -1;

    public float defaultHeight = 0.1f;
    private GameObject PCRenderer;
    private GameObject canvases;
    private Results results;

    private void saveNewResults() 
    {
        switch(testIndex)
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

    private void ShuffleScenes()
    {
        var rand = new System.Random();
        this.scenes = this.scenes.OrderBy(x => rand.Next()).ToList();
    }

    public void Advance()
    {
        if(sceneIndex >= 0) 
        {
            saveNewResults();
        }
        sceneIndex += 1;
        this.PCRenderer.GetComponent<ReadPointCloudRuntime>().ClearPointClouds();
        if (sceneIndex >= this.scenes.Count)
        {
            sceneIndex = -1;
            canvases.SetActive(false);
            //this.ShuffleScenes();
        }
        else 
        {
            this.PCRenderer.GetComponent<ReadPointCloudRuntime>().RenderPointClouds(this.scenes[sceneIndex]);
            canvases.SetActive(true);   
        }
    }

    public void Retreat()
    {
        sceneIndex -= 1;
        this.PCRenderer.GetComponent<ReadPointCloudRuntime>().ClearPointClouds();
        if (sceneIndex < -1)
        {
            sceneIndex = this.scenes.Count - 1;
        }
        
        if(sceneIndex == -1)
        {
            canvases.SetActive(false);
        }
        else
        {
            this.PCRenderer.GetComponent<ReadPointCloudRuntime>().RenderPointClouds(this.scenes[sceneIndex]);
            canvases.SetActive(true);
        }
    }

    public void StartOver()
    {
        sceneIndex = -1;
        this.PCRenderer.GetComponent<ReadPointCloudRuntime>().ClearPointClouds();
    }

    public void Respawn()
    {
        if(sceneIndex >= 0)
        {
            this.PCRenderer.GetComponent<ReadPointCloudRuntime>().ClearPointClouds();
            this.PCRenderer.GetComponent<ReadPointCloudRuntime>().RenderPointClouds(this.scenes[sceneIndex]);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        this.PCRenderer = GameObject.Find("PCRenderer").transform.GetChild(0).gameObject;
        canvases = GameObject.Find("Questions").gameObject;
        results =  GameObject.FindGameObjectWithTag("Results").GetComponent<Results>();
        
        canvases.SetActive(false);
        this.scenes = new();
        switch(this.testIndex)
        {
            case 1:
                this.scenes.Add(new() { "longdress.ply 2 " + defaultHeight + " 2 25 550", "longdress.ply -2 " + defaultHeight + " 2 50 550", "longdress.ply -2.7 " + defaultHeight + " -2.7 75 550", "longdress.ply 2 " + defaultHeight + " -2 100 550" });
                this.scenes.Add(new() { "soldier.ply 2 " + defaultHeight + " 2 25 550", "soldier.ply -2 " + defaultHeight + " 2 50 550", "soldier.ply -2.7 " + defaultHeight + " -2.7 75 550", "soldier.ply 2 " + defaultHeight + " -2 100 550" });
                this.scenes.Add(new() { "redandblack.ply 2 " + defaultHeight + " 2 25 550", "redandblack.ply -2 " + defaultHeight + " 2 50 550", "redandblack.ply -2.7 " + defaultHeight + " -2.7 75 550", "redandblack.ply 2 " + defaultHeight + " -2 100 550" });
                this.scenes.Add(new() { "loot.ply 2 " + defaultHeight + " 2 25 550", "loot.ply -2 " + defaultHeight + " 2 50 550", "loot.ply -2.7 " + defaultHeight + " -2.7 75 550", "loot.ply 2 " + defaultHeight + " -2 100 550" });
                break;
            case 2:
                this.scenes.Add(new() { "longdress.ply 2 " + defaultHeight + " 2 25 550", "longdress.ply -2 " + defaultHeight + " 2 50 550", "longdress.ply -2.7 " + defaultHeight + " -2.7 75 550", "longdress.ply 2 " + defaultHeight + " -2 100 550" });
                this.scenes.Add(new() { "soldier.ply 2 " + defaultHeight + " 2 25 550", "soldier.ply -2 " + defaultHeight + " 2 50 550", "soldier.ply -2.7 " + defaultHeight + " -2.7 75 550", "soldier.ply 2 0 -2 100 550" });
                this.scenes.Add(new() { "redandblack.ply 2 " + defaultHeight + " 2 25 550", "redandblack.ply -2 " + defaultHeight + " 2 50 550", "redandblack.ply -2.7 " + defaultHeight + " -2.7 75 550", "redandblack.ply 2 " + defaultHeight + " -2 100 550" });
                this.scenes.Add(new() { "loot.ply 2 " + defaultHeight + " 2 25 550", "loot.ply -2 " + defaultHeight + " 2 50 550", "loot.ply -2.7 " + defaultHeight + " -2.7 75 550", "loot.ply 2 " + defaultHeight + " -2 100 550" });
                break;
            case 3:
                this.scenes.Add(new() { "longdress.ply 2 " + defaultHeight + " 2 25 550", "loot.ply -2 " + defaultHeight + " 2 50 550"});
                this.scenes.Add(new() { "soldier.ply 2 " + defaultHeight + " 2 25 550", "longdress.ply -2 " + defaultHeight + " 2 50 550"});
                this.scenes.Add(new() { "soldier.ply 2 " + defaultHeight + " 2 25 550", "redandblack.ply -2 " + defaultHeight + " 2 50 550"});
                this.scenes.Add(new() { "loot.ply 2 " + defaultHeight + " 2 25 550", "redandblack.ply -2 " + defaultHeight + " 2 50 550"});
                break;

        }
        this.ShuffleScenes();
        this.Advance();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
