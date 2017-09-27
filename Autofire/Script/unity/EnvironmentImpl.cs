using UnityEngine;
using AutofireClient.Iface;

namespace AutofireClient.Unity
{

	public class EnvironmentImpl : IEnvironmentProvider
	{

		public string GetPlatform ()
		{
			return "Unity-" + Application.platform.ToString ();
		}

		public string GetOs ()
		{
			return SystemInfo.operatingSystem;
		}

		public string GetModel ()
		{
			return SystemInfo.deviceModel;
		}

		public string GetLocale ()
		{
			return Application.systemLanguage.ToString ();
		}

		public string GetVersion ()
		{
			return Application.version;
		}

	}

}
