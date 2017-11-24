using System;
using System.Collections.Generic;
using AutofireClient.Iface;

namespace AutofireClient.Util
{
	
	public class ImmPersistenceImpl : IPersistenceProvider
	{

		private static long retentionInSecs = BatchPersistence.ONE_WEEK_IN_SECS;
		private static string version = "";
		private string uuid = "";
		private string batch = "";
		private long batchTs = 0L;
		private int q = 0;

		public bool IsAvailable ()
		{
			return true;
		}

		private void ResetEvt ()
		{
			batch = "";
			batchTs = 0L;
		}

		public void Reset ()
		{
			uuid = "";
			ResetEvt ();
		}

		public string GetAutofireVersion ()
		{
			return version;
		}

		public void SetAutofireVersion (string version)
		{
			ImmPersistenceImpl.version = version;
		}

		public void SetGameId (string gameId)
		{
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

		public int WriteSerialized (string separator,
		                            string beginBatch,
		                            string header,
		                            string tags,
		                            string beginEvents,
		                            IEnumerable<string> gameEvents,
		                            long timestamp,
		                            bool forceBegin = false,
		                            bool forceEnd = false)
		{
			string evts = beginEvents;
			int i = 0;
			foreach (string gameEvent in gameEvents)
				if (!string.IsNullOrEmpty (gameEvent)) {
					evts += gameEvent + separator;
					i++;
				}
			if (i > 0)
				evts = evts.Remove (evts.Length - 1);

			batch = beginBatch +
			header + separator +
			tags + separator +
			evts;
			batchTs = timestamp;
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
		                              string endEvents,
		                              string endBatch,
		                              bool forceAll = false)
		{
			if (string.IsNullOrEmpty (batch))
				return "";

			if (!IsInRetention (timestamp, batchTs))
				ResetEvt ();
			q--;

			return batch + endEvents + endBatch;
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
