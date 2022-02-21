using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalEstimation : MonoBehaviour
{
    [SerializeField] ComputeShader normalEstimationCS;
    PCDRenderer pcdRenderer;
    List<Vector3> vertices = new List<Vector3>();
    List<Vector3> normals = new List<Vector3>();
    List<Color> colors = new List<Color>();
    int kernel;

    void Start()
    {
        pcdRenderer = FindObjectOfType<PCDRenderer>();
    }

    void Update() 
    {
        if (Input.GetKeyDown(KeyCode.Space)) 
            StartComputation();
    }

    void StartComputation()
    {
        GetPointCloudData();
        // MockPCD();
        Compute();
        CreatePostProcessedPointClouds();
    }

    void MockPCD()
    {
        for (int i = 0; i < 20; i++)
        {
            normals.Add(new Vector3(0f, 0f, 0f));
            colors.Add(new Color(Random.Range(0, 1), Random.Range(0, 1), Random.Range(0, 1)));
        }

        vertices.Add(new Vector3(0f, 0f, 0f));
        vertices.Add(new Vector3(0f, 20.6f, -0.1f));
        vertices.Add(new Vector3(3.5f, 9.5f, 0.01f));
        vertices.Add(new Vector3(20f, 20.2f, -0.18f));
        vertices.Add(new Vector3(20.2f, 0f, 0.05f));
        vertices.Add(new Vector3(11f, 10.1f, -0.18f));
        vertices.Add(new Vector3(12f, 4f, -0.24f));
        vertices.Add(new Vector3(5.1f, 16f, -0.22f));
        vertices.Add(new Vector3(35f, 10f, -0.05f));
        vertices.Add(new Vector3(55f, 15f, .06f));
        vertices.Add(new Vector3(75f, 10f, 0.1f));
        vertices.Add(new Vector3(95f, 5f, 0.12f));
        vertices.Add(new Vector3(115f, 10f, 0.18f));
        vertices.Add(new Vector3(130f, 20f, 0.28f));
        vertices.Add(new Vector3(150.8f, 20f, 0.49f));
        vertices.Add(new Vector3(140f, 16f, 0.42f));
        vertices.Add(new Vector3(140f, 10f, 0.26f));
        vertices.Add(new Vector3(140f, 4f, 0.22f));
        vertices.Add(new Vector3(130.2f, 0f, 0.38f));
        vertices.Add(new Vector3(150.6f, 0f, .44f));
    }

    void GetPointCloudData()
    {
        if (pcdRenderer.pointCloudMaster == null) return;

        foreach (Transform child in pcdRenderer.pointCloudMaster.transform)
        {
            if (child.name == "PointCloud")
            {
                Mesh mesh = child.GetComponent<MeshFilter>().mesh;
                vertices.AddRange(mesh.vertices);
                normals.AddRange(mesh.normals);
                colors.AddRange(mesh.colors);
            }
        }
    }

    void Compute()
    {
        kernel = normalEstimationCS.FindKernel("LeastSquaresFitting");
        
        ComputeBuffer verticesBuffer = new ComputeBuffer(vertices.Count, sizeof(float) * 3);
        ComputeBuffer normalsBuffer = new ComputeBuffer(normals.Count, sizeof(float) * 3);

        verticesBuffer.SetData(vertices);
        normalsBuffer.SetData(normals);

        normalEstimationCS.SetBuffer(kernel, "vertices", verticesBuffer);
        normalEstimationCS.SetBuffer(kernel, "normals", normalsBuffer);

        normalEstimationCS.Dispatch(kernel, vertices.Count / 8, 1, 1);

        Vector3[] normalsArray = new Vector3[normals.Count];
        normalsBuffer.GetData(normalsArray);
        normals.Clear();
        normals.AddRange(normalsArray);

        verticesBuffer.Dispose();
        normalsBuffer.Dispose();

        Debug.Log("Normal Estimation Done");
    }

    void CreatePostProcessedPointClouds()
    {
        if (pcdRenderer.pointCloudMaster == null) return;

        Destroy(pcdRenderer.pointCloudMaster);
        pcdRenderer.pointCloudMaster = new GameObject("PointCloudMaster");

        int maxVerticesPerMesh = 10000;
        int numMeshes = vertices.Count / maxVerticesPerMesh;

        for (int i = 0; i <= numMeshes; i++)
        {
            int count = Mathf.Min(maxVerticesPerMesh, vertices.Count - i * maxVerticesPerMesh);

            pcdRenderer.RenderPointCloudData(
                vertices.GetRange(i * maxVerticesPerMesh, count).ToArray(),
                normals.GetRange(i * maxVerticesPerMesh, count).ToArray(),
                colors.GetRange(i * maxVerticesPerMesh, count).ToArray());
        }
    }
}
