using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Profiling;


public class SaveStats : MonoBehaviour
{
    public int particleDivisor = 8;
    public int objectCount = 2;
    string fileName = "Assets/Files/Statistics/"; // file pathname;
    string statsText;


    ProfilerRecorder mainThreadTimeRecorder;
    ProfilerRecorder TrianglesRecorder;
    ProfilerRecorder VerticesRecorder;

    float timestamp = 0f;
    float distanceFromObject;
    public double fps = 0;
    //    GameObject player;
    //    GameObject longdress;

    static double GetRecorderFrameAverage(ProfilerRecorder recorder)
    {
        var samplesCount = recorder.Capacity;
        if (samplesCount == 0)
            return 0;

        double r = 0;
        var samples = new List<ProfilerRecorderSample>(samplesCount);
        recorder.CopyTo(samples);
        for (var i = 0; i < samples.Count; ++i)
            r += samples[i].Value;
        r /= samplesCount;

        return r;
    }


    public int index = 0;
    public bool reset = false;
    public int reset_index = 0;

    // Start is called before the first frame update
    void SetIndex()
    {
        if (reset)
        {
            PlayerPrefs.SetInt("index", reset_index);
        }
        else
        {
            index = PlayerPrefs.GetInt("index");
            index++;
            PlayerPrefs.SetInt("index", index);
        }
    }



    void Start()
    {
        SetIndex();

        fileName += "StatsIndex" + PlayerPrefs.GetInt("index") + "-d" + this.particleDivisor + "-n" + objectCount + ".csv";
        System.IO.File.WriteAllText(fileName, "Timestamp(s),FPS,RenderTime(ms),Triangles,Vertices,Distance\n");

        //    longdress = GameObject.Find("longdress");
        //    player = GameObject.Find("Main Camera");

        StartCoroutine(Delayrendering());
    }

    void OnEnable()
    {
        mainThreadTimeRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Main Thread", 15);
        VerticesRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Vertices Count");
        TrianglesRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Triangles Count");
    }

    void OnDisable()
    {
        mainThreadTimeRecorder.Dispose();
        TrianglesRecorder.Dispose();
        VerticesRecorder.Dispose();
    }

    void Update()
    {

        timestamp += Time.deltaTime;
        fps = 1 / GetRecorderFrameAverage(mainThreadTimeRecorder) * (1e9f);
    }

    void record()
    {
        long triangles = TrianglesRecorder.LastValue;
        long vertices = VerticesRecorder.LastValue;
        double time = GetRecorderFrameAverage(mainThreadTimeRecorder) * (1e-6f);
        distanceFromObject = 0;
        //        distanceFromObject = Vector3.Distance(player.transform.position, longdress.transform.position);


        statsText = string.Format("{0}, {1}, {2}, {3}, {4}, {5}", timestamp, fps, time, triangles, vertices, distanceFromObject);
        statsText += "\n";
        if ((triangles != 0) && (vertices != 0) && (time != 0))
        {
            System.IO.File.AppendAllText(fileName, statsText);
        }
    }
    IEnumerator Delayrendering()
    {
        while (true)
        {
            record();
            yield return new WaitForSeconds(0.5f);

        }


    }

}