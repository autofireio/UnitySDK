using System.Collections.Generic;
using AutofireClient.Event;

namespace AutofireClient
{

	public sealed class Initializer
	{

		private const int MAX_HEADERS = 0;
		private const int MAX_TAGS = 4;

		internal string gameId;
		internal string playerId = "";
		internal long timestamp = 0L;
		internal Dictionary<string, string> headers = new Dictionary<string, string> ();
		internal List<string> tags = new List<string> ();

		public Initializer (string gameId)
		{
			this.gameId = gameId;
			this.timestamp = GameEvent.NowFromEpoch ();
		}

		public Initializer WithPlayerId (string playerId)
		{
			this.playerId = playerId;
			return this;
		}

		public Initializer WithTimestamp (long timestamp)
		{
			this.timestamp = timestamp;
			return this;
		}

		public Initializer WithPlatform (string platform)
		{
			headers.Add (Header.PLATFORM_KEY, GameEvent.SanitizeNominalValue (platform));
			return this;
		}

		public Initializer WithOs (string os)
		{
			headers.Add (Header.OS_KEY, GameEvent.SanitizeNominalValue (os));
			return this;
		}

		public Initializer WithModel (string model)
		{
			headers.Add (Header.MODEL_KEY, GameEvent.SanitizeNominalValue (model));
			return this;
		}

		public Initializer WithLocale (string locale)
		{
			headers.Add (Header.LOCALE_KEY, GameEvent.SanitizeNominalValue (locale));
			return this;
		}

		public Initializer WithVersion (string version)
		{
			headers.Add (Header.VERSION_KEY, GameEvent.SanitizeNominalValue (version));
			return this;
		}

		public Initializer WithHeader (string key, string value)
		{
			if (headers.Count >= MAX_HEADERS)
				return this;
			
			headers.Add (GameEvent.SanitizeKey (key), GameEvent.SanitizeNominalValue (value));
			return this;
		}

		public Initializer WithTag (string tag)
		{
			if (tags.Count >= MAX_TAGS)
				return this;

			string t = GameEvent.SanitizeName (tag);
			if (tags.IndexOf (t) == -1)
				tags.Add (t);
			
			return this;
		}

	}

}
