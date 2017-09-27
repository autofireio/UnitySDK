using System;
using System.Collections.Generic;

namespace AutofireClient.Event
{

	[Serializable]
	public class RawEvent
	{

		public string name;
		public string timestamp;
		public Dictionary<string, string> nominals;
		public Dictionary<string, int> integrals;
		public Dictionary<string, double> fractionals;

	}

}
