using System;
using System.Collections.Generic;
using System.Text;

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

		public override string ToString ()
		{
			int i;
			StringBuilder sb = new StringBuilder ("(Event::" + name + " " + timestamp + " ");
			sb.Append ("(Nominals (");
			i = 0;
			foreach (KeyValuePair<string, string> entry in nominals) {
				string k = entry.Key;
				string v = entry.Value;
				if (i > 0)
					sb.Append (" ");
				sb.Append ("(").Append (k).Append (" ").Append (v).Append (")");
				i++;
			}
			sb.Append (")) ");
			sb.Append ("(Integrals (");
			i = 0;
			foreach (KeyValuePair<string, int> entry in integrals) {
				string k = entry.Key;
				int v = entry.Value;
				if (i > 0)
					sb.Append (" ");
				sb.Append ("(").Append (k).Append (" ").Append (v).Append (")");
				i++;
			}
			sb.Append (")) ");
			sb.Append ("(Fractionals (");
			i = 0;
			foreach (KeyValuePair<string, double> entry in fractionals) {
				string k = entry.Key;
				double v = entry.Value;
				if (i > 0)
					sb.Append (" ");
				sb.Append ("(").Append (k).Append (" ").Append (v).Append (")");
				i++;
			}
			sb.Append (")))");

			return sb.ToString ();
		}

	}

}
