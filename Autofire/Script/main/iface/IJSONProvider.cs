using System.Collections.Generic;
using AutofireClient.Event;

namespace AutofireClient.Iface
{

	public interface IJSONProvider
	{

		string JsonifyHeader (Dictionary<string, string> header);

		string JsonifyTags (List<string> tags);

		string JsonifyEvent (RawEvent rawEvent);

	}

}
