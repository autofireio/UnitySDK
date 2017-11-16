using System.Collections.Generic;
using AutofireClient.Iface;

namespace AutofireClient.Util
{
	
	public class DummyHTTPImpl : HelperHTTP
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

		public override bool IsOnline ()
		{
			return online == true;
		}

		public override void SetRequestTimeout (int secs)
		{
		}

		public override void PostJSON (string url,
		                               Dictionary<string, string> headers,
		                               string body,
		                               bool forceSync = false)
		{
			if (!online)
				HandleHTTPResponse (0, null);
			else {
				n++;
				if (flakyEvery > 0 && n % flakyEvery == 0)
					HandleHTTPResponse (500, "Internal Server Error");
				else if (notFoundEvery > 0 && n % notFoundEvery == 0)
					HandleHTTPResponse (404, "Not Found");
				else
					HandleHTTPResponse (200, "OK");
			}
		}

	}

}
