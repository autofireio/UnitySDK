using AutofireClient.Iface;

namespace AutofireClient.Util
{
	
	public class ImmPersistenceImpl : IPersistenceProvider
	{

		private static string uuid = "nobody";
		private static string evt = "";
		private static long evtTs = 0L;
		private static long retentionInSecs = BatchPersistence.ONE_WEEK_IN_SECS;

		public void SetRetention (long retentionInSecs)
		{
			if (retentionInSecs > 0L)
				ImmPersistenceImpl.retentionInSecs = retentionInSecs;
			else
				ImmPersistenceImpl.retentionInSecs = BatchPersistence.ONE_WEEK_IN_SECS;
		}

		public bool IsAvailable ()
		{
			return true;
		}

		public void SetGameId (string gameId)
		{
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

		public string ReadUUID ()
		{
			return uuid;
		}

		public bool WriteUUID (string guid)
		{
			uuid = guid;
			return true;
		}

		public int WriteSerialized (long timestamp,
		                            string gameEvent,
		                            string header,
		                            string tags,
		                            bool forceBegin = false,
		                            bool forceEnd = false)
		{
			evt = "{" +
			"\"header\":" + header + "," +
			"\"tags\":" + tags + "," +
			"\"events\":[" + gameEvent + "]" +
			"}";

			return 1;
		}

		private bool CheckRetention (long now, long timestamp)
		{
			return (now - timestamp) <= retentionInSecs;
		}

		public string ReadSerialized (long timestamp,
		                              bool forceAll = false)
		{
			if (!CheckRetention (timestamp, evtTs))
				ResetEvt ();

			return evt;
		}

		public bool CommitReadSerialized ()
		{
			ResetEvt ();
			return true;
		}

		public bool PersistToDisk ()
		{
			return true;
		}

	}

}
