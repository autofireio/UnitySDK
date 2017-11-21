using UnityEngine;
using AutofireClient.Event;
using AutofireClient.Util;
using AutofireClient.Unity.Util;

namespace AutofireClient.Unity
{

	public class Autofire : MonoBehaviour
	{

		private string gameId;

		void Awake ()
		{
			if (FindObjectsOfType (GetType ()).Length > 1)
				Destroy (gameObject);

			gameId = AutofireSettings.GetGameId ();

			DontDestroyOnLoad (gameObject);
		}

		void Start ()
		{
			if (!string.IsNullOrEmpty (gameId))
				Initialize (gameId);
		}

		void OnApplicationPause (bool pauseStatus)
		{
			if (pauseStatus)
				Flush ();
		}

		void OnApplicationQuit ()
		{
			Finish ();
		}

		public void Setup ()
		{
			SessionManager.SetProviders (
				new LoggerImpl (),
				new EnvironmentImpl (),
				new PersistenceImpl (),
				new DefaultGUIDImpl (),
				new DefaultJSONImpl (),
				gameObject.AddComponent<HTTPImpl> () as HTTPImpl);
		}

		public void Initialize (string gameId)
		{
			Setup ();
			SessionManager.Initialize (new Initializer (gameId));
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

		public static void Flush ()
		{
			SessionManager.FlushEvents ();
		}

		public static void Finish ()
		{
			SessionManager.Deinitialize ();
		}

	}

}
