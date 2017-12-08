using System;
using AutofireClient.Iface;

namespace AutofireClient.Util
{

	public class StdoutLoggerImpl : ILoggerProvider
	{

		public void LogDebug (string tag, string what)
		{
			Console.WriteLine (tag + ": " + what);
		}

	}

}
