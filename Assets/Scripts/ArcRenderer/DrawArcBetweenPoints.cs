using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class DrawArcBetweenPoints : MonoBehaviour
{
    [SerializeField]
    Vector3 Origin;

    [SerializeField]
    Vector3 Position;

    [SerializeField]
    bool Restart = false;

    GameObject ArcTarget;

    [SerializeField]
    int NumSegments = 0;

    LineRenderer lr;
    // Start is called before the first frame update
    void Start()
    {
        lr = GetComponent<LineRenderer>();
        ArcTarget = transform.Find("ArcTarget").gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        if (ArcTarget == null)
            ArcTarget = transform.Find("ArcTarget").gameObject;

        if (lr == null || Restart)
        {
            lr = GetComponent<LineRenderer>();
            lr.positionCount = NumSegments + 1;
        }

        if (ArcTarget != null && lr != null)
        {
            Origin = transform.position;
            Position = ArcTarget.transform.position;

            //DrawLineSegments();
            //            var angle = Mathf.Rad2Deg * Mathf.Acos(Vector3.Dot(Origin, Position) / (Origin.magnitude * Position.magnitude));
            DrawBezierCurve();
        }
    }
    //Draw a triangle between two points
    private void DrawTriangleBetween()
    {
        var ptsArray = new Vector3[NumSegments];
        var lineVector = Position - Origin;
        var distance = lineVector.magnitude / NumSegments;

        for (int i = 1; i < NumSegments - 1; i++)
        {
            ptsArray[i] = new Vector3(lineVector.x - distance * i, lineVector.y - distance * i, lineVector.z - distance * i);
        }
        lr.SetPositions(ptsArray);
    }

    //Draw a broken line between two points
    private void DrawLineSegments()
    {
        var ptsArray = new Vector3[NumSegments + 1];
        var lineVector = Position - Origin;
        var distance = new Vector3(lineVector.x / NumSegments, lineVector.y / NumSegments, lineVector.z / NumSegments);

        Vector3 to = Origin;

        //Debug.DrawRay(Origin, lineVector, Color.red);
        for (int i = 0; i < ptsArray.Length; i++)
        {
            ptsArray[i] = to;

            to += distance;
        }
        lr.SetPositions(ptsArray);
    }

    //Draw a Bezier curve between two points
    private void DrawBezierCurve()
    {
        var ptsArray = new Vector3[NumSegments + 1];
        var lineVector = Position - Origin;
        var distance = new Vector3(lineVector.x / NumSegments, lineVector.y / NumSegments, lineVector.z / NumSegments);

        Vector3 p0 = Origin;
        Vector3 p1 = ((Position - Origin) * 1 / 4f) + Vector3.up * 100;
        Vector3 p2 = ((Position - Origin) * 3 / 4f) + Vector3.up * 100;
        Debug.DrawLine(p2, p2 + Vector3.down * 100, Color.red);
        Vector3 p3 = Position;

        //Debug.DrawRay(Origin, lineVector, Color.red);
        for (int i = 0; i < ptsArray.Length; i++)
        {
            float t = (float)i / (float)NumSegments;
            ptsArray[i] = CalculateBezierPoint(t, p0, p1, p2, p3);

        }
        lr.SetPositions(ptsArray);
    }

    /*
    //Draw a curved arc two points
    private void DrawArcBetween(float angle, int resolution, float velocity)
    {
        Vector3[] arcArray = CalculateArcArray(angle, resolution, velocity);

        var ptsArray = new Vector3[NumSegments + 1];
        var lineVector = Position - Origin;
        var distance = new Vector3(lineVector.x / NumSegments, lineVector.y / NumSegments, lineVector.z / NumSegments);

        Vector3 to = Origin;

        //Debug.DrawLine(Position, Origin, Color.blue);
        //Debug.DrawRay(Origin, lineVector, Color.red);
        lr.SetPositions(arcArray);
    }

    private Vector3[] CalculateArcArray(float angle, int resolution, float velocity)
    {
        Vector3[] arcArray = new Vector3[NumSegments + 1];

        float gravity = -9.8f;
        var radianAngle = Mathf.Deg2Rad * angle;
        float maxDistance = (Mathf.Pow(velocity, 2) * Mathf.Sin(2 * radianAngle)) / gravity;

        for (int i = 0; i < arcArray.Length; i++)
        {
            //lerp value 't'
            float t = (float)i / (float)resolution;
            arcArray[i] = CalculateArcPoint(t, radianAngle, velocity, maxDistance);
        }
        return arcArray;
    }

    //calculate height and vertex of each index in array
    private Vector3 CalculateArcPoint(float t, float radianAngle, float velocity, float maxDistance)
    {
        float x = t * maxDistance;
        float g = -9.8f;
        float y = x * Mathf.Tan(radianAngle) - ((g * Mathf.Pow(x, 2)) / (2 * Mathf.Pow(velocity, 2) * Mathf.Pow(Mathf.Cos(radianAngle), 2)));

        Vector3 val = new Vector3(x, y, 0);

        return val;
    }
    */

    Vector3 CalculateBezierPoint(float t,
        Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float u = 1.0f - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;

        Vector3 p = uuu * p0; //first term
        p += 3 * uu * t * p1; //second term
        p += 3 * u * tt * p2; //third term
        p += ttt * p3; //fourth term

        return p;
    }
}
