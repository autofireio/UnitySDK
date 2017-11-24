using System.Collections.Generic;

namespace AutofireClient.Iface
{

	public interface IPersistenceProvider
	{

		bool IsAvailable ();

		void Reset ();

		string GetAutofireVersion ();

		void SetAutofireVersion (string version);

		void SetGameId (string gameId);

		string ReadUUID ();

		bool WriteUUID (string guid);

		void SetRetention (long retentionInSecs);

		int WriteSerialized (string separator,
		                     string beginBatch,
		                     string header,
		                     string tags,
		                     string beginEvents,
		                     IEnumerable<string> gameEvents,
		                     long timestamp,
		                     bool forceBegin = false,
		                     bool forceEnd = false);

		string ReadSerialized (long timestamp,
		                       string endEvents,
		                       string endBatch,
		                       bool forceAll = false);

		bool CommitReadSerialized ();

		bool PersistToDisk ();

	}

}
