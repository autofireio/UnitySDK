using UnityEngine;
using AutofireClient.Util;

namespace AutofireClient.Unity
{

	public class Manager : MonoBehaviour
	{

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

		public void Flush ()
		{
			SessionManager.FlushEvents ();
		}

		public void Finish ()
		{
			SessionManager.Deinitialize ();
		}

	}

}
