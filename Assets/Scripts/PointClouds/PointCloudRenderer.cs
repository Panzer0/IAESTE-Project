using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.VFX;
 
public class PointCloudRenderer : MonoBehaviour {
    VisualEffect vfx;
    uint resolution = 1400;
    public float particleSize = 1f;
    public int frame = 0;
    public int frames = 1;
    List<(Texture2D texColor, Texture2D texPosScale, uint particleCount)> textures = new List<(Texture2D, Texture2D, uint)>();

    public (List<Vector3> positions, List<Color> colors) ReadPointCloud(string name) {
        List<Vector3> positions = new List<Vector3>();
        List<Color> colors = new List<Color>();
        StreamReader inp = new StreamReader(@name);
        bool t = false;
        //var i = 0;
        while (!inp.EndOfStream) {
            if (!t) {
                if (inp.ReadLine().Contains("end_header")) {
                    t = true;
                }
            } else {
                var line = inp.ReadLine();
                //i++;
                //if (i%2 ==0){
                var values = line.Split(' ');
                float x = float.Parse(values[0]);
                float y = float.Parse(values[1]);
                float z = float.Parse(values[2]);
                Vector3 pos = new Vector3(x, y, z);
                positions.Add(pos);
                float r = float.Parse(values[3]) / 255.0f;
                float g = float.Parse(values[4]) / 255.0f;
                float b = float.Parse(values[5]) / 255.0f;
                Color col = new Color(r, g, b);
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

    public (Texture2D texColor, Texture2D texPosScale, uint particleCount) Generate(List<Vector3> positions, List<Color> colors) {
        Texture2D texColor = new Texture2D(positions.Count > (int)resolution ? (int)resolution : positions.Count, Mathf.Clamp(positions.Count / (int)resolution, 1, (int)resolution), TextureFormat.RGBAFloat, false);
        Texture2D texPosScale = new Texture2D(positions.Count > (int)resolution ? (int)resolution : positions.Count, Mathf.Clamp(positions.Count / (int)resolution, 1, (int)resolution), TextureFormat.RGBAFloat, false);
        int texWidth = texColor.width;
        int texHeight = texColor.height;
        Debug.Log("Length " + positions.Count.ToString());
        Debug.Log("Length " + colors.Count.ToString());
        for (int y = 0; y < texHeight; y++) {
            for (int x = 0; x < texWidth; x++) {
                int index = x + y * texWidth;
                texColor.SetPixel(x, y, colors[index]);
                var data = new Color(positions[index].x, positions[index].y, positions[index].z, particleSize);
                texPosScale.SetPixel(x, y, data);
            }
        }
        texColor.Apply();
        texPosScale.Apply();
        uint particleCount = (uint)positions.Count;
        return (texColor, texPosScale, particleCount);
    }
 
    private void Start() {
        vfx = GetComponent<VisualEffect>();
    }
 
    private void Update() {
        if (frame < frames) {
            string fileName =
                $"Assets//" +
                $"Resources//" +
                $"PointClouds//" +
                $"{name}//" +
                $"{name}_{frame + 1:D4}.ply";
            var (p, c) = ReadPointCloud(fileName);
            var (tc, tp, pc) = Generate(p, c);
            textures.Add((tc, tp, pc));
        } else /*if (frame == frames) */{
            var (tc, tp, pc) = textures[frame % frames];
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
