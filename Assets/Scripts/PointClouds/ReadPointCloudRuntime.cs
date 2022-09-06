using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using UnityEditor;

public class ReadPointCloudRuntime : MonoBehaviour
{
    // The amount of integer values each point consists of
    private static readonly int INTS_PER_POINT = 6;

    private ParticleSystem particleSystem;
    public string[] requests;
    public PhysicMaterial collisionMaterial;

    public List<GameObject> existingPointClouds;

    // Returns an int from the end of a line
    static uint ReadLastUint(string line)
    {
        string[] words = line.Split(' ');
        return uint.Parse(words[^1]);
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
    // Reads INTS_PER_POINT int values from a string and returns them as an array
    private static List<PointCloudTemplate> ReadCloudTemplateAnimation(string line)
    {
        string[] words = line.Split(' ');
        // The row contains an invalid amount of values
        if(words.Length == 8)
        {
            List<PointCloudTemplate> templates = new();

            string name = words[0];
            float xPos = float.Parse(words[1]);
            float yPos = float.Parse(words[2]);
            float zPos = float.Parse(words[3]);
            int countDivisor = int.Parse(words[4]);
            int scaleDivisor = int.Parse(words[5]);
            uint frameRangeStart = uint.Parse(words[6]);
            uint frameRangeEnd = uint.Parse(words[7]);

            for (uint i = frameRangeStart; i < frameRangeEnd; i++)
            {
                templates.Add(
                    new PointCloudTemplate(
                        name,
                        new Vector3(xPos, yPos, zPos),
                        countDivisor,
                        scaleDivisor,
                        i
                        ));
                if(i == frameRangeStart)
                {
                    yPos += 1000000;
                }
            }
            return templates;
        }
        else throw new FileLoadException("Corrupted template (invalid argument count)");
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
        return periodIndex != -1 ? fileName.Substring(0, periodIndex) : fileName;
    }
    // Returns the original string up until and excluding the first appearance of '.'.
    private string getBaseObjectName(string fullName)
    {
        if (fullName is null)
        {
            throw new ArgumentNullException(nameof(fullName));
        }

        int periodIndex = fullName.IndexOf("_");
        return periodIndex != -1 ? fullName.Substring(0, periodIndex) : RemoveFileExtension(fullName);
    }

    // Loads a mesh of the given name with "_0001.obj" appended from the appropriate directory in the Meshes folder.
    private Mesh LoadCollisionMesh(string name, uint index)
    {
        string gameObjectPath = $"Assets//Resources//Meshes//{name}//depth_7//Stl//{name}_{ToLeadingZeroes4Digit(index)}.obj";
        GameObject tempGameObject = AssetDatabase.LoadAssetAtPath<GameObject>(gameObjectPath).transform.GetChild(0).gameObject;
        MeshFilter collisionMeshFilter = tempGameObject.GetComponent<MeshFilter>();
        return collisionMeshFilter.sharedMesh;
    }
    void ApplyCollisionMesh(GameObject targetObject, String meshType, uint frameIndex)
    {
        Mesh collisionMesh = LoadCollisionMesh(meshType, frameIndex);
        MeshCollider collider = targetObject.AddComponent<MeshCollider>();
        collider.convex = true;
        collider.sharedMesh = collisionMesh;
        collider.sharedMaterial = collisionMaterial;
    }

    private string ToLeadingZeroes4Digit(uint value) => value switch
    {
        <= 9 => $"000{value}",
        <= 99 => $"00{value}",
        <= 999 => $"0{value}",
        _ => value.ToString(),
    };

    private GameObject CreateParticleSystemObject(String name)
    {
        // particleSystem game object creation
        Material particleMaterial = new(Shader.Find("Particles/Standard Unlit"));
        GameObject go = new(name)
        {
            tag = "Interactable"
        };
        this.existingPointClouds.Add(go);
        particleSystem = go.AddComponent<ParticleSystem>();
        go.GetComponent<ParticleSystemRenderer>().material = particleMaterial;
        particleSystem.maxParticles = 2000000;
        particleSystem.enableEmission = false;
        return go;
    }

    private uint ParseHeader(StreamReader sr)
    {
        // Current line
        string line;
        // File validity flags
        bool gotCount = false;
        bool gotEndHeader = false;
        uint pointCount = 0;
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
                pointCount = ReadLastUint(line);
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
        else
        {
            return pointCount;
        }
    }

    private void ParseAndEmit(StreamReader sr, uint pointCount, PointCloudTemplate cloudTemplate)
    {
        // Iterate over each point in the file
        for (int i = 0; i < pointCount; i++)
        {
            string line = sr.ReadLine();
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

    private void SetupRigidbody(GameObject targetObject)
    {
        Rigidbody rigidbody = targetObject.AddComponent<Rigidbody>();
        rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePositionY;  
    }

    private void ApplyRotation(GameObject targetObject)
    {
        Rigidbody rigidbody = targetObject.GetComponent<Rigidbody>();

        // Setting default drag values
        rigidbody.drag = 10;
        rigidbody.angularDrag = 0;

        // Making the object rotate by default
        rigidbody.AddTorque(new Vector3(0, 100000, 0), ForceMode.Force);
    }

    private void ParseTemplate(PointCloudTemplate cloudTemplate)
    {
        try
        {
            using StreamReader sr = new(
                $"Assets//" +
                $"Resources//" +
                $"PointClouds//" +
                $"{getBaseObjectName(cloudTemplate.GetName())}//" +
                $"{getBaseObjectName(cloudTemplate.GetName())}_{ToLeadingZeroes4Digit(cloudTemplate.GetFrameIndex())}.ply");
            // Amount of points in the Point Cloud
            uint pointCount = ParseHeader(sr);
            ParseAndEmit(sr, pointCount, cloudTemplate);
        }
        catch (Exception e)
        {
            print(e.Message);
        }
    }

    bool IsRequestAnimation(String request)
    {
        string[] words = request.Split(' ');
        // The row contains an invalid amount of values
        return words.Length == 8;
    }
    bool IsRequestSingle(String request)
    {
        string[] words = request.Split(' ');
        // The row contains an invalid amount of values
        return words.Length == 6;
    }

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
            if (IsRequestSingle(request))
            {
                cloudTemplates.Add(ReadCloudTemplate(request));
            }
            else if (IsRequestAnimation(request))
            {
                cloudTemplates.AddRange(ReadCloudTemplateAnimation(request));
            }
        }

        foreach (PointCloudTemplate cloudTemplate in cloudTemplates)
        {
            print("Currently parsing " + cloudTemplate.GetName());
            try
            {
                GameObject go = CreateParticleSystemObject("Particle System " + cloudTemplate.GetName());

                ParseTemplate(cloudTemplate);
                // todo: The second argument being 1 is a temporary solution to the lack of meshes. Replace with cloudTemplate.GetFrameIndex() if these are present
                ApplyCollisionMesh(go, getBaseObjectName(cloudTemplate.GetName()), 1);
                SetupRigidbody(go);

                // Adding SenveGlove-related scripts
                String materialPath = "Assets//SenseGlove//Scripts//Feedback//SG_Material.cs";
                MonoScript materialScript = AssetDatabase.LoadAssetAtPath<MonoScript>(materialPath);
                go.AddComponent(materialScript.GetClass());

                // ParticleSystem adjustments
                particleSystem.transform.localScale =
                    new Vector3(
                        1f / cloudTemplate.GetScaleDivisor(),
                        1f / cloudTemplate.GetScaleDivisor(),
                        1f / cloudTemplate.GetScaleDivisor());
                particleSystem.transform.position = cloudTemplate.GetOffset();
                ApplyRotation(go);
            }
            catch (Exception e)
            {
                print("The file could not be read:");
                print(e.Message);
            }
        }
    }
    /*
     * Renders a set of point clouds from the arguments passed in "requests" under the following format:
     * [file name] [x offset] [y offset] [z offset] [Point amount divisor] [Scale divisor] [frame range start] [frame range end (non-inclusive)]
     * The point clouds are rendered as particle systems attached to new game objects
     */
    public void RenderPointCloudAnimation(List<string> requests)
    {
        List<PointCloudTemplate> cloudTemplates = new();
        // Parse the inspector variables to templates
        foreach (string request in requests)
        {
            if (IsRequestSingle(request))
            {
                cloudTemplates.Add(ReadCloudTemplate(request));
            }
            else if (IsRequestAnimation(request))
            {
                cloudTemplates.AddRange(ReadCloudTemplateAnimation(request));
            }
        }

        foreach (PointCloudTemplate cloudTemplate in cloudTemplates)
        {
            print("Currently parsing " + cloudTemplate.GetName());
            try
            {
                GameObject go = CreateParticleSystemObject("Particle System " + cloudTemplate.GetName());

                ParseTemplate(cloudTemplate);
                ApplyCollisionMesh(go, RemoveFileExtension(cloudTemplate.GetName()), cloudTemplate.GetFrameIndex());
                SetupRigidbody(go);

                // Adding SenveGlove-related scripts
                String materialPath = "Assets//SenseGlove//Scripts//Feedback//SG_Material.cs";
                MonoScript materialScript = AssetDatabase.LoadAssetAtPath<MonoScript>(materialPath);
                go.AddComponent(materialScript.GetClass());

                // ParticleSystem adjustments
                particleSystem.transform.localScale =
                    new Vector3(
                        1f / cloudTemplate.GetScaleDivisor(),
                        1f / cloudTemplate.GetScaleDivisor(),
                        1f / cloudTemplate.GetScaleDivisor());
                particleSystem.transform.position = cloudTemplate.GetOffset();
                ApplyRotation(go);
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
