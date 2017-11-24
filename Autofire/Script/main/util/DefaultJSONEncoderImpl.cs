using System.Collections.Generic;
using AutofireClient.Event;
using AutofireClient.Iface;

namespace AutofireClient.Util
{
	
	public class DefaultJSONEncoderImpl : IBatchEncoderProvider
	{

		public const string CONTENT_TYPE = "application/json";
		public const string BEGIN_OBJECT = "{";
		public const string END_OBJECT = "}";
		public const string EMPTY_OBJECT = BEGIN_OBJECT + END_OBJECT;
		public const string BEGIN_ARRAY = "[";
		public const string END_ARRAY = "]";
		public const string EMPTY_ARRAY = BEGIN_ARRAY + END_ARRAY;
		public const string SEPARATOR = ",";
		public const string ASSIGNMENT = ":";

		private string headerLabel = DefaultJSONEncoderImpl.StringifyStringValue ("header");
		private string tagsLabel = DefaultJSONEncoderImpl.StringifyStringValue ("tags");
		private string eventsLabel = DefaultJSONEncoderImpl.StringifyStringValue ("events");

		public string GetContentType ()
		{
			return CONTENT_TYPE;
		}

		private static string StringifyStringValue (string value)
		{
			return "\"" + value + "\"";
		}

		private string JsonifyDict<TKey, TValue> (IDictionary<TKey, TValue> dictionary, bool isStringValue)
		{
			if (dictionary == null)
				return EMPTY_OBJECT;
			
			int count = dictionary.Count;
			if (count == 0)
				return EMPTY_OBJECT;

			string result = BEGIN_OBJECT;

			int i = 0;
			foreach (KeyValuePair<TKey, TValue> entry in dictionary) {
				string k = entry.Key.ToString ();
				string v = entry.Value.ToString ();
				if (i > 0)
					result += SEPARATOR;
				if (isStringValue)
					v = DefaultJSONEncoderImpl.StringifyStringValue (v);
				result += DefaultJSONEncoderImpl.StringifyStringValue (k) + ASSIGNMENT + v;
				i++;
			}

			result += END_OBJECT;

			return result;
		}

		private string JsonifyList<T> (IList<T> list, bool isStringValue)
		{
			if (list == null)
				return EMPTY_ARRAY;

			int count = list.Count;
			if (count == 0)
				return EMPTY_ARRAY;

			string result = BEGIN_ARRAY;

			int i = 0;
			foreach (T x in list) {
				string v = x.ToString ();
				if (i > 0)
					result += SEPARATOR;
				if (isStringValue)
					v = DefaultJSONEncoderImpl.StringifyStringValue (v);
				result += v;
				i++;
			}

			result += END_ARRAY;

			return result;
		}

		public string EncodeHeader (IDictionary<string, string> header)
		{
			return headerLabel + ASSIGNMENT + JsonifyDict (header, true);
		}

		public string EncodeTags (IList<string> tags)
		{
			return tagsLabel + ASSIGNMENT + JsonifyList (tags, true);
		}

		public string EncodeEvent (RawEvent rawEvent)
		{
			if (rawEvent == null)
				return "";
			
			string required = DefaultJSONEncoderImpl.StringifyStringValue (rawEvent.name) +
			                  SEPARATOR +
			                  DefaultJSONEncoderImpl.StringifyStringValue (rawEvent.timestamp);

			string optional = "";
			if ((rawEvent.nominals != null && rawEvent.nominals.Count != 0) ||
			    (rawEvent.integrals != null && rawEvent.integrals.Count != 0) ||
			    (rawEvent.fractionals != null && rawEvent.fractionals.Count != 0)) {

				optional = SEPARATOR +
				JsonifyDict (rawEvent.nominals, true) + SEPARATOR +
				JsonifyDict (rawEvent.integrals, false) + SEPARATOR +
				JsonifyDict (rawEvent.fractionals, false);
			}

			return BEGIN_ARRAY + required + optional + END_ARRAY;
		}

		public string GetSeparator ()
		{
			return SEPARATOR;
		}

		public string GetBatchBegin ()
		{
			return BEGIN_OBJECT;
		}

		public string GetEventsBegin ()
		{
			return eventsLabel + ASSIGNMENT + BEGIN_ARRAY;
		}

		public string GetEventsEnd ()
		{
			return END_ARRAY;
		}

		public string GetBatchEnd ()
		{
			return END_OBJECT;
		}

	}

}
