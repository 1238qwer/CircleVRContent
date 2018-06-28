using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBarrierEventHandler
{
    Vector3 GetPosition();
    void OnStay(bool inBarrier);
}

public class CircleVRBarrier : MonoBehaviour
{
    public const float angle = 15.0f;
    public const float heightMin = 0.2f;
    public const float heightMax = 6.0f;
    public const float drawingHeight = 1.0f;

    private float radius;
    private CircleVR circleVR;
        
    private MeshFilter filter;

    private bool initFinished;

    public static List<IBarrierEventHandler> Events = new List<IBarrierEventHandler>();

    public void Init(float radius, Material mat , bool show)
    {
        circleVR = CircleVR.Instance;
        this.radius = radius;
        MeshRenderer renderer = gameObject.AddComponent<MeshRenderer>();
        renderer.material = mat;


        filter = gameObject.AddComponent<MeshFilter>();

        if(show)
            CreateMesh(radius, drawingHeight, mat);

        initFinished = true;

        Debug.Log("[Barrier] Initialized");
    }

    public bool IsInBarrier(Vector3 worldPosition)
    {
        float dist = (new Vector2(worldPosition.x , worldPosition.z) - new Vector2(transform.position.x , transform.position.z)).magnitude;
        float height = Mathf.Abs(worldPosition.y - transform.position.y);

        return dist <= radius && height <= heightMax && height >= heightMin;
    }

    private void CreateMesh(float radius, float height, Material mat)
    {
        Mesh barrier = new Mesh();

        List<Vector3> vertices = new List<Vector3>();

        float total = 0.0f;
        while (total <= 360)
        {
            Vector3 vertice = new Vector3(radius * Mathf.Cos(total * Mathf.Deg2Rad), 0.0f, radius * Mathf.Sin(total * Mathf.Deg2Rad));
            Vector3 vertice2 = new Vector3(radius * Mathf.Cos(total * Mathf.Deg2Rad), height, radius * Mathf.Sin(total * Mathf.Deg2Rad));

            vertices.Add(vertice);
            vertices.Add(vertice2);

            total += angle;
        }

        int[] tris = new int[(vertices.Count - 2) * 3];

        for (int i = 0; i < tris.Length / 3; i++)
        {
            if (i % 2 == 1)
            {
                tris[i * 3] = i + 2;
                tris[i * 3 + 1] = i;
                tris[i * 3 + 2] = i + 1;
                continue;
            }
            tris[i * 3] = i + 2;
            tris[i * 3 + 1] = i + 1;
            tris[i * 3 + 2] = i;
        }

        barrier.SetVertices(vertices);
        barrier.triangles = tris;
        filter.mesh = barrier;
    }

    private void Update()
    {
        if(!initFinished)
            return;

        if(circleVR.trackerOrigin)
            transform.position = Vector3.Lerp(transform.position, circleVR.trackerOrigin.position , 0.95f);

        foreach (var Event in Events)
        {
            Event.OnStay(IsInBarrier(Event.GetPosition()));
        }
    }
}
