using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

[ExecuteInEditMode]
public class ObjectPool_Root : MonoBehaviour
{
    [SerializeField]
    Vector3[] objectPositions;
    [SerializeField]
    GameObject[] myObjects;
    [SerializeField]
    Vector3 SizeVector;
    [SerializeField]
    GameObject cam;

    public bool shouldUpdate = false;
    public float dist = 5;
    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main.gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        if (!shouldUpdate)
        {
            return;
        }
        if (cam == null)
            cam = Camera.main.gameObject;
        if (myObjects == null || objectPositions == null)
            return;

        var OriginToCamera = transform.position - (cam.transform.position + cam.transform.forward * dist);

        Vector3[] ActivePositions = new Vector3[myObjects.Length];
        SetActivePositions(ref ActivePositions, OriginToCamera);

        //objectPositions = objectPositions.OrderBy(x => x.x).ToArray();

        if (myObjects.Length > objectPositions.Length)
        {
            print("Something wrong");
        }

        for (int i = 0; i < myObjects.Length; i++)
        {
            myObjects[i].transform.localPosition = ActivePositions[i];
        }
    }

    private void SetActivePositions(ref Vector3[] activePositions, Vector3 distOffset)
    {
        //grab the proper indexes from the object positions array
        //these indexes correspond to (position - distanceFromOriginToCamera)/offset sizeing
        int offset = 0;
        for (int i = 0; i < activePositions.Length; i++)
        {
            //This is a calculation to get the index of objects relative to the camera
            var myIndex = new Vector3(distOffset.x / SizeVector.x, distOffset.y / SizeVector.y, distOffset.z / SizeVector.z);
            int index = (int)((myIndex.x * (SizeVector.y * SizeVector.z)) + (myIndex.y * SizeVector.z) + (myIndex.z));

            //if (i < SizeVector.x)

            //offset = i + (int)(SizeVector.y * SizeVector.z); //Offset to X axis

            // if (i >= SizeVector.x && i < SizeVector.x * SizeVector.y)

            //offset = (i * (int)(SizeVector.z)); //Offset to Y Axis
            // else
            //offset = i; //No offset, order is z axis

            //Flip to X - Axis
            //Order of cubes is YX
            if (true)
            {
                offset = (i * (int)(SizeVector.z));
                if (i >= SizeVector.x * SizeVector.y)
                {
                    //wrap around when number is too large
                    offset = (i % (int)(SizeVector.x * SizeVector.y)) * (int)(SizeVector.z) + i / (int)(SizeVector.x * SizeVector.y);
                }
            }

            //Todo: I need to think about this more. some numbers need to be reduced, not increased
            //How can this be achieved via wrapping?
            //Order of cubes is ZX
            /*
            if (true)
            {
                offset = (i % (int)(SizeVector.z)) + (int)(i / SizeVector.z) * (int)(SizeVector.z * SizeVector.y);
                if (offset >= SizeVector.z * SizeVector.x * SizeVector.y)
                {
                    offset = ((i) % (int)(SizeVector.z)) + (int)((i) / SizeVector.z) * (int)(SizeVector.z * SizeVector.y);
                    offset -= (int)(i / (SizeVector.x * SizeVector.z)) * (int)(SizeVector.z * SizeVector.x * SizeVector.y);
                    offset += (int)(SizeVector.z) * (int)(i / (int)(SizeVector.x * SizeVector.z));
                }

            }
            */
            //Order of cubes is ZY
            // if (true)
            //     offset = i;

            try
            {
                activePositions[i] = objectPositions[offset];
            }
            catch (Exception e)
            {
                //Debug.Log("oops i=" + " offset=" + offset);
            }


        }
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

