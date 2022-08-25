using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MeshRender : MonoBehaviour
{
    private int frameIndex = 0;
    public int loadedFrames = 0;
    
    
    private float time = 0.03333f;

    [SerializeField] List<Mesh> meshList = new List<Mesh>();
    [SerializeField] List<Texture> materialList = new List<Texture>();

    Renderer rend;
    MeshFilter filter;

    public int objectReferenceScene = 0;

    public List<int> loadQualities = new List<int>();
    public List<int> loadSequences = new List<int>();

    public int fps = 50;


    void Start()
    {
        StartCoroutine(LoadFrames());

        gameObject.transform.localScale = new Vector3(0.0018f, 0.0018f, 0.0018f);

        StartCoroutine(MeshRendering());

    }

    
    void LoadByQuality(int depth, int frames, int i)
    {
        for (int f = i+1 ; f <= frames+i; f++)
        {
            string fileName = "Meshes/" + name + "/depth_" + depth + "/Stl/" + name + "_" + (f).ToString("D4");
            string fileTex = "Meshes/" + name + "/depth_" + depth + "/Stl/" + name + "_" + (f).ToString("D4") + "_tex";

            Texture loadedtexture = (Texture)Resources.Load(fileTex, typeof(Texture));
            Mesh loadedmesh = (Mesh)Resources.Load(fileName, typeof(Mesh));

            meshList.Add(loadedmesh);
            materialList.Add(loadedtexture);
        }
    }


    private IEnumerator LoadFrames()
    {

        int frames, depth;

        for (var i = 0; i < loadQualities.Count; i++) {
            depth = loadQualities[i];
            frames = loadSequences[i];
            LoadByQuality(depth, frames, loadedFrames);
            loadedFrames += frames;

            yield return null;
        }
    }

    public IEnumerator MeshRendering(){

        rend = GetComponent<Renderer>();
        filter = GetComponent<MeshFilter>();

        bool colliderReady = false;
        while (true){
            if(ButtonObject.Instance.sceneIndex == objectReferenceScene){

                if(rend.isVisible){
                    if(fps > 30){
                        //Forward play of the sequence 
                        if(frameIndex < loadedFrames){
                            rend.material.mainTexture = materialList[frameIndex];    
                            filter.sharedMesh = meshList[frameIndex];
                        }
                        //Backward play of the sequence 
                        else
                        {
                            
                            rend.material.mainTexture = materialList[2 * loadedFrames - frameIndex -1];    
                            filter.sharedMesh = meshList[2 * loadedFrames - frameIndex - 1];                           
                        }

                        if (!colliderReady)
                        {
                            gameObject.AddComponent<CapsuleCollider>();
                            gameObject.AddComponent<Rigidbody>();
                            gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
                            colliderReady = true;
                        }
                    }
                }
                frameIndex ++;
                frameIndex = frameIndex % (2 * loadedFrames);
                yield return new WaitForSeconds(time); //0.03333f   
            }else{
                yield return null;
            }

        }
    }
}
