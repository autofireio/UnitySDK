using UnityEngine;
using AutofireClient.Event;
using AutofireClient.Unity.Util;

namespace AutofireClient.Unity
{

	public class Autofire : MonoBehaviour
	{

		private Manager autofire;

		private string gameId;

		void Awake ()
		{
			if (FindObjectsOfType (GetType ()).Length > 1)
				Destroy (gameObject);

			gameId = AutofireSettings.GetGameId ();

			autofire = gameObject.AddComponent<Manager> () as Manager;
			DontDestroyOnLoad (this.gameObject);
		}

		void Start ()
		{
			if (!string.IsNullOrEmpty (gameId))
				autofire.Initialize (gameId);
		}

		void OnApplicationPause (bool pauseStatus)
		{
			if (pauseStatus)
				autofire.Flush ();
		}

		void OnApplicationQuit ()
		{
			autofire.Finish ();
		}

		public static void Progress (string level, int score)
		{
			new Progress (level, score).Send ();
		}

		public static void Monetize (string item, int ac, int qty)
		{
			new Monetize (item, ac, qty).Send ();
		}

		public static void Monetize (string item, int ac)
		{
			Autofire.Monetize (item, ac, 1);
		}

		public static void Resource (string name, int qty)
		{
			new Resource (name, qty).Send ();
		}

		public static void Action (string what)
		{
			new Action (what).Send ();
		}

	}

}
