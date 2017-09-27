using UnityEngine;
using AutofireClient.Iface;

namespace AutofireClient.Unity
{

	public class LoggerImpl : ILoggerProvider
	{

		public void LogDebug (string tag, string what)
		{
			Debug.Log (tag + ": " + what);
		}

	}

}
