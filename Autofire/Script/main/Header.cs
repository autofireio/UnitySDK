using System;
using System.Collections.Generic;
using AutofireClient.Event;
using AutofireClient.Iface;

namespace AutofireClient
{

	[Serializable]
	internal class Header
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

			this.autofireVersion = Version.VERSION;
			if (features.TryGetValue (PLATFORM_KEY, out v)) {
				this.platform = v;
				features.Remove (PLATFORM_KEY);
			} else
				this.platform = env.GetPlatform ();
			if (features.TryGetValue (OS_KEY, out v)) {
				this.os = v;
				features.Remove (OS_KEY);
			} else
				this.os = env.GetOs ();
			if (features.TryGetValue (MODEL_KEY, out v)) {
				this.model = v;
				features.Remove (MODEL_KEY);
			} else
				this.model = env.GetModel ();
			if (features.TryGetValue (LOCALE_KEY, out v)) {
				this.locale = v;
				features.Remove (LOCALE_KEY);
			} else
				this.locale = env.GetLocale ();
			if (features.TryGetValue (VERSION_KEY, out v)) {
				this.version = v;
				features.Remove (VERSION_KEY);
			} else
				this.version = env.GetVersion ();
			this.initTimestamp = initTimestamp;
			this.atLevel = atLevel;

			if (features != null)
				this.features = new Dictionary<string, string> (features);
			else
				this.features = new Dictionary<string, string> ();
		}

		internal Dictionary<string, string> ToRaw ()
		{
			Dictionary<string, string> result = new Dictionary<string, string> (features);
			result.Add ("autofireVersion", autofireVersion);
			result.Add (PLATFORM_KEY, platform);
			result.Add (OS_KEY, os);
			result.Add (MODEL_KEY, model);
			result.Add (LOCALE_KEY, locale);
			result.Add (VERSION_KEY, version);
			result.Add ("initTimestamp", GameEvent.ToISO8601String (initTimestamp));
			if (!string.IsNullOrEmpty (atLevel))
				result.Add ("atLevel", atLevel);

			return result;
		}
	}

}
