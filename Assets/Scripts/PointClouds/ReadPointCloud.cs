using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using static UnityEngine.ParticleSystem;
using UnityEditor;

// Contains basic information required to generate a new particle
// No longern eeded, since I'm no longer storing points. 
public class ParticleParams
{
    private Vector3 position;
    private Color32 colour;

    public ParticleParams(Vector3 position, byte r, byte g, byte b)
    {
        this.position = position;
        this.position.x *= -1;
        this.colour = new Color32(Convert.ToByte(r), Convert.ToByte(g), Convert.ToByte(b), 255);
    }

    public Vector3 GetPosition()
    {
        return this.position;
    }

    public Color32 getColour()
    {
        return this.colour;
    }

    public override string ToString()
    {
        string formatted =
            string.Format("xyz: ({0}, {1}, {2}), RGB: ({3}, {4}, {5})",
                          this.GetPosition().x, this.GetPosition().y,
                          this.GetPosition().z, this.getColour().r,
                          this.getColour().g, this.getColour().b);
        return formatted;
    }
}

// Contains basic information required to generate a new point cloud
public class PointCloudTemplate
{
    private readonly string name;
    private Vector3 offset;
    private readonly int countDivisor;
    private readonly int scaleDivisor;

    public PointCloudTemplate(string name, Vector3 offset, int countDivisor, int scaleDivisor)
    {
        this.name = name;
        this.offset = offset;
        this.countDivisor = countDivisor > 1 ? countDivisor : 1;
        this.scaleDivisor = scaleDivisor != 0 ? scaleDivisor : 1;
    }
    
    public string GetName()
    {
        return this.name;
    }

    public Vector3 GetOffset()
    {
        return this.offset;
    }

    public int GetScaleDivisor()
    {
        return this.scaleDivisor;
    }

    public int GetCountDivisor()
    {
        return this.countDivisor;
    }

    public override string ToString()
    {
        return string.Format("{0} -  offset by ({1}), scale divisor = {2}, count divisor = {3}",
            this.GetName(), this.GetOffset(), this.GetScaleDivisor(), this.GetCountDivisor());
    }
}

public class ReadPointCloud : MonoBehaviour
{
    private ParticleSystem particleSystem;
    public string[] requests;
    public PhysicMaterial collisionMaterial;

    // The amount of integer values each point consists of
    static readonly int INTS_PER_POINT = 6;

    // Returns an int from the end of a line
    static int readLastInt(string line) {
        string[] words = line.Split(' ');
        return int.Parse(words[^1]);
    }

    // Reads INTS_PER_POINT int values from a string and returns them as an array
    static int[] readPoint(string line)
    {
        string[] words = line.Split(' ');
        // The row contains an invalid amount of values
        if (words.Length != INTS_PER_POINT)
        {
            throw new FileLoadException("Corrupted point (invalid argument count)");
        }
        int[] returnArray = new int[INTS_PER_POINT];
        for (int i = 0; i < INTS_PER_POINT; i++)
        {
            returnArray[i] = int.Parse(words[i]);
        }
        return returnArray;
    }

    // Reads INTS_PER_POINT int values from a string and returns them as an array
    static PointCloudTemplate readCloudTemplate(string line)
    {
        string[] words = line.Split(' ');
        // The row contains an invalid amount of values
        if (words.Length != 6)
        {
            throw new FileLoadException("Corrupted template (invalid argument count)");
        }
        return new PointCloudTemplate(
            words[0], 
            new Vector3(int.Parse(words[1]), int.Parse(words[2]), int.Parse(words[3])), 
            int.Parse(words[4]), 
            int.Parse(words[5])
            );
    }


    void Start()
    {
        ///////// CONFIGURATION /////////
        List<PointCloudTemplate> cloudTemplates = new();

        // Parse the inspector variables to templates
        foreach (string request in requests)
        {
            cloudTemplates.Add(readCloudTemplate(request));
        }

        foreach (PointCloudTemplate cloudTemplate in cloudTemplates)
        {
            print("Currently parsing " + cloudTemplate.GetName());

            // Loading collider mesh
            // todo: Delegate to a method
            int dotIndex = cloudTemplate.GetName().IndexOf(".");
            String gameObjectPath = "Assets//Resources//Meshes//" +
                cloudTemplate.GetName().Substring(0, dotIndex) +
                "//depth_7//Stl//" +
                cloudTemplate.GetName().Substring(0, dotIndex) + "_0001.obj";
            print("THE PATH IS " + gameObjectPath);
            GameObject tempGameObject = AssetDatabase.LoadAssetAtPath<GameObject>(gameObjectPath).transform.GetChild(0).gameObject;
            MeshFilter collisionMeshFilter = tempGameObject.GetComponent<MeshFilter>();
            Mesh collisionMesh = collisionMeshFilter.sharedMesh;

            try
            {
                // particleSystem game object creation
                Material particleMaterial = new(Shader.Find("Particles/Standard Unlit"));
                var go = new GameObject("Particle System " + cloudTemplate.GetName());
                particleSystem = go.AddComponent<ParticleSystem>();
                go.GetComponent<ParticleSystemRenderer>().material = particleMaterial;
                go.GetComponent<ParticleSystem>().maxParticles = 2000000;
                var colissionTemp = go.GetComponent<ParticleSystem>().collision;
                //colissionTemp.enabled = true;
                colissionTemp.type = ParticleSystemCollisionType.World;
                colissionTemp.bounce = 0;
                colissionTemp.dampen = 10000;

                using (StreamReader sr = new(cloudTemplate.GetName()))
                {
                    // Current line
                    string line;
                    // Amount of points in the Point Cloud
                    int pointCount = 0;
                    // File validity flags
                    bool gotCount = false;
                    bool gotEndHeader = false;


                    // Header parsing
                    while ((line = sr.ReadLine()) != null)
                    {
                        // Point count definition reached
                        if (line.StartsWith("element vertex "))
                        {
                            // Size can't be defined twice!
                            if (gotCount)
                            {
                                throw new FileLoadException("Double size definition");
                            }
                            pointCount = readLastInt(line);
                            gotCount = true;
                        }
                        // End of header reached
                        if (line.Equals("end_header"))
                        {
                            gotEndHeader = true;
                            break;
                        }

                    }

                    // Header integrity validation
                    if (!(gotCount && gotEndHeader))
                    {
                        throw new FileLoadException("Missing header information");
                    }

                    // Obsolete, points are no longer stored
                   // pointArray = new ParticleParams[pointCount / cloudTemplate.getCountDivisor() + 1];

                    // Iterate over each point in the file
                    for (int i = 0; i < pointCount; i++)
                    {
                        line = sr.ReadLine();
                        // End of file reached before finding the promised amount of points
                        if (line == null)
                        {
                            throw new FileLoadException("Insufficient points");
                        }
                        // Read and emit the point
                        if (i % cloudTemplate.GetCountDivisor() == 0)   // Omit countDivisor-1 points for each read one
                        {
                            int[] pointParams = readPoint(line);
                            var emitParams = new ParticleSystem.EmitParams
                            {
                                position = new Vector3(-1 * pointParams[0], pointParams[1], pointParams[2]),
                                velocity = new Vector3(0, 0, 0),
                                startSize = (float)Math.Sqrt((float)cloudTemplate.GetCountDivisor()),
                                startLifetime = 100000,
                                startColor = new Color32(
                                    Convert.ToByte(pointParams[3]), 
                                    Convert.ToByte(pointParams[4]), 
                                    Convert.ToByte(pointParams[5]), 255)
                            };
                            particleSystem.Emit(emitParams, 1);
                        }
                    }
                }
                


                // Collider initialisation
                var collider = go.AddComponent<MeshCollider>();
                collider.convex = true;
                collider.sharedMesh = collisionMesh;
                collider.sharedMaterial = collisionMaterial;

                go.AddComponent<Rigidbody>();

                // Adding SenveGlove-related scripts
                String grabablePath = "Assets//SenseGlove//Scripts//Interaction//SG_Grabable.cs";
                MonoScript grabableScript = AssetDatabase.LoadAssetAtPath<MonoScript>(grabablePath);
                String materialPart = "Assets//SenseGlove//Scripts//Feedback//SG_Material.cs";
                MonoScript materialScript = AssetDatabase.LoadAssetAtPath<MonoScript>(materialPart);

                go.AddComponent(grabableScript.GetClass());
                go.AddComponent(materialScript.GetClass());

                // ParticleSystem adjustments
                particleSystem.transform.localScale = 
                    new Vector3(
                        1f / cloudTemplate.GetScaleDivisor(), 
                        1f / cloudTemplate.GetScaleDivisor(), 
                        1f / cloudTemplate.GetScaleDivisor());
                particleSystem.transform.position = cloudTemplate.GetOffset();
            }
            catch (Exception e)
            {
                print("The file could not be read:");
                print(e.Message);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
    }
}
