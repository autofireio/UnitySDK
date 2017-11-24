using System.Collections.Generic;
using AutofireClient.Event;

namespace AutofireClient.Iface
{

	public interface IBatchEncoderProvider
	{

		string GetContentType ();

		string EncodeHeader (IDictionary<string, string> header);

		string EncodeTags (IList<string> tags);

		string EncodeEvent (RawEvent rawEvent);

		string GetSeparator ();

		string GetBatchBegin ();

		string GetEventsBegin ();

		string GetEventsEnd ();

		string GetBatchEnd ();

	}

}
