using System;
using System.IO;

namespace AutofireClient.Iface
{
	
	public abstract class FileBatchPersistence : KVBatchPersistence
	{

		protected abstract string RootDirectory ();

		protected string ChRootDirectory ()
		{
			return Path.Combine (RootDirectory (), ".autofire");
		}

		protected string GameDirectory ()
		{
			return Path.Combine (ChRootDirectory (), BatchPersistence.gameId);
		}

		protected string GetFileName (string fname, bool isAbsolute)
		{
			return isAbsolute ? 
				Path.Combine (ChRootDirectory (), fname) :
				Path.Combine (GameDirectory (), fname);
		}

		protected void CheckDirectory (bool isAbsolute)
		{
			if (!System.IO.Directory.Exists (ChRootDirectory ()))
				System.IO.Directory.CreateDirectory (ChRootDirectory ());
			if (!isAbsolute && !System.IO.Directory.Exists (GameDirectory ()))
				System.IO.Directory.CreateDirectory (GameDirectory ());
		}

		protected override string GetString (string key, bool isAbsolute)
		{
			try {
				string fname = GetFileName (key, isAbsolute);

				if (!File.Exists (fname))
					return DEFAULT_STRING;
				return File.ReadAllText (fname);
			} catch {
				return DEFAULT_STRING;
			}
		}

		protected override void SetString (string key, string value, bool isAbsolute)
		{
			try {
				string fname = GetFileName (key, isAbsolute);

				CheckDirectory (isAbsolute);
				File.WriteAllText (fname, value);
			} catch {
			}
		}

		protected override int GetInt (string key, bool isAbsolute)
		{
			try {
				string fname = GetFileName (key, isAbsolute);

				if (!File.Exists (fname))
					return DEFAULT_INT;
				return int.Parse (File.ReadAllText (fname));
			} catch {
				return DEFAULT_INT;
			}
		}

		protected override void SetInt (string key, int value, bool isAbsolute)
		{
			try {
				string fname = GetFileName (key, isAbsolute);

				CheckDirectory (isAbsolute);
				File.WriteAllText (fname, value.ToString ());
			} catch {
			}
		}

		public override bool IsAvailable ()
		{
			try {
				CheckDirectory (false);
				string file = Path.Combine (GameDirectory (), Path.GetRandomFileName ());
				using (FileStream fs = File.Create (file, 1)) {
				}
				File.Delete (file);

				return true;
			} catch {
				return false;
			}
		}

		public override bool PersistToDisk ()
		{
			return true;
		}

	}

}
