using System.Collections.Generic;

namespace AutofireClient.Iface
{

	public interface IPersistenceProvider
	{

		bool IsAvailable ();

		void Reset ();

		void SetGameId (string gameId);

		string GetAutofireVersion ();

		void SetAutofireVersion (string version);

		string ReadUUID ();

		bool WriteUUID (string guid);

		void SetRetention (long retentionInSecs);

		int WriteSerialized (IEnumerable<string> gameEvents,
		                     long timestamp,
		                     string header,
		                     string tags,
		                     bool forceBegin = false,
		                     bool forceEnd = false);

		string ReadSerialized (long timestamp,
		                       bool forceAll = false);

		bool CommitReadSerialized ();

		bool PersistToDisk ();

	}

}
