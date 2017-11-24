namespace AutofireClient.Iface
{
	
	public abstract class KVBatchPersistence : BatchPersistence
	{

		protected const string DEFAULT_STRING = "";
		protected const int DEFAULT_INT = 0;

		protected abstract string GetString (string key, bool isAbsolute);

		protected abstract void SetString (string key, string value, bool isAbsolute);

		protected abstract int GetInt (string key, bool isAbsolute);

		protected abstract void SetInt (string key, int value, bool isAbsolute);

		protected string GetBatchKey (int key)
		{
			return "batch" + key;
		}

		protected string GetBatchTimestampKey (int key)
		{
			return "batch" + key + ".ts";
		}

		public override string GetAutofireVersion ()
		{
			return GetString ("ver", false);
		}

		public override void SetAutofireVersion (string version)
		{
			SetString ("ver", version, false);
		}

		public override string ReadUUID ()
		{
			return GetString ("uuid", true);
		}

		public override bool WriteUUID (string uuid)
		{
			bool ret = true;

			try {
				SetString ("uuid", uuid, true);
			} catch {
				ret = false;
			}

			return ret;
		}

		protected override string GetBatch (int key)
		{
			return GetString (GetBatchKey (key), false);
		}

		protected override long GetBatchTimestamp (int key)
		{
			return long.Parse (GetString (GetBatchTimestampKey (key), false));
		}

		protected override void SetBatch (int key, string value)
		{
			SetString (GetBatchKey (key), value, false);
		}

		protected override void SetBatchWithTimestamp (int key, string value, long timestamp)
		{
			SetBatch (key, value);
			SetString (GetBatchTimestampKey (key), timestamp.ToString (), false);
		}

		protected override int GetWriteBatchEvents ()
		{
			return GetInt ("writeBatchEvents", false);
		}

		protected override void SetWriteBatchEvents (int writeBatchEvents)
		{
			SetInt ("writeBatchEvents", writeBatchEvents, false);
		}

		protected override int GetReadBatch ()
		{
			return GetInt ("readBatch", false);
		}

		protected override void SetReadBatch (int readBatch)
		{
			SetInt ("readBatch", readBatch, false);
		}

		protected override int GetWriteBatch ()
		{
			return GetInt ("writeBatch", false);
		}

		protected override void SetWriteBatch (int writeBatch)
		{
			SetInt ("writeBatch", writeBatch, false);
		}

	}

}
