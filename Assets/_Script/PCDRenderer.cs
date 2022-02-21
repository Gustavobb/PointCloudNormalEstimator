using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class PCDRenderer : MonoBehaviour
{
    [HideInInspector] public GameObject pointCloudMaster;
    [SerializeField] List<TextAsset> pointClouds;
    [SerializeField] Material pointCloudMaterial;
    [SerializeField] bool showNormals = false;
    [SerializeField] bool cullNormals = false;

    void Start()
    {
        StartPointCloudVisualization();
        if (cullNormals) pointCloudMaterial.SetFloat("cull_normals", 1);
    }

    public void StartPointCloudVisualization()
    {
        pointCloudMaster = new GameObject("PointCloudMaster");
        ReadPointClouds();
    }

    void ReadPointClouds()
    {
        foreach (TextAsset txt in pointClouds)
        {
            byte[] bytes = CLZF2.Decompress(txt.bytes);
            string str = System.Text.Encoding.UTF8.GetString(bytes);

            PointCloud pointCloud = JsonUtility.FromJson<PointCloud>(str);
            RenderPointCloudData(pointCloud.vertices, pointCloud.normals, pointCloud.colors);
        }
    }

    public void EndPointCloudVisualization()
    {
        if (pointCloudMaster == null) return;
        Destroy(pointCloudMaster);
    }

    public void RenderPointCloudData(Vector3[] vertices, Vector3[] normals, Color[] colors)
    {
        GameObject pointCloud = new GameObject("PointCloud");
        pointCloud.transform.parent = pointCloudMaster.transform;

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.colors = colors;

        int[] indices = new int[vertices.Length];
        for (int i = 0; i < indices.Length; i++)
            indices[i] = i;

        mesh.SetIndices(indices, MeshTopology.Points, 0);
        mesh.Optimize();

        MeshRenderer meshRenderer = pointCloud.AddComponent<MeshRenderer>();
        meshRenderer.material = pointCloudMaterial;

        MeshFilter meshFilter = pointCloud.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;

        if (showNormals) pointCloud.AddComponent<TangentSpaceVisualizer>();
    }

    void OnApplicationQuit()
    {
        pointCloudMaterial.SetFloat("cull_normals", 0);
    }
}

[System.Serializable]
public class PointCloud
{
    public Vector3[] vertices;
    public Vector3[] normals;
    public Color[] colors;
}