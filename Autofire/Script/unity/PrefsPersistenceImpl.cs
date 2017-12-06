using UnityEngine;
using AutofireClient.Iface;

namespace AutofireClient.Unity
{

	public class PrefsPersistenceImpl : KVBatchPersistence
	{

		internal const string PREFIX = "io.autofire.";

		private static string GetName (string key)
		{
			return PREFIX + key;
		}

		protected override string GetString (string key, bool isAbsolute)
		{
			return PlayerPrefs.GetString (GetName (key), DEFAULT_STRING);
		}

		protected override void SetString (string key, string value, bool isAbsolute)
		{
			PlayerPrefs.SetString (GetName (key), value);
		}

		protected override int GetInt (string key, bool isAbsolute)
		{
			return PlayerPrefs.GetInt (GetName (key), DEFAULT_INT);
		}

		protected override void SetInt (string key, int value, bool isAbsolute)
		{
			PlayerPrefs.SetInt (GetName (key), value);
		}

		public override bool IsAvailable ()
		{
			return true;
		}

		public override bool PersistToDisk ()
		{
			bool ret = true;
			 
			try {
				PlayerPrefs.Save ();
			} catch {
				ret = false;
			}
			 
			return ret;
		}

		private string ReadUUIDHelper ()
		{
			return GetString ("uuid", true);
		}

		public override string ReadUUID ()
		{
			StoragePersistenceImpl storage = new StoragePersistenceImpl ();
			if (storage.IsAvailable ()) {
				string uuid = storage.ReadUUID ();
				if (string.IsNullOrEmpty (uuid))
					return ReadUUIDHelper ();
				else
					return storage.ReadUUID ();
			}

			return ReadUUIDHelper ();
		}

		private bool WriteUUIDHelper (string uuid)
		{
			bool ret = true;

			try {
				SetString ("uuid", uuid, true);
			} catch {
				ret = false;
			}

			return ret;
		}

		public override bool WriteUUID (string uuid)
		{
			StoragePersistenceImpl storage = new StoragePersistenceImpl ();
			if (storage.IsAvailable ())
				return storage.WriteUUID (uuid) ||
				WriteUUIDHelper (uuid);
			else
				return WriteUUIDHelper (uuid);
		}

	}

}
