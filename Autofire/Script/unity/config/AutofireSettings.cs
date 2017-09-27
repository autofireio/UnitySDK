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
    public class AutofireSettings
	{

		internal static string GetGameIdFile ()
		{
			return Application.dataPath + @"/Autofire/Resources/gameid.txt";
		}

		#if UNITY_EDITOR
		private static string gameId;

		public static string GameId {
			get {
				if (gameId == null) {
					gameId = GetGameId ();
					if (gameId == null) {
						gameId = AutofireEditorScript.GAME_ID_DEFAULT;
					}
				}
				return gameId;
			}
			set {
				if (gameId != value) {
					gameId = value;
					SetGameId (value.ToString ());
					AutofireEditorScript.DirtyEditor ();
				}
			}
		}

		GUIContent gameIdLabel = new GUIContent ("Game Id [?]:", "Your Autofire Game Id");
		GUIContent versionLabel = new GUIContent ("Autofire Unity SDK Version: " + AutofireClient.Version.VERSION);

		public void OnAutofireGUI ()
		{
			FileStream fs = new FileStream (Application.dataPath + @"/Autofire/Resources/autofire_logo.png",
				                FileMode.Open,
				                FileAccess.Read);
			byte[] imageData = new byte[fs.Length];
			fs.Read (imageData, 0, (int)fs.Length);
			Texture2D logoTexture = new Texture2D (300, 92);
			logoTexture.LoadImage (imageData);

			EditorGUILayout.BeginHorizontal ();
			GUIContent logoImgLabel = new GUIContent (logoTexture);
			EditorGUILayout.LabelField (logoImgLabel, GUILayout.MaxHeight (70), GUILayout.ExpandWidth (true));
			EditorGUILayout.EndHorizontal ();

			GameObject.DestroyImmediate (logoTexture);

			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.LabelField (versionLabel, GUILayout.MaxHeight (70), GUILayout.ExpandWidth (true));
			EditorGUILayout.EndHorizontal ();

			//EditorGUILayout.HelpBox ("Please fill in the needed information below", MessageType.None);

			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.LabelField (gameIdLabel, AutofireEditorScript.FieldWidth, AutofireEditorScript.FieldHeight);
			GameId = EditorGUILayout.TextField (GameId, AutofireEditorScript.FieldHeight);
			EditorGUILayout.EndHorizontal ();

			EditorGUILayout.Space ();
		}

		public static void SetGameId (string value)
		{
			File.WriteAllText (GetGameIdFile (), value);
		}
		#endif

		public static string GetGameId ()
		{
			if (!File.Exists (GetGameIdFile ()))
				return null;
			
			string id = File.ReadAllText (GetGameIdFile ());
			return id;
		}

	}

}
