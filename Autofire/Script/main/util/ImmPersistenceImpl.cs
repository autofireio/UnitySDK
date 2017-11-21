using System;
using System.Collections.Generic;
using AutofireClient.Iface;

namespace AutofireClient.Util
{
	
	public class ImmPersistenceImpl : IPersistenceProvider
	{

		private static string version = "";
		private static string uuid = "nobody";
		private static string evt = "";
		private static long evtTs = 0L;
		private static int q = 0;
		private static long retentionInSecs = BatchPersistence.ONE_WEEK_IN_SECS;

		public bool IsAvailable ()
		{
			return true;
		}

		private void ResetEvt ()
		{
			evt = "";
			evtTs = 0L;
		}

		public void Reset ()
		{
			uuid = "";
			ResetEvt ();
		}

		public void SetGameId (string gameId)
		{
		}

		public string GetAutofireVersion ()
		{
			return version;
		}

		public void SetAutofireVersion (string version)
		{
			ImmPersistenceImpl.version = version;
		}

		public string ReadUUID ()
		{
			return uuid;
		}

		public bool WriteUUID (string guid)
		{
			uuid = guid;
			return true;
		}

		public void SetRetention (long retentionInSecs)
		{
			if (retentionInSecs > 0L)
				ImmPersistenceImpl.retentionInSecs = retentionInSecs;
			else
				ImmPersistenceImpl.retentionInSecs = BatchPersistence.ONE_WEEK_IN_SECS;
		}

		public int WriteSerialized (IEnumerable<string> gameEvents,
		                            long timestamp,
		                            string header,
		                            string tags,
		                            bool forceBegin = false,
		                            bool forceEnd = false)
		{
			string evts = "";
			foreach (string gameEvent in gameEvents)
				evts += gameEvent + ",";
			evts = evts.Remove (evts.Length - 1);

			evt = "{" +
			"\"header\":" + header + "," +
			"\"tags\":" + tags + "," +
			"\"events\":[" + evts + "]" +
			"}";
			evtTs = timestamp;
			q++;
			if (q < 0)
				q = 1;

			return 1;
		}

		private bool IsInRetention (long now, long timestamp)
		{
			return (now - timestamp) <= retentionInSecs;
		}

		public string ReadSerialized (long timestamp,
		                              bool forceAll = false)
		{
			if (!IsInRetention (timestamp, evtTs))
				ResetEvt ();
			q--;

			return evt;
		}

		public bool CommitReadSerialized ()
		{
			if (q > 0)
				return true;
			
			ResetEvt ();
			return true;
		}

		public bool PersistToDisk ()
		{
			return true;
		}

	}

}
