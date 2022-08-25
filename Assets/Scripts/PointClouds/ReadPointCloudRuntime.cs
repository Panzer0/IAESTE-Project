using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using static UnityEngine.ParticleSystem;
using UnityEditor;


// Contains basic information required to generate a new point cloud

public class ReadPointCloudRuntime : MonoBehaviour
{
    private ParticleSystem particleSystem;
    public string[] requests;
    public PhysicMaterial collisionMaterial;

    // The amount of integer values each point consists of
    static readonly int INTS_PER_POINT = 6;

    private List<GameObject> existingPointClouds;

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
            new Vector3(float.Parse(words[1]), float.Parse(words[2]), float.Parse(words[3])), 
            int.Parse(words[4]), 
            int.Parse(words[5])
            );
    }

    public void ClearPointClouds()
    {
        foreach(GameObject cloud in this.existingPointClouds)
        {
            Destroy(cloud.gameObject);
        }
        this.existingPointClouds.Clear();
    }

    private string RemoveFileExtension(String fileName)
    {
        int periodIndex = fileName.IndexOf(".");
        return fileName.Substring(0, periodIndex);
    }

    private Mesh LoadCollisionMesh(String name)
    {
        //cloudTemplate.GetName()
        String gameObjectPath = "Assets//Resources//Meshes//" +
            name + "//depth_7//Stl//" +
            name + "_0001.obj";
        GameObject tempGameObject = AssetDatabase.LoadAssetAtPath<GameObject>(gameObjectPath).transform.GetChild(0).gameObject;
        MeshFilter collisionMeshFilter = tempGameObject.GetComponent<MeshFilter>();
        return collisionMeshFilter.sharedMesh;
    }

    public void RenderPointClouds(List<String> requests)
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
            Mesh collisionMesh = LoadCollisionMesh(RemoveFileExtension(cloudTemplate.GetName()));

            try
            {
                // particleSystem game object creation
                Material particleMaterial = new(Shader.Find("Particles/Standard Unlit"));
                var go = new GameObject("Particle System " + cloudTemplate.GetName());
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

                var rigidbody = go.AddComponent<Rigidbody>();
                rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePositionY;

                // Adding SenveGlove-related scripts
                //String grabablePath = "Assets//SenseGlove//Scripts//Interaction//SG_Grabable.cs";
                //MonoScript grabableScript = AssetDatabase.LoadAssetAtPath<MonoScript>(grabablePath);
                String materialPart = "Assets//SenseGlove//Scripts//Feedback//SG_Material.cs";
                MonoScript materialScript = AssetDatabase.LoadAssetAtPath<MonoScript>(materialPart);

                //go.AddComponent(grabableScript.GetClass());
                go.AddComponent(materialScript.GetClass());


                go.tag = "Interactable";

                // ParticleSystem adjustments
                particleSystem.transform.localScale =
                    new Vector3(
                        1f / cloudTemplate.GetScaleDivisor(),
                        1f / cloudTemplate.GetScaleDivisor(),
                        1f / cloudTemplate.GetScaleDivisor());
                particleSystem.transform.position = cloudTemplate.GetOffset();

                //TEMPORARY CENTRE OF MASS PARTICLE FOR TESTING PURPOSES
                // todo: Remove
                var emitParamsTemp = new ParticleSystem.EmitParams
                {
                    position = rigidbody.centerOfMass,
                    velocity = Vector3.zero,
                    startSize = (float)Math.Sqrt((float)cloudTemplate.GetCountDivisor() * 10),
                    startLifetime = 100000,
                    startColor = new Color32(255, 255, 255, 255)
                };
                particleSystem.Emit(emitParamsTemp, 1);

                //go.GetComponent<RectTransform>().pivot = rigidbody.centerOfMass;
                //GameObject pivotGameObject = new GameObject("pivot");
                //pivotGameObject.transform.position = rigidbody.centerOfMass;
                //go.transform.SetParent(pivotGameObject.transform);

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

    // Update is called once per frame
    void Update()
    {
    }
}
