using System.Collections.Generic;
using AutofireClient.Iface;

namespace AutofireClient.Util
{
	
	public class DummyHTTPImpl : IHTTPProvider
	{

		private static bool online = true;
		private static int flakyEvery = 0;
		private static int notFoundEvery = 0;
		private static int n = 0;

		public static void SetOnline (bool online)
		{
			DummyHTTPImpl.online = online;
		}

		public static void SetFlakyEvery (int flakyEvery)
		{
			DummyHTTPImpl.flakyEvery = flakyEvery;
		}

		public static void SetNotFoundEvery (int notFoundEvery)
		{
			DummyHTTPImpl.notFoundEvery = notFoundEvery;
		}

		public bool IsOnline ()
		{
			return online;
		}

		public void SetRequestTimeout (int secs)
		{
		}

		private void HandleHTTPResponse (HelperHTTPResponseHandler responseHandler,
		                                 int code,
		                                 string body)
		{
			HelperHTTPResponse response = new HelperHTTPResponse (code, body);
			responseHandler.HandleResponse (response);
		}

		public void PostData (HelperHTTPResponseHandler responseHandler,
		                      string url,
		                      string contentType,
		                      string acceptType,
		                      Dictionary<string, string> headers,
		                      string body,
		                      bool forceSync = false)
		{
			if (!online)
				HandleHTTPResponse (responseHandler, 0, null);
			else {
				n++;
				if (flakyEvery > 0 && n % flakyEvery == 0)
					HandleHTTPResponse (responseHandler, 500, "Internal Server Error");
				else if (notFoundEvery > 0 && n % notFoundEvery == 0)
					HandleHTTPResponse (responseHandler, 404, "Not Found");
				else
					HandleHTTPResponse (responseHandler, 200, "OK");
			}
		}

	}

}
