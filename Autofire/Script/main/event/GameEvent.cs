using System;
using System.Collections.Generic;

namespace AutofireClient.Event
{
	
	public abstract class GameEvent
	{

		public static readonly DateTime EPOCH = new DateTime (1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		public const string DATETIME_FORMATTER = "yyyy-MM-ddTHH:mm:sszzz";

		private const string SEPARATOR = "/";
		private const string ASSIGNMENT = "=";
		private const int MAX_NAME_LEN = 32;
		private const int MAX_KEY_LEN = 64;
		private const int MAX_NOMINAL_VALUE_LEN = 64;
		internal const string EMPTY_STRING = "_EMPTY";

		private const int MAX_FEATURES = 0;

		internal const string INIT_NAME = "INIT";

		internal string name;
		internal long timestamp;
		internal Dictionary<string, string> nominals = new Dictionary<string, string> ();
		internal Dictionary<string, int> integrals = new Dictionary<string, int> ();
		internal Dictionary<string, double> fractionals = new Dictionary<string, double> ();

		public static long NowFromEpoch ()
		{
			long currTime = (long)(DateTime.Now - EPOCH).TotalSeconds;
			return currTime;
		}

		internal static string ToISO8601String (long unixTime)
		{
			return EPOCH.AddSeconds (unixTime).ToString (DATETIME_FORMATTER);
		}

		private static string OnNonEmpty (string str)
		{
			if (string.IsNullOrEmpty (str))
				return EMPTY_STRING;
			else
				return str;
		}

		private static string Left (string input, int length)
		{
			if (length >= input.Length)
				return input;
			else
				return input.Substring (length);
		}

		private static string Sanitize (string str, int len)
		{
			return OnNonEmpty (Left (str.Trim (), len))
				.Replace (SEPARATOR, "_")
				.Replace (ASSIGNMENT, "_");
		}

		internal static string SanitizeName (string name)
		{
			return Sanitize (name, MAX_NAME_LEN);
		}

		internal static string SanitizeKey (string key)
		{
			return Sanitize (key, MAX_KEY_LEN);
		}

		internal static string SanitizeNominalValue (string value)
		{
			return Sanitize (value, MAX_NOMINAL_VALUE_LEN);
		}

		protected GameEvent (string name, long timestamp)
		{
			this.name = SanitizeName (name);
			this.timestamp = timestamp;
		}

		protected GameEvent (string name)
			: this (name, NowFromEpoch ())
		{
		}

		public GameEvent WithTimestamp (long timestamp)
		{
			this.timestamp = timestamp;
			return this;
		}

		internal GameEvent WithPredefinedFeature (string key, string value)
		{
			nominals.Add (key, SanitizeNominalValue (value));
			return this;
		}

		internal GameEvent WithPredefinedFeature (string key, int value)
		{
			integrals.Add (key, value);
			return this;
		}

		internal GameEvent WithPredefinedFeature (string key, double value)
		{
			fractionals.Add (key, value);
			return this;
		}

		private bool CheckFeaturesNumber ()
		{
			int total = nominals.Count + integrals.Count + fractionals.Count;
			return total < MAX_FEATURES;
		}

		public GameEvent WithFeature (string key, string value)
		{
			if (!CheckFeaturesNumber ())
				return this;
			
			nominals.Add (SanitizeKey (key), SanitizeNominalValue (value));
			return this;
		}

		public GameEvent WithFeature (string key, int value)
		{
			if (!CheckFeaturesNumber ())
				return this;

			integrals.Add (SanitizeKey (key), value);
			return this;
		}

		public GameEvent WithFeature (string key, double value)
		{
			if (!CheckFeaturesNumber ())
				return this;

			fractionals.Add (SanitizeKey (key), value);
			return this;
		}

		public void Send ()
		{
			SessionManager.SendEvent (this);
		}

		internal RawEvent ToRaw ()
		{
			RawEvent raw = new RawEvent ();
			raw.name = this.name;
			raw.timestamp = ToISO8601String (this.timestamp);
			raw.nominals = new Dictionary<string, string> (this.nominals);
			raw.integrals = new Dictionary<string, int> (this.integrals);
			raw.fractionals = new Dictionary<string, double> (this.fractionals);

			return raw;
		}

	}

}
