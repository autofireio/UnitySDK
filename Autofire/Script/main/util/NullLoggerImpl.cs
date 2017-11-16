using AutofireClient.Iface;

namespace AutofireClient.Util
{

	public class NullLoggerImpl : ILoggerProvider
	{

		public void LogDebug (string tag, string what)
		{
		}

	}

}
