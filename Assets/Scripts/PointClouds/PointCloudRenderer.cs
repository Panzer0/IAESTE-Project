using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.VFX;
 
public class PointCloudRenderer : MonoBehaviour {
    VisualEffect vfx;
    readonly uint resolution = 1400;
    public float particleSize = 1f;
    public int frame = 0;
    public int frames = 1;
    readonly List<(Texture2D texColor, Texture2D texPosScale, uint particleCount)> textures = new();

    public (List<Vector3> positions, List<Color> colors) ReadPointCloud(string name) {
        List<Vector3> positions = new();
        List<Color> colors = new();
        StreamReader inp = new(@name);
        bool t = false;
        //var i = 0;
        while (!inp.EndOfStream)
        {
            if (!t)
            {
                if (inp.ReadLine().Contains("end_header"))
                {
                    t = true;
                }
            }
            else
            {
                string line = inp.ReadLine();
                //i++;
                //if (i%2 ==0){
                string[] values = line.Split(' ');
                float x = float.Parse(values[0]);
                float y = float.Parse(values[1]);
                float z = float.Parse(values[2]);
                Vector3 pos = new(x, y, z);
                positions.Add(pos);
                float r = float.Parse(values[3]) / 255.0f;
                float g = float.Parse(values[4]) / 255.0f;
                float b = float.Parse(values[5]) / 255.0f;
                Color col = new(r, g, b);
                colors.Add(col);
                /*}
                else{
                    continue;
                }*/
            }
        }
        inp.Close();
        return (positions, colors);
    }

    public (Texture2D texColor, Texture2D texPosScale, uint particleCount) Generate(List<Vector3> positions, List<Color> colors)
    {
        Texture2D texColor = new(positions.Count > (int)resolution ? (int)resolution : positions.Count, Mathf.Clamp(positions.Count / (int)resolution, 1, (int)resolution), TextureFormat.RGBAFloat, false);
        Texture2D texPosScale = new(positions.Count > (int)resolution ? (int)resolution : positions.Count, Mathf.Clamp(positions.Count / (int)resolution, 1, (int)resolution), TextureFormat.RGBAFloat, false);
        int texWidth = texColor.width;
        int texHeight = texColor.height;
        Debug.Log("Length " + positions.Count.ToString());
        Debug.Log("Length " + colors.Count.ToString());
        for (int y = 0; y < texHeight; y++)
        {
            for (int x = 0; x < texWidth; x++)
            {
                int index = x + (y * texWidth);
                texColor.SetPixel(x, y, colors[index]);
                Color data = new Color(positions[index].x, positions[index].y, positions[index].z, particleSize);
                texPosScale.SetPixel(x, y, data);
            }
        }
        texColor.Apply();
        texPosScale.Apply();
        uint particleCount = (uint)positions.Count;
        return (texColor, texPosScale, particleCount);
    }

    public void RestartPointCloud(string newName)
    {
        this.name = newName;
        this.frame = 0;
        this.textures.Clear();
    }
 
    private void Start() {
        vfx = GetComponent<VisualEffect>();
    }
 
    private void Update() {
        if (frame < frames)
        {
            // Clunky temporary solution, shouldn't be dependent on starting frame
            if(frame == 1)
            {
                gameObject.AddComponent<Rigidbody>().useGravity = false;
                gameObject.AddComponent<CapsuleCollider>();
            }
            string fileName =
                $"Assets//" +
                $"Resources//" +
                $"PointClouds//" +
                $"{name}//" +
                $"{name}_{frame + 1:D4}.ply";
            try
            {
                (List<Vector3> p, List<Color> c) = ReadPointCloud(fileName);
                (Texture2D tc, Texture2D tp, uint pc) = Generate(p, c);
                textures.Add((tc, tp, pc));
            }
            catch(Exception e)
            {
                print("Couldn't load point cloud: ");
                print(e.Message);
                frame = -1;
            }
        } else /*if (frame == frames) */{
            (Texture2D tc, Texture2D tp, uint pc) = textures[frame % frames];
            vfx.Reinit();
            vfx.SetUInt(Shader.PropertyToID("ParticleCount"), pc);
            vfx.SetTexture(Shader.PropertyToID("TexColor"), tc);
            vfx.SetTexture(Shader.PropertyToID("TexPosScale"), tp);
            vfx.SetUInt(Shader.PropertyToID("Resolution"), resolution);
        }
        frame += 1;
        Debug.Log("Frame " + frame.ToString());
    }
}
