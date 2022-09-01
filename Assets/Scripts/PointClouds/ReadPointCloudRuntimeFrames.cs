using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using UnityEditor;

public class ReadPointCloudRuntimeFrames : MonoBehaviour
{
    // The amount of integer values each point consists of
    private static readonly int INTS_PER_POINT = 6;

    private ParticleSystem particleSystem;
    public string[] requests;
    public PhysicMaterial collisionMaterial;

    public List<GameObject> existingPointClouds;

    // Returns an int from the end of a line
    static int ReadLastInt(string line)
    {
        string[] words = line.Split(' ');
        return int.Parse(words[^1]);
    }

    // Reads INTS_PER_POINT int values from a string and returns them as an array
    private static int[] ReadPoint(string line)
    {
        if (line is null)
        {
            throw new ArgumentNullException(nameof(line));
        }

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
    private static PointCloudTemplate ReadCloudTemplate(string line)
    {
        string[] words = line.Split(' ');
        // The row contains an invalid amount of values
        return words.Length == 6
            ? new PointCloudTemplate(
                words[0],
                new Vector3(float.Parse(words[1]), float.Parse(words[2]), float.Parse(words[3])),
                int.Parse(words[4]),
                int.Parse(words[5])
                )
            : throw new FileLoadException("Corrupted template (invalid argument count)");
    }

    // Destroys all of the point cloud gameobjects. 
    public void ClearPointClouds()
    {
        foreach (GameObject cloud in this.existingPointClouds)
        {
            Destroy(cloud);
        }
        this.existingPointClouds.Clear();
    }

    // Returns the original string up until and excluding the first appearance of '.'.
    private string RemoveFileExtension(string fileName)
    {
        if (fileName is null)
        {
            throw new ArgumentNullException(nameof(fileName));
        }

        int periodIndex = fileName.IndexOf(".");
        return fileName.Substring(0, periodIndex);
    }
    
    // Loads a mesh of the given name with "_0001.obj" appended from the appropriate directory in the Meshes folder.
    private Mesh LoadCollisionMesh(string name, uint index)
    {
        string gameObjectPath = $"Assets//Resources//Meshes//{name}//depth_7//Stl//{name}_{ToLeadingZeroes4Digit(index)}.obj";
        GameObject tempGameObject = AssetDatabase.LoadAssetAtPath<GameObject>(gameObjectPath).transform.GetChild(0).gameObject;
        MeshFilter collisionMeshFilter = tempGameObject.GetComponent<MeshFilter>();
        return collisionMeshFilter.sharedMesh;
    }

    private string ToLeadingZeroes4Digit(uint value) => value switch
    {
        <= 9 => $"000{value}",
        <= 99 => $"00{value}",
        <= 999 => $"0{value}",
        _ => value.ToString(),
    };
    /*
     * Renders a set of point clouds from the arguments passed in "requests" under the following format:
     * [file name] [x offset] [y offset] [z offset] [Point amount divisor] [Scale divisor]
     * The point clouds are rendered as particle systems attached to new game objects
     */
    public void RenderPointClouds(List<string> requests)
    {
        List<PointCloudTemplate> cloudTemplates = new();
        // Parse the inspector variables to templates
        foreach (string request in requests)
        {
            cloudTemplates.Add(ReadCloudTemplate(request));
        }

        foreach (PointCloudTemplate cloudTemplate in cloudTemplates)
        {
            print("Currently parsing " + cloudTemplate.GetName());

            // Loading collider mesh
            Mesh collisionMesh = LoadCollisionMesh(RemoveFileExtension(cloudTemplate.GetName()), 0);

            try
            {
                // particleSystem game object creation
                Material particleMaterial = new(Shader.Find("Particles/Standard Unlit"));
                GameObject go = new("Particle System " + cloudTemplate.GetName())
                {
                    tag = "Interactable"
                };
                this.existingPointClouds.Add(go);
                particleSystem = go.AddComponent<ParticleSystem>();
                go.GetComponent<ParticleSystemRenderer>().material = particleMaterial;
                particleSystem.maxParticles = 2000000;
                particleSystem.enableEmission = false;

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
                            pointCount = ReadLastInt(line);
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
                            int[] pointParams = ReadPoint(line);
                            // Initialise new particle
                            ParticleSystem.EmitParams emitParams = new()
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
                MeshCollider collider = go.AddComponent<MeshCollider>();
                collider.convex = true;
                collider.sharedMesh = collisionMesh;
                collider.sharedMaterial = collisionMaterial;

                // Rigidbody initialisation
                Rigidbody rigidbody = go.AddComponent<Rigidbody>();
                rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePositionY;

                // Adding SenveGlove-related scripts
                String materialPart = "Assets//SenseGlove//Scripts//Feedback//SG_Material.cs";
                MonoScript materialScript = AssetDatabase.LoadAssetAtPath<MonoScript>(materialPart);
                go.AddComponent(materialScript.GetClass());

                // ParticleSystem adjustments
                particleSystem.transform.localScale =
                    new Vector3(
                        1f / cloudTemplate.GetScaleDivisor(),
                        1f / cloudTemplate.GetScaleDivisor(),
                        1f / cloudTemplate.GetScaleDivisor());
                particleSystem.transform.position = cloudTemplate.GetOffset();

                // Setting default drag values
                rigidbody.drag = 10;
                rigidbody.angularDrag = 0;

                // Making the object rotate by default
                rigidbody.AddTorque(new Vector3(0, 100000, 0), ForceMode.Force);
            }
            catch (Exception e)
            {
                print("The file could not be read:");
                print(e.Message);
            }
        }
    }

    void Start()
    {
        existingPointClouds = new();
    }
}
