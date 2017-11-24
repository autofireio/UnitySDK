using System.Collections.Generic;

namespace AutofireClient.Iface
{
	
	public abstract class BatchPersistence: IPersistenceProvider
	{

		public abstract bool IsAvailable ();

		public abstract string GetAutofireVersion ();

		public abstract void SetAutofireVersion (string version);

		public abstract string ReadUUID ();

		public abstract bool WriteUUID (string guid);

		public abstract bool PersistToDisk ();

		protected abstract string GetBatch (int key);

		protected abstract long GetBatchTimestamp (int key);

		protected abstract void SetBatch (int key, string value);

		protected abstract void SetBatchWithTimestamp (int key, string value, long timestamp);

		protected abstract int GetWriteBatchEvents ();

		protected abstract void SetWriteBatchEvents (int writeBatchEvents);

		protected abstract int GetReadBatch ();

		protected abstract void SetReadBatch (int readBatch);

		protected abstract int GetWriteBatch ();

		protected abstract void SetWriteBatch (int writeBatch);

		public const long ONE_WEEK_IN_SECS = 7L * 24L * 3600L;
		protected const int MAX_BATCHES = 20;

		protected static string gameId = "Unknown";
		protected static int maxEventsPerBatch = 32;
		protected static int maxBatches = 16;
		protected static long retentionInSecs = ONE_WEEK_IN_SECS;

		private int previousReadBatch = -1;

		public void SetGameId (string gameId)
		{
			BatchPersistence.gameId = gameId;
		}

		public void SetRetention (long retentionInSecs)
		{
			if (retentionInSecs > 0L)
				BatchPersistence.retentionInSecs = retentionInSecs;
			else
				BatchPersistence.retentionInSecs = ONE_WEEK_IN_SECS;
		}

		public static void SetMaxEventsPerBatch (int maxEvents)
		{
			if (maxEvents >= 1)
				BatchPersistence.maxEventsPerBatch = maxEvents;
			else
				BatchPersistence.maxEventsPerBatch = 1;
		}

		public static void SetMaxBatches (int maxBatches)
		{
			if (maxBatches >= 2 && maxBatches <= MAX_BATCHES)
				BatchPersistence.maxBatches = maxBatches;
			else if (maxBatches > MAX_BATCHES)
				BatchPersistence.maxBatches = MAX_BATCHES;
			else
				BatchPersistence.maxBatches = 2;
		}

		private void ResetWriteBatchEvents ()
		{
			SetWriteBatchEvents (0);
		}

		private void ResetReadBatch ()
		{
			SetReadBatch (0);
		}

		private void ResetWriteBatch ()
		{
			SetWriteBatch (0);
		}

		private void ResetBatches ()
		{
			ResetWriteBatchEvents ();
			ResetReadBatch ();
			ResetWriteBatch ();
		}

		public void Reset ()
		{
			WriteUUID ("");
			ResetBatches ();
		}

		private bool IsBatchEmpty (int batchEvents)
		{
			return batchEvents == 0;
		}

		private bool IsBatchFull (int batchEvents)
		{
			return batchEvents >= maxEventsPerBatch;
		}

		private int NextBatch (int batch)
		{
			return (batch + 1) % maxBatches;
		}

		private int Diff (int writeBatch, int readBatch)
		{
			if (writeBatch >= readBatch)
				return writeBatch - readBatch;

			return (maxBatches - readBatch) + (writeBatch - 1);
		}

		private bool IsEmpty (int writeBatch, int readBatch)
		{
			return writeBatch == readBatch;
		}

		private bool IsEmpty (int writeBatch, int readBatch, int writeBatchEvents)
		{
			return IsBatchEmpty (writeBatchEvents) && IsEmpty (writeBatch, readBatch);
		}

		private bool IsFull (int writeBatch, int readBatch)
		{
			return NextBatch (writeBatch) == readBatch;
		}

		private bool IsFull (int writeBatch, int readBatch, int writeBatchEvents)
		{
			return IsBatchFull (writeBatchEvents) && IsFull (writeBatch, readBatch);
		}

		private bool IsInRetention (long now, long timestamp)
		{
			return (now - timestamp) <= retentionInSecs;
		}

		private int FilterRetention (long timestamp,
		                             int writeBatch,
		                             ref int readBatch,
		                             ref int writeBatchEvents)
		{
			int i = 0;
			bool done = false;
			long rdTs;
			while (!done)
				if (IsEmpty (writeBatch, readBatch, writeBatchEvents))
					done = true;
				else {
					rdTs = GetBatchTimestamp (readBatch);
					if (IsInRetention (timestamp, rdTs))
						done = true;
					else if (IsEmpty (writeBatch, readBatch)) {
						ResetWriteBatchEvents ();
						writeBatchEvents = 0;
						i++;
						done = true;
					} else {
						readBatch = NextBatch (readBatch);
						i++;
					}
				}
			if (i > 0)
				SetReadBatch (readBatch);

			return i;
		}

		private string BatchOf (int writeBatch,
		                        int writeBatchEvents,
		                        string separator,
		                        string beginBatch,
		                        string header,
		                        string tags,
		                        string beginEvents)
		{
			string batchValue;
			if (writeBatchEvents == 0)
				batchValue = beginBatch +
				header + separator +
				tags + separator +
				beginEvents;
			else
				batchValue = GetBatch (writeBatch);

			return batchValue;
		}

		private void AppendEvent (ref string batchValue,
		                          ref int writeBatchEvents,
		                          string gameEvent,
		                          string separator)
		{
			if (!string.IsNullOrEmpty (gameEvent)) {
				if (writeBatchEvents > 0)
					batchValue += separator;
				batchValue += gameEvent;
				writeBatchEvents++;
			}
		}

		private string SealBatch (int readBatch,
		                          string endEvents,
		                          string endBatch)
		{
			string last = endEvents + endBatch;
			string batchValue = GetBatch (readBatch);
			if (string.IsNullOrEmpty (batchValue))
				return "";

			if (!batchValue.EndsWith (last)) {
				batchValue += last;
				SetBatch (readBatch, batchValue);
			}

			return batchValue;
		}

		private void IncWriteBatch (long timestamp,
		                            ref int writeBatch,
		                            ref int readBatch,
		                            ref int writeBatchEvents)
		{
			if (IsFull (writeBatch, readBatch)) {
				int removed =
					FilterRetention (timestamp, writeBatch, ref readBatch, ref writeBatchEvents);
				if (removed == 0)
					readBatch = NextBatch (readBatch);
			}
			writeBatch = NextBatch (writeBatch);
			writeBatchEvents = 0;
		}

		private void SetBatches (int previousWriteBatch,
		                         int currentWriteBatch,
		                         int previousReadBatch,
		                         int currentReadBatch,
		                         int previousWriteBatchEvents,
		                         int currentWriteBatchEvents)
		{
			if (previousWriteBatch != currentWriteBatch)
				SetWriteBatch (currentWriteBatch);
			if (previousReadBatch != currentReadBatch)
				SetReadBatch (currentReadBatch);
			if (previousWriteBatchEvents != currentWriteBatchEvents)
				SetWriteBatchEvents (currentWriteBatchEvents);
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
			try {
				int wr0 = GetWriteBatch ();
				int wr = wr0;
				int rd0 = GetReadBatch ();
				int rd = rd0;
				int wrEvents0 = GetWriteBatchEvents ();
				int wrEvents = wrEvents0;
				int result = 0;

				if (forceBegin && IsBatchEmpty (wrEvents))
					forceBegin = false;
				if (forceBegin) {
					IncWriteBatch (timestamp, ref wr, ref rd, ref wrEvents);
					result = 1;
				}

				string batchValue = BatchOf (wr, wrEvents, separator, beginBatch, header, tags, beginEvents);
				foreach (string gameEvent in gameEvents) {
					AppendEvent (ref batchValue, ref wrEvents, gameEvent, separator);

					if (IsBatchFull (wrEvents))
						forceEnd = true;
					if (forceEnd) {
						SetBatchWithTimestamp (wr, batchValue, timestamp);
						IncWriteBatch (timestamp, ref wr, ref rd, ref wrEvents);
						batchValue = BatchOf (wr, wrEvents, separator, beginBatch, header, tags, beginEvents);
						forceEnd = false;
						result = 1;
					}
				}
				if (wrEvents > 0)
					SetBatchWithTimestamp (wr, batchValue, timestamp);

				SetBatches (wr0, wr, rd0, rd, wrEvents0, wrEvents);

				return result;
			} catch {
				ResetBatches ();

				return -1;
			}
		}

		public string ReadSerialized (long timestamp,
		                              string endEvents,
		                              string endBatch,
		                              bool forceAll = false)
		{
			try {
				int wr0 = GetWriteBatch ();
				int wr = wr0;
				int rd0 = GetReadBatch ();
				int rd = rd0;
				int wrEvents0 = GetWriteBatchEvents ();
				int wrEvents = wrEvents0;
				string result = "";

				FilterRetention (timestamp, wr, ref rd, ref wrEvents);

				if (IsEmpty (wr, rd) && (!forceAll || IsBatchEmpty (wrEvents)))
					return "";

				result = SealBatch (rd, endEvents, endBatch);
				previousReadBatch = rd;

				if (IsEmpty (wr, rd))
					IncWriteBatch (timestamp, ref wr, ref rd, ref wrEvents);

				SetBatches (wr0, wr, rd0, rd, wrEvents0, wrEvents);

				return result;
			} catch {
				ResetBatches ();

				return "";
			}
		}

		public bool CommitReadSerialized ()
		{
			try {
				int wr0 = GetWriteBatch ();
				int wr = wr0;
				int rd0 = GetReadBatch ();
				int rd = rd0;

				if (rd != previousReadBatch)
					return true;

				if (IsEmpty (wr, rd))
					ResetWriteBatchEvents ();
				else
					rd = NextBatch (rd);

				SetBatches (wr0, wr, rd0, rd, -1, -1);

				return true;
			} catch {
				ResetBatches ();

				return false;
			}
		}

	}

}
