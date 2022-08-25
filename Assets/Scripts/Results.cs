using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using System.IO;

public class Results : MonoBehaviour
{
    private string fileName = "Assets/Files/Results/";
    public string TesterName = "Harry Potter";
    public string TestName = "Test3";

    private string output= "";
    public Slider[] sliders = null;
    public Button[] buttons = null;

    private int selection=0;


    void Start()
    {
        fileName += TestName + ".csv";
        sliders = FindObjectsOfType<Slider>(true);
        buttons = FindObjectsOfType<Button>(true);
        {
            
        }
    }

    public void SetResultValueA(){
        selection = 1;
    }
    public void SetResultValueB(){
        selection = 2;
    }
    public void SetResultValueC(){
        selection = 3;
    }

    public void SaveResultsTest1(string part)
    {
        output = string.Format("{0}, {1}", TesterName, part);
        //Save and reset slider values
        for(int i= 0; i < sliders.Length ; i++ ){
            output+= ", " + sliders[i].name + ", " + sliders[i].value;
            sliders[i].value = 0;
        }
        //Save and reset button values
        output+= ", " + selection;
        output += "\n";
        System.IO.File.AppendAllText(fileName, output);

        selection = 0;

        for (int i= 0; i < buttons.Length ; i++ ){
            buttons[i].interactable = false;
            buttons[i].interactable = true;
        }


    }

    public void SaveResultsTest2(string part)
    {
        output = string.Format("{0}, {1}", TesterName, part);
        //Save and reset slider values
        for(int i= 0; i < sliders.Length ; i++ ){
            output+= ", " + sliders[i].name + ", " + sliders[i].value;
            sliders[i].value = 0;
        }
        output += "\n";
        System.IO.File.AppendAllText(fileName, output);
    }

    public void SaveResultsTest3(string part)
    {
        output = string.Format("{0}, {1}, {2}", TesterName, part, selection);
        output += "\n";
        System.IO.File.AppendAllText(fileName, output);
        selection = 0;
        for (int i= 0; i < buttons.Length ; i++ ){
            buttons[i].interactable = false;
            buttons[i].interactable = true;
        }
    }
}
