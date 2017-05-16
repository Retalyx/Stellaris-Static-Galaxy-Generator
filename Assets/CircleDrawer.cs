using UnityEngine;
using System.Collections;

public class CircleDrawer : MonoBehaviour
{
    public float ThetaScale = 0.01f;
    public float radius = 10f;
    private int Size;
    private LineRenderer LineDrawer;
    private float Theta = 0f;
	public Vector3 offset = new Vector3(0,0,0);
    void Start ()
    {       
        LineDrawer = GetComponent<LineRenderer>();
    }

    void Update ()
    {      
        Theta = 0f;
        Size = (int)((1f / ThetaScale) + 1f);
        LineDrawer.SetVertexCount(Size); 
        for(int i = 0; i < Size; i++){          
            Theta += (2.0f * Mathf.PI * ThetaScale);         
            float x = radius * Mathf.Cos(Theta);
            float z = radius * Mathf.Sin(Theta);          
            LineDrawer.SetPosition(i, new Vector3(x+offset.x, 0, z+offset.z));
        }
    }
}