using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayRopeRenderer : MonoBehaviour
{
    public Transform[] points;
    public LineRenderer settings;

    private void Update()
    {
        for(int i =0;i<points.Length;i++)
        {
            settings.SetPosition(i, points[i].position);
        }
    }
}
