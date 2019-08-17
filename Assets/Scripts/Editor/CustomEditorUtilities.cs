using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CustomEditorGUI
{
    static class CustomEditorUtilities
    {


        internal static void AuthenticateField(bool boolean, string msg)
        {
            if (boolean)
                return;

            GUIStyle s = new GUIStyle(EditorStyles.label);
            s.normal.textColor = Color.red;

            EditorGUILayout.LabelField(msg, s);
        }

    }
}