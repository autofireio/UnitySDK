using UnityEngine;
using AutofireClient.Iface;

namespace AutofireClient.Unity
{

	public class PersistenceImpl : BatchPersistence
	{

		private const string DEFAULT_STRING = "";
		private const int DEFAULT_INT = 0;

		internal const string PREFIX = "io.autofire.";

		private string GetBatchKey (int key)
		{
			return "batch" + key;
		}

		private string GetBatchTimestampKey (int key)
		{
			return "batch" + key + ".ts";
		}

		private static string GetName (string key)
		{
			return PREFIX + key;
		}

		private static string GetString (string key)
		{
			return PlayerPrefs.GetString (GetName (key), DEFAULT_STRING);
		}

		private static void SetString (string key, string value)
		{
			PlayerPrefs.SetString (GetName (key), value);
		}

		private static int GetInt (string key)
		{
			return PlayerPrefs.GetInt (GetName (key), DEFAULT_INT);
		}

		private static void SetInt (string key, int value)
		{
			PlayerPrefs.SetInt (GetName (key), value);
		}

		public override bool IsAvailable ()
		{
			return true;
		}

		public override string GetAutofireVersion ()
		{
			return GetString ("ver");
		}

		public override void SetAutofireVersion (string version)
		{
			SetString ("ver", version);
		}

		public override string ReadUUID ()
		{
			return GetString ("uuid");
		}

		public override bool WriteUUID (string uuid)
		{
			bool ret = true;

			try {
				SetString ("uuid", uuid);
			} catch {
				ret = false;
			}

			return ret;
		}

		protected override string GetBatch (int key)
		{
			return GetString (GetBatchKey (key));
		}

		protected override long GetBatchTimestamp (int key)
		{
			return long.Parse (GetString (GetBatchTimestampKey (key)));
		}

		protected override void SetBatch (int key, string value)
		{
			SetString (GetBatchKey (key), value);
		}

		protected override void SetBatchWithTimestamp (int key, string value, long timestamp)
		{
			SetBatch (key, value);
			SetString (GetBatchTimestampKey (key), timestamp.ToString ());
		}

		protected override int GetWriteBatchEvents ()
		{
			return GetInt ("writeBatchEvents");
		}

		protected override void SetWriteBatchEvents (int writeBatchEvents)
		{
			SetInt ("writeBatchEvents", writeBatchEvents);
		}

		protected override int GetReadBatch ()
		{
			return GetInt ("readBatch");
		}

		protected override void SetReadBatch (int readBatch)
		{
			SetInt ("readBatch", readBatch);
		}

		protected override int GetWriteBatch ()
		{
			return GetInt ("writeBatch");
		}

		protected override void SetWriteBatch (int writeBatch)
		{
			SetInt ("writeBatch", writeBatch);
		}

		public override bool PersistToDisk ()
		{
			// NOTE: Unity will persist PlayerPrefs on application quit
			/*
			 *bool ret = true;
			 *
			 *try {
			 *	PlayerPrefs.Save ();
			 *} catch {
			 *	ret = false;
			 *}
			 *
			 *return ret;
			 */
			return true;
		}

	}

}
