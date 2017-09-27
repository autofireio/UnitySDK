using System.Collections.Generic;

namespace AutofireClient.Iface
{

	public abstract class HelperHTTP: IHTTPProvider
	{

		public abstract bool IsOnline ();

		public abstract void SetRequestTimeout (int secs);

		public abstract void PostJSON (string url,
		                               Dictionary<string, string> headers,
		                               string body,
		                               bool forceSync = false);

		protected void HandleHTTPResponse (int statusCode, string responseBody)
		{
			HelperHTTPResponse resp = new HelperHTTPResponse (statusCode, responseBody);
			SessionManager.HandleHTTPResponse (resp);
		}

	}

}
