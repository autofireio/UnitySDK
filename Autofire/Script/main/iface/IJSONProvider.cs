using System.Collections.Generic;
using AutofireClient.Event;

namespace AutofireClient.Iface
{

	public interface IJSONProvider
	{

		string JsonifyHeader (IDictionary<string, string> header);

		string JsonifyTags (IList<string> tags);

		string JsonifyEvent (RawEvent rawEvent);

	}

}
