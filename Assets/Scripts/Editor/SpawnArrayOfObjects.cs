//C# Example
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using CustomEditorGUI;
using System;

public class SpawnArrayWindow : EditorWindow
{
	public UnityEngine.GameObject ObjectToSpawn;
	public UnityEngine.GameObject prefab;

	GameObject container;
	string ContainerName = "Container";
	string ObjectName = "";

	float z = 0;

	int X_Number = 0;
	int Y_Number = 0;
	int Z_Number = 0;

	bool X_useBounds = true;
	bool Y_useBounds = true;
	bool Z_useBounds = true;

	float X_Spacing;
	float Y_Spacing;
	float Z_Spacing;

	float X_Offset = 0;
	float Y_Offset = 0;
	float Z_Offset = 0;

	/*
    * Axes to use is an int representing which axes to create the grid on
    * Evalutaion of this int is determined after the dimensionality.
    * 1D: x = 3,y = 2,z = 1;
    * 2D  : xy = 1, yz = 3, xz = 2
    * 3D  : xyz = 3;
    */
	private int index;

	[MenuItem("Tools/SpawnArrayOfObjects")]
	public static void ShowWindow()
	{
		//Creates the window
		SpawnArrayWindow window = ScriptableObject.CreateInstance(typeof(SpawnArrayWindow)) as SpawnArrayWindow;
		//Show editor window as floating utility. Undockable window, but will always stay on top.
		window.ShowUtility();
	}

	void OnGUI()
	{
		//Label the window
		GUILayout.Label("Spawn Array of Objects", EditorStyles.boldLabel);

		//Gameobject entry field
		ObjectToSpawn = EditorGUILayout.ObjectField("Object To Spawn:", ObjectToSpawn, typeof(UnityEngine.Object), true) as GameObject;
		CustomEditorUtilities.AuthenticateField(ObjectToSpawn, "Insert Object to Spawn"); //authenticates field, makes it requried with message

		//return if we don't have an object. no need to show other information until we get one
		if (ObjectToSpawn == null)
		{
			return;
		}
		//if object is not a prefab we need to make it a prefab before continuing
		if (PrefabUtility.GetCorrespondingObjectFromOriginalSource(ObjectToSpawn) != null)
		{
			Debug.Log("YES");
			prefab = PrefabUtility.GetCorrespondingObjectFromOriginalSource(ObjectToSpawn);
		}
		else if (PrefabUtility.GetCorrespondingObjectFromSource(ObjectToSpawn) != null)
		{
			Debug.Log("YES");
			prefab = PrefabUtility.GetCorrespondingObjectFromSource(ObjectToSpawn);
		}
		else
		{
			// prefab is null;
			Debug.Log("NO");

			EditorGUILayout.HelpBox("You must first make the object into a prefab before creating an array", MessageType.Info);

			if (GUILayout.Button("Make Prefab"))
				OnMakePrefabButtonPressed();
			return;
		}

		ObjectName = prefab.name;
		//string fields, autopopulated.
		ContainerName = EditorGUILayout.TextField("Container Name", ContainerName); //name of the container(parent gameobject)
		ObjectName = EditorGUILayout.TextField("Object Name", ObjectName);//name of the children gameobject.
																		  //TODO: naming needs to be fixed. options for naming as dropdown and some default(x,y,z?)

		//starts a horizontal layout group
		EditorGUILayout.BeginHorizontal();
		GUILayout.Label("Number");
		GUILayout.Space(29);
		EditorGUIUtility.labelWidth = 10; //sets size of input field
		X_Number = Math.Abs(EditorGUILayout.IntField("X", X_Number));
		EditorGUIUtility.labelWidth = 10;
		Y_Number = Math.Abs(EditorGUILayout.IntField("Y", Y_Number));
		EditorGUIUtility.labelWidth = 10;
		Z_Number = Math.Abs(EditorGUILayout.IntField("Z", Z_Number));
		EditorGUILayout.EndHorizontal();
		//end horizontal group
		CustomEditorUtilities.AuthenticateField(X_Number + Y_Number + Z_Number > 0, "Select a number of objects to spawn");

		EditorGUILayout.BeginHorizontal();
		GUILayout.Label("UseBounds");

		//begins a disabled group, using the boolean parameter to determine activation
		EditorGUI.BeginDisabledGroup(X_Number <= 0);
		X_useBounds = EditorGUILayout.Toggle(X_useBounds);
		EditorGUI.EndDisabledGroup();

		EditorGUIUtility.labelWidth = 1;
		EditorGUI.BeginDisabledGroup(Y_Number <= 0);
		Y_useBounds = EditorGUILayout.Toggle(Y_useBounds);
		EditorGUI.EndDisabledGroup();

		EditorGUIUtility.labelWidth = 1;
		EditorGUI.BeginDisabledGroup(Z_Number <= 0);
		EditorGUIUtility.labelWidth = 10;
		Z_useBounds = EditorGUILayout.Toggle(Z_useBounds);
		EditorGUI.EndDisabledGroup();

		EditorGUILayout.EndHorizontal();

		if (X_useBounds || Y_useBounds || Z_useBounds)
		{
			EditorGUILayout.BeginHorizontal();
			EditorGUI.BeginDisabledGroup(true);
			//begins a disabled group, using the boolean parameter to determine activation
			GUILayout.Label("Bounds");

			EditorGUIUtility.labelWidth = 10;
			EditorGUILayout.FloatField("X", GetBounds(prefab).extents.x, GUILayout.ExpandWidth(false));

			EditorGUIUtility.labelWidth = 10;
			EditorGUILayout.FloatField("Y", GetBounds(prefab).extents.y, GUILayout.ExpandWidth(false));

			EditorGUIUtility.labelWidth = 10;
			EditorGUILayout.FloatField("Z", GetBounds(prefab).extents.z, GUILayout.ExpandWidth(false));

			EditorGUI.EndDisabledGroup();
			EditorGUILayout.EndHorizontal();


			EditorGUILayout.BeginHorizontal();
			//begins a disabled group, using the boolean parameter to determine activation
			GUILayout.Label("Offset   ");

			EditorGUIUtility.labelWidth = 1;
			EditorGUI.BeginDisabledGroup(!X_useBounds);
			EditorGUIUtility.labelWidth = 10;
			X_Offset = EditorGUILayout.FloatField("X", X_Offset, GUILayout.ExpandWidth(false));
			EditorGUI.EndDisabledGroup();

			EditorGUIUtility.labelWidth = 1;
			EditorGUI.BeginDisabledGroup(Y_Number > 0 && !Y_useBounds);
			EditorGUIUtility.labelWidth = 10;
			Y_Offset = EditorGUILayout.FloatField("Y", Y_Offset);
			EditorGUI.EndDisabledGroup();

			EditorGUIUtility.labelWidth = 1;
			EditorGUI.BeginDisabledGroup(Z_Number > 0 && !Z_useBounds);
			EditorGUIUtility.labelWidth = 10;
			Z_Offset = EditorGUILayout.FloatField("Z", Z_Offset);
			EditorGUI.EndDisabledGroup();

			EditorGUILayout.EndHorizontal();
		}

		if (!X_useBounds)
		{
			X_Spacing -= X_Offset;
			X_Offset = 0;
		}
		if (!Y_useBounds)
		{
			Y_Spacing -= Y_Offset;
			Y_Offset = 0;
		}
		if (!Z_useBounds)
		{
			Z_Spacing -= Z_Offset;
			Z_Offset = 0;
		}

		EditorGUILayout.BeginHorizontal();
		GUILayout.Label("Spacing");
		//begins a disabled group, using the boolean parameter to determine activation
		EditorGUI.BeginDisabledGroup(X_Number <= 0 || X_useBounds);
		if (X_Number <= 0)
			X_Spacing = 0;
		if (X_useBounds)
		{
			X_Spacing = GetBounds(prefab).extents.x + X_Offset;
		}
		EditorGUIUtility.labelWidth = 10;
		X_Spacing = EditorGUILayout.FloatField("X", X_Spacing, GUILayout.ExpandWidth(false));
		EditorGUI.EndDisabledGroup();

		EditorGUIUtility.labelWidth = 1;
		EditorGUI.BeginDisabledGroup(Y_Number <= 0 || Y_useBounds);
		if (Y_Number <= 0)
			Y_Spacing = 0;
		if (Y_useBounds)
		{
			Y_Spacing = GetBounds(prefab).extents.y + Y_Offset;
		}
		EditorGUIUtility.labelWidth = 10;
		Y_Spacing = EditorGUILayout.FloatField("Y", Y_Spacing);
		EditorGUI.EndDisabledGroup();

		EditorGUIUtility.labelWidth = 1;
		EditorGUI.BeginDisabledGroup(Z_Number <= 0 || Z_useBounds);
		if (Z_Number <= 0)
			Z_Spacing = 0;
		if (Z_useBounds)
		{
			Z_Spacing = GetBounds(prefab).extents.z + Z_Offset;
		}
		EditorGUIUtility.labelWidth = 10;
		Z_Spacing = EditorGUILayout.FloatField("Z", Z_Spacing);
		EditorGUI.EndDisabledGroup();

		EditorGUILayout.EndHorizontal();

		//Before we create the grid, check that we aren't creating too many gameobjects
		var numVector = new Vector3(X_Number, Y_Number, Z_Number);
		float gameobjectCount = ((1 * numVector) + (((GameObject)prefab).transform.childCount * numVector)).x *
			((1 * numVector) + (((GameObject)prefab).transform.childCount * numVector)).y *
			((1 * numVector) + (((GameObject)prefab).transform.childCount * numVector)).z;

		var maxNum = 100000;
		if (gameobjectCount >= maxNum)
			EditorGUILayout.HelpBox(gameobjectCount + " gameobjects is likely too many to spawn and will crash Unity", MessageType.Error);

		if (prefab && X_Number + Y_Number + Z_Number > 0)
		{
			EditorGUI.BeginDisabledGroup(gameobjectCount >= maxNum);

			if (GUILayout.Button("Create"))
				OnCreateButtonPressed();
			EditorGUI.EndDisabledGroup();

		}
	}

	private void OnMakePrefabButtonPressed()
	{
		// Set the path as within the Assets folder,
		// and name it as the GameObject's name with the .Prefab format
		string localPath = "Assets/Prefabs/" + ObjectToSpawn.name + ".prefab";

		// Make sure the file name is unique, in case an existing Prefab has the same name.
		localPath = AssetDatabase.GenerateUniqueAssetPath(localPath);

		// Create the new Prefab.
		PrefabUtility.SaveAsPrefabAssetAndConnect(ObjectToSpawn, localPath, InteractionMode.UserAction);
	}

	private Bounds GetBounds(GameObject go)
	{
		var bounds = new Bounds();

		//first try getting the bounds of the bounds of the root gameobject
		try
		{
			bounds.size = go.GetComponent<MeshRenderer>().bounds.extents * 4;
		}
		catch (Exception e)
		{
			MeshRenderer[] renderers = go.GetComponentsInChildren<MeshRenderer>();
			foreach (MeshRenderer mr in renderers)
			{
				bounds.Encapsulate(mr.bounds);
			}
			bounds.size = bounds.size * 2;
		}

		return bounds;

	}

	void OnCreateButtonPressed()
	{
		List<int> myList = new List<int> { X_Number, Y_Number, Z_Number };

		index = 0;
		//Figure out which axes to use
		for (int i = 0; i < myList.Count; i++)
		{
			if (myList[i] > 0)
				index += i;
		}
		//Remove the 0's from the list
		myList.RemoveAll((int number) => number == 0);

		//create empty container gameobject
		GenerateGrid(new Vector3(X_Number, Y_Number, Z_Number), myList.Count);
	}

	void GenerateGrid(Vector3 numbers, int dimensions)
	{
		container = new GameObject(ContainerName);
		switch (dimensions)
		{
			case 1:
				for (int row = 0; row < numbers[index]; row++)
				{
					CreateNewInstance(prefab, row, 0, 0);
				}
				break;
			case 2:
				//Do for two
				//adjust index for 2 dimensions
				index = index - 1;

				//1D array pretending to be a 3D array
				int[] map = { 0, 1, 2, 0, 2, 1, 1, 2, 2 };
				Debug.Log("Index " + index + " " + numbers[map[index + 3]] + " " + numbers[map[index + 6]]);
				//get the second index
				int complement = map[index * 2 + 1];

				for (int row = 0; row < numbers[map[index + 3]]; row++)
				{
					for (int col = 0; col < numbers[map[index + 6]]; col++)
					{
						CreateNewInstance(prefab, row, col, 0);
					}
				}
				break;
			case 3:
				//Do for three
				for (int row = 0; row < numbers[0]; row++)
				{
					for (int col = 0; col < numbers[1]; col++)
					{
						for (int depth = 0; depth < numbers[2]; depth++)
						{
							CreateNewInstance(prefab, row, col, depth);
						}
					}
				}
				break;

		}
	}

	private void CreateNewInstance(GameObject p, int row, int col, int depth)
	{
		GameObject tmp = (GameObject)PrefabUtility.InstantiatePrefab(p);
		tmp.name = ObjectName + " " + row;
		tmp.transform.parent = container.transform;
		tmp.transform.position = container.transform.position;


		tmp.transform.position = AssignObjectPosition(row, col, depth);
	}

	private Vector3 AssignObjectPosition(int row, int col, int depth)
	{
		return new Vector3(container.transform.position.x + X_Spacing * row,
					   container.transform.position.y + Y_Spacing * col,
					   container.transform.position.z + Z_Spacing * depth);
	}



}
