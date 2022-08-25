using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;

public class ButtonObject : MonoBehaviour
{
    public static ButtonObject Instance = null;

    public int testIndex = 0;
    public int sceneIndex = 0;
    Random randomizer = new Random();
    int startingScene = 0;


    public GameObject canvases = null;
    public Results results= null;
    public Text message = null;

    public GameObject[] scenes = null;


    void Awake(){
        if (Instance != null && Instance != this) 
        { 
            Destroy(this); 
        } 
        else 
        { 
            Instance = this; 
        } 

        message =  GameObject.FindGameObjectWithTag("Message").GetComponent<Text>();
        results =  GameObject.FindGameObjectWithTag("Results").GetComponent<Results>();
        canvases = GameObject.FindGameObjectWithTag("Questions");
        
        //canvases.SetActive(false);
        scenes = GameObject.FindGameObjectsWithTag("Scene");

        startingScene = randomizer.Next(1, 3);
    }


    public void LoadTest1()
    {
        if(sceneIndex == 0){
            canvases.SetActive(true);
            message.text = "Each mesh has a different quality. Use the scales to rate them from worst to best. You can only get closer to objects A, B and C. Try to guess which mesh does the furthest object ressemble to by clicking on either A, B or C. Once you're sure of your answers, click on the red sphere to confirm.";
            sceneIndex = startingScene;

        }else if(sceneIndex == 1){
            results.SaveResultsTest1("Part1");
            Destroy(scenes[0]);

            if(startingScene == 1){
                sceneIndex = 2; 
            }else{
                canvases.SetActive(false);  
                message.text = "Good job! You can now take off your headset and answer the follow-up questionnaire.";              
                sceneIndex = 3;
            }
     
        }else if(sceneIndex == 2){
            results.SaveResultsTest1("Part2");
            Destroy(scenes[1]);
            if(startingScene == 2){
                sceneIndex = 1; 
            }else{
                canvases.SetActive(false);  
                message.text = "Good job! You can now take off your headset and answer the follow-up questionnaire.";              
                sceneIndex = 3;
            }
        }
    }

    public void LoadTest2()
    {
        if(sceneIndex == 0){
            canvases.SetActive(true);
                sceneIndex = startingScene;

        }else if(sceneIndex == 1){
            results.SaveResultsTest2("Part1");
            Destroy(scenes[0]);
            if(startingScene == 1){
                sceneIndex = 2; 
            }else{
                canvases.SetActive(false); 
                message.text = "Good job! You can now take off your headset and answer the follow-up questionnaire.";              
                sceneIndex = 3;
            }
     
        }else if(sceneIndex == 2){
            results.SaveResultsTest2("Part2");
            Destroy(scenes[1]); 
            if(startingScene == 2){
                sceneIndex = 1; 
            }else{
                canvases.SetActive(false); 
                message.text = "Good job! You can now take off your headset and answer the follow-up questionnaire.";              
                sceneIndex = 3;
            }
        }
    }

    public void LoadTest3()
    {
        if(sceneIndex == 0){
            canvases.SetActive(true);
            sceneIndex++;
        }else if(sceneIndex == 1){
            results.SaveResultsTest3("Part1");
            Destroy(scenes[0]);
            sceneIndex++;      
        }else if(sceneIndex == 2){
            results.SaveResultsTest3("Part2");
            Destroy(scenes[1]);               
            sceneIndex++;    
        }else if(sceneIndex == 3){
            results.SaveResultsTest3("Part3");
            Destroy(scenes[2]);               
            sceneIndex++;    
        }else if(sceneIndex == 4){
            results.SaveResultsTest3("Part4");
            Destroy(scenes[3]);               
            sceneIndex++;  
        }else if(sceneIndex == 5){
            results.SaveResultsTest3("Part5");
            Destroy(scenes[4]);               
            sceneIndex++;   
        }else if(sceneIndex == 6){
            results.SaveResultsTest3("Part6");
            Destroy(scenes[5]);
            message.text = "Good job! You can now take off your headset and answer the follow-up questionnaire.";
            canvases.SetActive(false);                
            sceneIndex++;    
        }
    }
}
