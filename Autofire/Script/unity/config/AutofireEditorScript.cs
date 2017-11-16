using System.Collections.Generic;
using System.IO;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AutofireClient.Unity.Util
{
	
	#if UNITY_EDITOR
	[InitializeOnLoad]
	#endif
    public class AutofireEditorScript : ScriptableObject
	{
		
		public const string GAME_ID_DEFAULT = "ENTER YOUR GAME ID";

		const string SETTINGS_ASSET_NAME = "AutofireEditorScript";
		const string SETTINGS_PATH = "Autofire/Resources";
		const string SETTINGS_ASSET_EXT = ".asset";

		private static AutofireEditorScript instance;

		public static AutofireEditorScript Instance {
			get {
				if (instance == null) {
					instance = Resources.Load (SETTINGS_ASSET_NAME) as AutofireEditorScript;

					if (instance == null) {
						// If not found, autocreate the asset object.
						instance = CreateInstance<AutofireEditorScript> ();
#if UNITY_EDITOR
						string properPath = Path.Combine (Application.dataPath, SETTINGS_PATH);
						if (!Directory.Exists (properPath))
							AssetDatabase.CreateFolder ("Assets/Autofire", "Resources");

						string fullPath = Path.Combine (Path.Combine ("Assets", SETTINGS_PATH),
							                  SETTINGS_ASSET_NAME + SETTINGS_ASSET_EXT);
						AssetDatabase.CreateAsset (instance, fullPath);
#endif
					}
				}
				return instance;
			}
		}

		#if UNITY_EDITOR
		private static Dictionary<string, string[]> fileList = new Dictionary<string, string[]> ();

		public static void AddFileList (string moduleId, string fileListPath, string[] additionalFiles)
		{
			List<string> foldersFiles = new List<string> ();
			foldersFiles.Add (fileListPath);
			foldersFiles.AddRange (additionalFiles);
			string line;
			StreamReader reader = new StreamReader (fileListPath);
			do {
				line = reader.ReadLine ();
				if (line != null) {
					foldersFiles.Add (line);
				}
			} while (line != null);
			reader.Close ();
			fileList.Add (moduleId, foldersFiles.ToArray ());
		}

		private static AutofireSettings autofireSettings = new AutofireSettings ();

		public static void OnInspectorGUI ()
		{
			autofireSettings.OnAutofireGUI ();
		}

		[MenuItem ("Window/Autofire/Settings")]
		public static void Edit ()
		{
			Selection.activeObject = Instance;
		}

		[MenuItem ("Window/Autofire/Documentation")]
		public static void OpenDocs ()
		{
			string url = "https://autofire.io/documentation/sdk/get-started-sdk/?platform=unity";
			Application.OpenURL (url);
		}

		[MenuItem ("Window/Autofire/Demo")]
		public static void OpenDemo ()
		{
			string url = "https://autofire.io/#inAction";
			Application.OpenURL (url);
		}

		//[MenuItem("Window/Autofire/Report an issue")]
		//public static void OpenIssue()
		//{
		//    //string url = "https://answers.autofire.io";
		//    //Application.OpenURL(url);
		//}

		[MenuItem ("Window/Autofire/Remove Editor Script")]
		public static void Remove ()
		{
			string fullPath = Path.Combine (Path.Combine ("Assets", SETTINGS_PATH),
				                  SETTINGS_ASSET_NAME + SETTINGS_ASSET_EXT);
			if (EditorUtility.DisplayDialog (
				    "Confirmation",
				    "Are you sure you want to remove Autofire from your project?",
				    "Yes",
				    "No")) {
				AssetDatabase.DeleteAsset (fullPath);
				foreach (KeyValuePair<string, string[]> attachStat in fileList) {
					RemoveModule (attachStat.Value);
				}
			}
		}

		[MenuItem ("Window/Autofire/Sign Up")]
		public static void SignUp ()
		{
			string url = "https://webapp.autofire.io/#register";
			Application.OpenURL (url);
		}

		[MenuItem ("Window/Autofire/Sign In")]
		public static void LogIn ()
		{
			string url = "https://webapp.autofire.io";
			Application.OpenURL (url);
		}

		public static void DirtyEditor ()
		{
			EditorUtility.SetDirty (Instance);
		}

		/** Autofire UI **/
		public static GUILayoutOption FieldHeight = GUILayout.Height (16);
		public static GUILayoutOption FieldWidth = GUILayout.Width (120);
		public static GUILayoutOption SpaceWidth = GUILayout.Width (24);
		public static GUIContent EmptyContent = new GUIContent ("");

		public static void RemoveModule (string[] filePaths)
		{
			List<string> folders = new List<string> ();
			foreach (string file in filePaths) {
				FileUtil.DeleteFileOrDirectory (file);
				string folderPath = Path.GetDirectoryName (file);
				do {
					if (!folders.Contains (folderPath)) {
						folders.Add (folderPath);
					}
					folderPath = Path.GetDirectoryName (folderPath);
				} while (folderPath != "");
			}
			folders.Sort ((a, b) => b.Length.CompareTo (a.Length));
			foreach (string fPath in folders) {
				AssetDatabase.Refresh ();
				if (Directory.Exists (fPath)) {
					if (System.IO.Directory.GetFiles (fPath).Length == 0) {
						FileUtil.DeleteFileOrDirectory (fPath);
					}
				}
			}

			AssetDatabase.Refresh ();
		}

		public static void SetConfigValue (string prefix, string key, string value)
		{
			PlayerPrefs.SetString (PersistenceImpl.PREFIX + prefix + "." + key, value);
			PlayerPrefs.Save ();
		}

		public static string GetConfigValue (string prefix, string key)
		{
			string value;
			value = PlayerPrefs.GetString (PersistenceImpl.PREFIX + prefix + "." + key);
			SetConfigValue (prefix, key, value);
			return value.Length > 0 ? value : null;
		}
		#endif

	}

}
