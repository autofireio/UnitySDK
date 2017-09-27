using System;
using AutofireClient.Iface;

namespace AutofireClient.Util
{

	public class DefaultGUIDImpl : IGUIDProvider
	{

		public string NewGUID ()
		{
			return Guid.NewGuid ().ToString ();
		}

	}

}
