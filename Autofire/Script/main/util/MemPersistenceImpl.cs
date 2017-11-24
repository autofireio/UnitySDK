using AutofireClient.Iface;

namespace AutofireClient.Util
{

	public class MemPersistenceImpl : BatchPersistence
	{

		private static string version = "";
		private string uuid = "";
		private string[] batches = new string[MAX_BATCHES];
		private long[] batchTimestamps = new long[MAX_BATCHES];
		private int readBatch = 0;
		private int writeBatch = 0;
		private int writeBatchEvents = 0;

		public override bool IsAvailable ()
		{
			return true;
		}

		public override string GetAutofireVersion ()
		{
			return version;
		}

		public override void SetAutofireVersion (string version)
		{
			MemPersistenceImpl.version = version;
		}

		public override string ReadUUID ()
		{
			return uuid;
		}

		public override bool WriteUUID (string uuid)
		{
			this.uuid = uuid;

			return true;
		}

		protected override string GetBatch (int key)
		{
			return batches [key];
		}

		protected override long GetBatchTimestamp (int key)
		{
			return batchTimestamps [key];
		}

		protected override void SetBatch (int key, string value)
		{
			batches [key] = value;
		}

		protected override void SetBatchWithTimestamp (int key, string value, long timestamp)
		{
			SetBatch (key, value);
			batchTimestamps [key] = timestamp;
		}

		protected override int GetWriteBatchEvents ()
		{
			return writeBatchEvents;
		}

		protected override void SetWriteBatchEvents (int writeBatchEvents)
		{
			this.writeBatchEvents = writeBatchEvents;
		}

		protected override int GetReadBatch ()
		{
			return readBatch;
		}

		protected override void SetReadBatch (int readBatch)
		{
			this.readBatch = readBatch;
		}

		protected override int GetWriteBatch ()
		{
			return writeBatch;
		}

		protected override void SetWriteBatch (int writeBatch)
		{
			this.writeBatch = writeBatch;
		}

		public override bool PersistToDisk ()
		{
			return true;
		}

	}

}
