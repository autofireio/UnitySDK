using AutofireClient.Iface;

namespace AutofireClient.Util
{

	public class DummyEnvironmentImpl : IEnvironmentProvider
	{

		public string GetPlatform ()
		{
			return "";
		}

		public string GetOs ()
		{
			return "";
		}

		public string GetModel ()
		{
			return "";
		}

		public string GetLocale ()
		{
			return "";
		}

		public string GetVersion ()
		{
			return "";
		}

	}

}
