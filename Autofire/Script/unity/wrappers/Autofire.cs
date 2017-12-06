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
				SessionManager.PersistToDisk ();
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
				new PrefsPersistenceImpl (),
				new DefaultGUIDImpl (),
				new DefaultJSONEncoderImpl (),
				gameObject.AddComponent<HTTPImpl> () as HTTPImpl);
		}

		public void Initialize (string gameId)
		{
			Setup ();
			SessionManager.Initialize (new Initializer (gameId));
		}

		public static void Progress (string level, int score)
		{
			SessionManager.SendEvent (new Progress (level, score));
		}

		public static void Monetize (string item, int ac, int qty)
		{
			SessionManager.SendEvent (new Monetize (item, ac, qty));
		}

		public static void Monetize (string item, int ac)
		{
			Monetize (item, ac, 1);
		}

		public static void Resource (string name, int qty)
		{
			SessionManager.SendEvent (new Resource (name, qty));
		}

		public static void Action (string what)
		{
			SessionManager.SendEvent (new Action (what));
		}

		public static void Flush ()
		{
			SessionManager.FlushEvents ();
		}

		public static void Finish ()
		{
			SessionManager.Deinitialize ();
			SessionManager.Shutdown ();
		}

	}

}
