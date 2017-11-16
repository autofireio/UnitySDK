﻿using AutofireClient.Iface;

namespace AutofireClient.Util
{

	public class MemPersistenceImpl : BatchPersistence
	{

		private static string version = "";
		private static string uuid = "nobody";
		private static string[] batches = new string[MAX_BATCHES];
		private static long[] batchTimestamps = new long[MAX_BATCHES];
		private static int readBatch = 0;
		private static int writeBatch = 0;
		private static int writeBatchEvents = 0;

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
			MemPersistenceImpl.uuid = uuid;

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
			MemPersistenceImpl.writeBatchEvents = writeBatchEvents;
		}

		protected override int GetReadBatch ()
		{
			return readBatch;
		}

		protected override void SetReadBatch (int readBatch)
		{
			MemPersistenceImpl.readBatch = readBatch;
		}

		protected override int GetWriteBatch ()
		{
			return writeBatch;
		}

		protected override void SetWriteBatch (int writeBatch)
		{
			MemPersistenceImpl.writeBatch = writeBatch;
		}

		public override bool PersistToDisk ()
		{
			return true;
		}

	}

}
