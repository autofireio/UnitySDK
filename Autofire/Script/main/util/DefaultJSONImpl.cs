using System.Collections.Generic;
using AutofireClient.Event;
using AutofireClient.Iface;

namespace AutofireClient.Util
{
	
	public class DefaultJSONImpl : IJSONProvider
	{
		
		private string JsonifyDict<TKey, TValue> (IDictionary<TKey, TValue> dictionary, bool isStringValue)
		{
			string result = "{";
			int i = 0;
			foreach (KeyValuePair<TKey, TValue> entry in dictionary) {				
				string v = entry.Value.ToString ();
				if (i > 0)
					result += ", ";
				if (isStringValue)
					v = "\"" + v + "\"";
				result += "\"" + entry.Key + "\"" + ":" + v;
				i++;
			}
			result += "}";

			return result;
		}

		public string JsonifyHeader (IDictionary<string, string> header)
		{
			return JsonifyDict (header, true);
		}

		public string JsonifyTags (IList<string> tags)
		{
			int count = tags.Count;
			if (count == 0)
				return "[]";

			string result = "[";
			foreach (string i in tags) {
				result += "\"" + i + "\",";
			}

			return result.Remove (result.Length - 1) + "]";
		}

		public string JsonifyEvent (RawEvent rawEvent)
		{
			string required = "\"" + rawEvent.name + "\",\"" + rawEvent.timestamp + "\"";
			string optional = "";
			if (rawEvent.nominals.Count != 0 ||
			    rawEvent.integrals.Count != 0 ||
			    rawEvent.fractionals.Count != 0) {

				optional = "," +
				JsonifyDict (rawEvent.nominals, true) + "," +
				JsonifyDict (rawEvent.integrals, false) + "," +
				JsonifyDict (rawEvent.fractionals, false);
			}

			return "[" + required + optional + "]";
		}

	}

}
