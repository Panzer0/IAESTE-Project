using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShowScaleValue : MonoBehaviour
{
    TMP_Text val;
    public Slider slider;

    void Start(){
        val = GetComponent<TMP_Text>();
    }
    public void ShowValue(){
        val.text = slider.value + "/10";
    }
}