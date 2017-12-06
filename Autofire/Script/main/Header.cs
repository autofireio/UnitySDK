using System;
using System.Collections.Generic;
using AutofireClient.Event;
using AutofireClient.Iface;

namespace AutofireClient
{

	[Serializable]
	internal sealed class Header
	{

		public const string PLATFORM_KEY = "platform";
		public const string OS_KEY = "os";
		public const string MODEL_KEY = "model";
		public const string LOCALE_KEY = "locale";
		public const string VERSION_KEY = "version";

		public string autofireVersion;
		public string platform;
		public string os;
		public string model;
		public string locale;
		public string version;
		public long initTimestamp;
		public string atLevel;
		public Dictionary<string, string> features;

		public Header (IEnvironmentProvider env, Dictionary<string, string> features,
		               long initTimestamp, string atLevel)
		{
			string v;
			string vv;

			this.autofireVersion = Version.VERSION;
			if (features.TryGetValue (PLATFORM_KEY, out v)) {
				vv = v;
				features.Remove (PLATFORM_KEY);
			} else
				vv = env.GetPlatform ();
			this.platform = GameEvent.SanitizeNominalValue (vv);
			if (features.TryGetValue (OS_KEY, out v)) {
				vv = v;
				features.Remove (OS_KEY);
			} else
				vv = env.GetOs ();
			this.os = GameEvent.SanitizeNominalValue (vv);
			if (features.TryGetValue (MODEL_KEY, out v)) {
				vv = v;
				features.Remove (MODEL_KEY);
			} else
				vv = env.GetModel ();
			this.model = GameEvent.SanitizeNominalValue (vv);
			if (features.TryGetValue (LOCALE_KEY, out v)) {
				vv = v;
				features.Remove (LOCALE_KEY);
			} else
				vv = env.GetLocale ();
			this.locale = GameEvent.SanitizeNominalValue (vv);
			if (features.TryGetValue (VERSION_KEY, out v)) {
				vv = v;
				features.Remove (VERSION_KEY);
			} else
				vv = env.GetVersion ();
			this.version = GameEvent.SanitizeNominalValue (vv);
			this.initTimestamp = initTimestamp;
			this.atLevel = GameEvent.SanitizeNominalValue (atLevel);

			this.features = new Dictionary<string, string> (features);
		}

		private bool TryAdd (Dictionary<string, string> to, string key, string value)
		{
			if (!string.IsNullOrEmpty (value) && value != GameEvent.EMPTY_STRING) {
				to.Add (key, value);

				return true;
			}

			return false;
		}

		internal Dictionary<string, string> ToRaw ()
		{
			Dictionary<string, string> result = new Dictionary<string, string> (features);
			TryAdd (result, "autofireVersion", autofireVersion);
			TryAdd (result, PLATFORM_KEY, platform);
			TryAdd (result, OS_KEY, os);
			TryAdd (result, MODEL_KEY, model);
			TryAdd (result, LOCALE_KEY, locale);
			TryAdd (result, VERSION_KEY, version);
			TryAdd (result, "initTimestamp", GameEvent.ToISO8601String (initTimestamp));
			TryAdd (result, "atLevel", atLevel);

			return result;
		}
	}

}
