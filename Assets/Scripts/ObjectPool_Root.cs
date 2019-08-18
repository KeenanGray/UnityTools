using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using System.Linq;


[ExecuteInEditMode]
public class ObjectPool_Root : MonoBehaviour
{

    private enum Orientation
    {
        XY,
        YZ,
        XZ
    }

    [HideInInspector]
    GameObject cam;
    [SerializeField]
    Orientation orient = Orientation.XY;

    [HideInInspector]
    Orientation lastOrientation;

    [SerializeField]
    [HideInInspector]
    Vector3 SizeVector;

    [SerializeField]
    [HideInInspector]
    Vector3[] objectPositions;
    [HideInInspector]
    GameObject[] myObjects;

    Vector3[] ActivePositions;
    private int lastNumToShow;
    Vector3[] v3Array;
    bool justStarted = false;
    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main.gameObject;
        justStarted = true;
        holdIndexVector = new Vector3();
    }

    // Update is called once per frame
    void Update()
    {
        if (myObjects == null)
        {
            myObjects = new GameObject[transform.childCount];
            for (int i = 0; i < transform.childCount; i++)
            {
                myObjects[i] = transform.GetChild(i).gameObject;
            }
        }
        if (orient != lastOrientation || justStarted)
        {
            justStarted = false;
            lastOrientation = orient;

            ActivePositions = new Vector3[myObjects.Length];
            if (cam == null)
                cam = Camera.main.gameObject;
            if (myObjects == null || objectPositions == null)
                return;

            //orient and set the positions in the list
            OrientGameObjects(ref ActivePositions);
            //move each object into position based on the current orientation of active positions
            for (int i = 0; i < myObjects.Length; i++)
            {
                myObjects[i].transform.localPosition = ActivePositions[i];
                myObjects[i].name = IndexToVector(i).ToString();
            }
        }

        if (numToShow != lastNumToShow)
        {
            v3Array = new Vector3[(int)Mathf.Pow(numToShow, 3)];
            lastNumToShow = numToShow;
        }

        var camT = GameObject.Find("CameraTracker").transform;

        Vector3 cameraRelative = camT.InverseTransformPoint(transform.position) * -1;
        cameraRelative += Camera.main.transform.forward;
        //        Debug.Log(cameraRelative);
        //ActivateClosestToOrigin(new Vector3(0, 0, 0));
        ActivateClosestToOrigin(cameraRelative);
    }

    public int numToShow = 5;
    private void ActivateClosestToOrigin(Vector3 origin)
    {
        var realNum = (numToShow - 1) / 2;
        var len = 0;
        if (ActivePositions != null)
        {
            if (ActivePositions.Length > 0)
            {
                len = ActivePositions.Length;
                //Setup the active positions array so that only the positions nearest the origin are active
                Vector3 tmp = new Vector3();

                int max_X = (int)origin.x + realNum;// (int)SizeVector.x;
                int max_Y = (int)origin.y + realNum;// (int)SizeVector.y;
                int max_Z = (int)origin.z + realNum; //(int)SizeVector.z;

                int min_X = (int)origin.x - realNum;// (int)SizeVector.x;
                int min_Y = (int)origin.y - realNum;// (int)SizeVector.y;
                int min_Z = (int)origin.z - realNum; //(int)SizeVector.z;

                var j = 0;

                // if (objectPositions == null)
                //     GenerateObjectPositions();

                //TODO: Manage this without iteration
                //We should be able to grab the indexes we need some other ways
                for (int i = 0; i < objectPositions.Length; i++)
                {
                    //if (i < objectPositions.Length - 1)
                    //    Debug.DrawLine(objectPositions[i], objectPositions[i + 1]);

                    //Index to vector seems to the be the holdup
                    //To much mathmatics
                    tmp = IndexToVector(i);

                    if ((tmp.x >= min_X && tmp.x <= max_X) &&
                        (tmp.y >= min_Y && tmp.y <= max_Y) &&
                        (tmp.z >= min_Z && tmp.z <= max_Z))
                    {

                        if (j < len)
                        {
                            ActivePositions[j] = objectPositions[ObjectPositionFromVector(tmp)];
                            j++;
                        }
                        else
                        {
                            i = objectPositions.Length;
                            break;
                        }
                    }
                    else
                    {

                    }

                }

                for (int i = 0; i < ActivePositions.Length; i++)
                {
                    //myObjects[i].transform.localPosition = new Vector3(0, 0, 0);
                    myObjects[i].transform.localPosition = ActivePositions[i];
                }

            }
        }
    }

    private void GenerateObjectPositions()
    {
        var pos = 0;
        var X_Spacing = Mathf.Abs(transform.GetChild(0).transform.position.x - transform.GetChild(1).transform.position.x);
        var Y_Spacing = Mathf.Abs(transform.GetChild(0).transform.position.y - transform.GetChild(1).transform.position.y);
        var Z_Spacing = Mathf.Abs(transform.GetChild(0).transform.position.z - transform.GetChild(1).transform.position.z);

        for (int row_x = 0; row_x < SizeVector.x; row_x++)
        {
            for (int col_y = 0; col_y < SizeVector.y; col_y++)
            {
                for (int depth_z = 0; depth_z < SizeVector.z; depth_z++)
                {
                    // CreateNewInstance(prefab, row, col, depth);
                    var CurrentTotal = (row_x * (SizeVector.y * SizeVector.z)) + (col_y * SizeVector.z) + (depth_z);

                    objectPositions[pos] = new Vector3(
                        transform.position.x + X_Spacing * row_x,
                       transform.position.y + Y_Spacing * col_y,
                       transform.position.z + Z_Spacing * depth_z);
                    pos++;
                }
            }
        }

    }

    private int ObjectPositionFromVector(Vector3 tmp)
    {
        switch (orient)
        {
            case Orientation.XY:
                return (int)((tmp.x * (SizeVector.y * SizeVector.z)) + (tmp.y * SizeVector.z) + (tmp.z));
            case Orientation.YZ:
                return (int)((tmp.x * (SizeVector.y * SizeVector.z)) + (tmp.y * SizeVector.z) + (tmp.z));
            case Orientation.XZ:
                return (int)((tmp.x * (SizeVector.y * SizeVector.z)) + (tmp.y * SizeVector.z) + (tmp.z));
        }
        return (int)((tmp.x * (SizeVector.y * SizeVector.z)) + (tmp.y * SizeVector.z) + (tmp.z));

    }

    private void OrientGameObjects(ref Vector3[] activePositions)
    {
        for (int i = 0; i < activePositions.Length; i++)
        {
            int offset = 0;

            //Flip to X - Axis
            //Order of cubes is YX

            if (orient == Orientation.XY)
            {
                offset = (i * (int)(SizeVector.z));
                if (i >= SizeVector.x * SizeVector.y)
                {
                    //wrap around when number is too large
                    if (SizeVector.x > 0 && SizeVector.y > 0)
                        offset = (i % (int)(SizeVector.x * SizeVector.y)) * (int)(SizeVector.z) + i / (int)(SizeVector.x * SizeVector.y);
                }
            }

            //Order of cubes is ZX

            if (orient == Orientation.XZ)
            {
                offset = (i % (int)(SizeVector.z)) + (int)(i / SizeVector.z) * (int)(SizeVector.z * SizeVector.y);
                if (offset >= SizeVector.z * SizeVector.x * SizeVector.y)
                {
                    offset = ((i) % (int)(SizeVector.z)) + (int)((i) / SizeVector.z) * (int)(SizeVector.z * SizeVector.y);
                    offset -= (int)(i / (SizeVector.x * SizeVector.z)) * (int)(SizeVector.z * SizeVector.x * SizeVector.y);
                    offset += (int)(SizeVector.z) * (int)(i / (int)(SizeVector.x * SizeVector.z));
                }

            }

            //Order of cubes is ZY
            if (orient == Orientation.YZ)
                offset = i;

            try
            {
                activePositions[i] = objectPositions[offset];
            }
            catch (Exception e)
            {
                if (e.GetType() == typeof(NullReferenceException)) { }
                //Debug.Log("oops i=" + " offset=" + offset);
            }
        }
    }

    //gives a coordinate to a gameobject in postion based on currently chosen orientation
    //And index of position in the list
    Vector3 holdIndexVector;
    private Vector3 IndexToVector(int i)
    {
        float x = -1;
        float y = -1;
        float z = -1;

        switch (orient)
        {

            case Orientation.XY:
                y = i % SizeVector.y;
                x = (i / SizeVector.y) % SizeVector.x;
                z = i / (SizeVector.x * SizeVector.y);  //BAD
                break;
            case Orientation.XZ:
                z = i % SizeVector.z;
                x = (i / SizeVector.z) % SizeVector.x;
                y = i / (SizeVector.x * SizeVector.z);  //BAD
                break;
            case Orientation.YZ:
                z = i % SizeVector.z;
                y = (i / SizeVector.z) % SizeVector.y;
                x = i / (SizeVector.y * SizeVector.z);//BAD
                break;
            default:
                break;
        }
        //Debug.Log(x + " " + y + "" + z);
        return new Vector3((int)x, (int)y, (int)z);
    }

    public void SetupObjectPool(Vector3[] positions, int count, Vector3 spacing)
    {
        objectPositions = positions;
        myObjects = new GameObject[count];
        SizeVector = spacing;

        for (int i = 0; i < count; i++)
        {
            myObjects[i] = transform.GetChild(i).gameObject;
        }
    }
}

