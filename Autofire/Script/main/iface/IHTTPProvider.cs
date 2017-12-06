using System.Collections.Generic;

namespace AutofireClient.Iface
{

	public interface IHTTPProvider
	{

		bool IsOnline ();

		void SetRequestTimeout (int secs);

		void PostData (HelperHTTPResponseHandler responseHandler,
		               string url,
		               string contentType,
		               string acceptType,
		               Dictionary<string, string> headers,
		               string body,
		               bool forceSync = false);

	}

}
