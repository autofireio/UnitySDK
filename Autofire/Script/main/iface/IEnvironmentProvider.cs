namespace AutofireClient.Iface
{

	public interface IEnvironmentProvider
	{

		string GetPlatform ();

		string GetOs ();

		string GetModel ();

		string GetLocale ();

		string GetVersion ();

	}

}
