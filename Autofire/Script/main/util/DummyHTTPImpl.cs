using System.Collections.Generic;
using AutofireClient.Iface;

namespace AutofireClient.Util
{
	
	public class DummyHTTPImpl : HelperHTTP
	{
		
		public override bool IsOnline ()
		{
			return true;
		}

		public override void SetRequestTimeout (int secs)
		{
		}

		public override void PostJSON (string url,
		                               Dictionary<string, string> headers,
		                               string body,
		                               bool forceSync = false)
		{
			HandleHTTPResponse (200, "OK");
		}

	}

}
