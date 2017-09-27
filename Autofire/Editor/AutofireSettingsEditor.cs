using UnityEngine;
using UnityEditor;
using AutofireClient.Unity.Util;

[CustomEditor (typeof(AutofireEditorScript))]
public class AutofireSettingsEditor : Editor
{

	public override void OnInspectorGUI ()
	{
		AutofireEditorScript.OnInspectorGUI ();
	}

	public void OnDisable ()
	{
	}

}
