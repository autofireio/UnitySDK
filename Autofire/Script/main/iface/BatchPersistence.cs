namespace AutofireClient.Iface
{
	
	public abstract class BatchPersistence: IPersistenceProvider
	{

		public abstract bool IsAvailable ();

		public abstract string ReadUUID ();

		public abstract bool WriteUUID (string guid);

		public abstract bool PersistToDisk ();

		protected abstract string GetBatch (int key);

		protected abstract long GetBatchTimestamp (int key);

		protected abstract void SetBatch (int key, string value);

		protected abstract void SetBatchWithTimestamp (int key, string value, long timestamp);

		protected abstract int GetCurrentBatchEvents ();

		protected abstract void SetCurrentBatchEvents (int currentBatchEvents);

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

		private void ResetCurrentBatchEvents ()
		{
			SetCurrentBatchEvents (0);
		}

		private int IncCurrentBatchEvents (int batchEvents)
		{
			int t = batchEvents;
			if (t >= maxEventsPerBatch - 1)
				return -1;

			t++;
			SetCurrentBatchEvents (t);
			return t;
		}

		private int NextBatch (int batch)
		{
			return (batch + 1) % MAX_BATCHES;
		}

		private int BatchesDiff (int write, int read)
		{
			if (write >= read)
				return write - read;

			return (MAX_BATCHES - read) + (write - 1);
		}

		private void ResetReadBatch ()
		{
			SetReadBatch (0);
		}

		private int NextReadBatch (int currentReadBatch, int currentWriteBatch)
		{
			int rd = NextBatch (currentReadBatch);

			if (currentReadBatch == currentWriteBatch)
				return -1;

			SetReadBatch (rd);
			return rd;
		}

		private void ResetWriteBatch ()
		{
			SetWriteBatch (0);
		}

		private int NextWriteBatch (int currentWriteBatch, int currentReadBatch)
		{
			int wr = NextBatch (currentWriteBatch);

			if (wr == currentReadBatch)
				return -2;
			else if (BatchesDiff (wr, currentReadBatch) > maxBatches)
				return -1;

			SetWriteBatch (wr);
			return wr;
		}

		private bool CheckRetention (long now, long timestamp)
		{
			return (now - timestamp) <= retentionInSecs;
		}

		private void AppendEvent (int currentBatch,
		                          int eventsAlreadyInBatch,
		                          string header,
		                          string tags,
		                          string gameEvent,
		                          long timestamp)
		{
			string batchValue = GetBatch (currentBatch);
			if (eventsAlreadyInBatch == 0)
				batchValue = "{" +
				"\"header\":" + header + "," +
				"\"tags\":" + tags + "," +
				"\"events\":[";
			else
				batchValue += ",";
			batchValue += gameEvent;
			SetBatchWithTimestamp (currentBatch, batchValue, timestamp);
		}

		private string SealBatch (int currentBatch)
		{
			string batchValue = GetBatch (currentBatch);
			if (string.IsNullOrEmpty (batchValue))
				return "";

			if (!batchValue.EndsWith ("]}"))
				batchValue += "]}";
			SetBatch (currentBatch, batchValue);
			return batchValue;
		}

		public int WriteSerialized (long timestamp,
		                            string gameEvent,
		                            string header,
		                            string tags,
		                            bool forceBegin = false,
		                            bool forceEnd = false)
		{
			try {
				int currWr = GetWriteBatch ();
				int currRd = GetReadBatch ();
				int currBatchEvents = GetCurrentBatchEvents ();

				int currWr2 = currWr;
				if (forceBegin && currBatchEvents > 0) {
					currWr2 = NextWriteBatch (currWr, currRd);
					if (currWr2 >= 0) {
						ResetCurrentBatchEvents ();
					} else {
						string rr = ReadSerialized (timestamp);
						if (string.IsNullOrEmpty (rr))
							return 0;

						CommitReadSerialized ();
					}
					currWr = GetWriteBatch ();
					currRd = GetReadBatch ();
					currBatchEvents = GetCurrentBatchEvents ();
				}

				AppendEvent (currWr2, currBatchEvents, header, tags, gameEvent, timestamp);
				currBatchEvents++;
				int incBatchEvents = IncCurrentBatchEvents (currBatchEvents);
				if ((forceEnd && incBatchEvents >= 0) || incBatchEvents < 0) {
					int nextWr = NextWriteBatch (currWr2, currRd);
					if (nextWr >= 0) {
						ResetCurrentBatchEvents ();
						return 1;
					} else {
						string r = ReadSerialized (timestamp);
						if (string.IsNullOrEmpty (r))
							return 0;

						CommitReadSerialized ();
						nextWr = NextWriteBatch (currWr2, currRd);
						if (nextWr >= 0)
							return 1;
						else
							return 0;
					}
				} else {
					// incBatchEvents >= 0
					return 2;
				}
			} catch {
				ResetBatches ();

				return 0;
			}
		}

		private string FilterRetention (long timestamp,
		                                bool forceAll,
		                                int currentReadBatch,
		                                int currentWriteBatch)
		{
			long rdTs = GetBatchTimestamp (currentReadBatch);
			if (!CheckRetention (timestamp, rdTs)) {
				currentReadBatch = NextReadBatch (currentReadBatch, currentWriteBatch);

				return ReadSerialized (timestamp, forceAll);
			}

			return SealBatch (currentReadBatch);
		}

		public string ReadSerialized (long timestamp,
		                              bool forceAll = false)
		{
			int currRd = GetReadBatch ();
			int currWr = GetWriteBatch ();

			int diff = BatchesDiff (currWr, currRd);
			if (diff == 0) {
				if (forceAll) {
					int currBatchEvents = GetCurrentBatchEvents ();
					if (currBatchEvents == 0)
						return "";
					else {
						string res = SealBatch (currRd);
						return res;
					}
				} else
					return "";
			} else if (diff > 0) {
				string res = FilterRetention (timestamp, forceAll, currRd, currWr);
				return res;
			}

			return "";
		}

		public bool CommitReadSerialized ()
		{
			int currRd = GetReadBatch ();
			int currWr = GetWriteBatch ();

			int nextRd = NextReadBatch (currRd, currWr);
			bool nonEmpty = nextRd >= 0;
			if (!nonEmpty)
				ResetCurrentBatchEvents ();
			return nonEmpty;
		}

		private void ResetBatches ()
		{
			ResetCurrentBatchEvents ();
			ResetReadBatch ();
			ResetWriteBatch ();
		}

		public void Reset ()
		{
			WriteUUID ("");
			ResetBatches ();
		}

	}

}
