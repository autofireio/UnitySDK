using System.Collections.Generic;
using AutofireClient.Event;
using AutofireClient.Iface;

namespace AutofireClient
{

	public class SessionManager
	{
		
		private const int MAX_SEND_INTERVAL_SEC = 120;
		private const int HTTP_REQUEST_TIMEOUT_SEC = 10;

		private static object manageLock = new object ();

		private static ILoggerProvider logger;
		private static IEnvironmentProvider environment;
		private static IPersistenceProvider persistence;
		private static IGUIDProvider guid;
		private static IJSONProvider json;
		private static IHTTPProvider http;

		private static bool checkExit = false;
		private static bool isInitialized = false;
		private static bool isFlushing = false;
		private static bool canExit = true;
		private static bool httpSending = false;
		// NOTE: set to 0 in order to send the INIT asap (bypass batching)
		private static long httpLastAttempt = GameEvent.NowFromEpoch ();
		private static Header currentHeader;
		private static string currentSerializedHeader;
		private static List<string> currentTags;
		private static string currentSerializedTags;

		private static int sendIntervalSec = MAX_SEND_INTERVAL_SEC;
		private static string url = "https://service.autofire.io/api/v1/command/clients/datapoints";
		private static Dictionary<string, string> requestHeaders;

		public static void HandleHTTPResponse (HelperHTTPResponse response)
		{
			bool hasNext = true;
			bool willFlush = false;
			long now = GameEvent.NowFromEpoch ();

			lock (manageLock) {
				logger.LogDebug ("Autofire HTTP response", response.ToString ());
				willFlush = isFlushing;
				httpSending = false;
				if (response.IsDiscardable ())
					hasNext = persistence.CommitReadSerialized ();
				else
					hasNext = false;
				canExit = !hasNext;
			}

			if (hasNext)
				SendBatch (now, willFlush);
		}

		public static void SetProviders (ILoggerProvider loggerProvider,
		                                 IEnvironmentProvider environmentProvider,
		                                 IPersistenceProvider persistenceProvider,
		                                 IGUIDProvider guidProvider,
		                                 IJSONProvider jsonProvider,
		                                 IHTTPProvider httpProvider)
		{
			lock (manageLock) {
				logger = loggerProvider;
				environment = environmentProvider;
				persistence = persistenceProvider;
				guid = guidProvider;
				json = jsonProvider;
				http = httpProvider;
				http.SetRequestTimeout (HTTP_REQUEST_TIMEOUT_SEC);
			}
		}

		public static void ToggleCheckExit ()
		{
			lock (manageLock) {
				checkExit = !checkExit;
			}
		}

		public static void SetRetention (long retentionInSecs)
		{
			lock (manageLock) {
				persistence.SetRetention (retentionInSecs);
			}
		}

		public static void setSendInterval (int intervalSecs)
		{
			lock (manageLock) {
				if (intervalSecs <= MAX_SEND_INTERVAL_SEC)
					SessionManager.sendIntervalSec = intervalSecs;
				else
					SessionManager.sendIntervalSec = MAX_SEND_INTERVAL_SEC;
			}
		}

		public static void setURL (string url)
		{
			lock (manageLock) {
				if (!string.IsNullOrEmpty (url))
					SessionManager.url = url;
			}
		}

		private static void InitializeParameters (string gameId, string playerId,
		                                          long now, string atLevel, 
		                                          Dictionary<string, string> headerFeatures, List<string> tags)
		{
			currentHeader = new Header (environment, headerFeatures, now, atLevel);
			currentSerializedHeader = json.JsonifyHeader (currentHeader.ToRaw ());
			currentTags = new List<string> ();
			if (tags != null)
				currentTags = new List<string> (tags);
			currentSerializedTags = json.JsonifyTags (currentTags);

			requestHeaders = new Dictionary<string, string> ();
			requestHeaders.Add ("Content-Type", "application/json");
			requestHeaders.Add ("X-Autofire-Game-Id", gameId);
			requestHeaders.Add ("X-Autofire-Player-Id", playerId);
		}

		private static string GetUUID ()
		{
			string uuid = persistence.ReadUUID ();

			if (string.IsNullOrEmpty (uuid)) {
				uuid = guid.NewGUID ();
				persistence.WriteUUID (uuid);
			}

			return uuid;
		}

		public static void Initialize (Initializer initializer)
		{
			long initTs = 0L;

			lock (manageLock) {
				persistence.SetGameId (initializer.gameId);
				initTs = initializer.timestamp;
				string gameId = initializer.gameId;
				string playerId = initializer.playerId;
				if (string.IsNullOrEmpty (playerId))
					playerId = GetUUID ();
				else
					persistence.WriteUUID (playerId);

				logger.LogDebug ("Autofire initializing with Game Id", gameId);
				logger.LogDebug ("Autofire initializing with Player Id", playerId);

				InitializeParameters (
					gameId, playerId,
					initTs, "",
					initializer.headers, initializer.tags);
				isInitialized = true;
			}

			GameEvent init = new Init ().WithTimestamp (initTs);
			SendEvent (init);
		}

		private static void SendBatch (long now, bool willFlush)
		{
			bool willPost = false;
			string payload = "";

			lock (manageLock) {
				if (isInitialized && !httpSending && http.IsOnline ()) {
					
					payload = persistence.ReadSerialized (now, willFlush);
					if (!string.IsNullOrEmpty (payload)) {
						willPost = true;
						logger.LogDebug ("Autofire HTTP request payload", payload);
					}
					if (willPost)
						httpLastAttempt = GameEvent.NowFromEpoch ();
					httpSending = willPost;
					canExit = !willPost;
				}
			}

			if (willPost)
				http.PostJSON (url, requestHeaders, payload, willFlush);
		}

		public static void SendEvent (GameEvent gameEvent)
		{
			bool willSend = false;
			long now = GameEvent.NowFromEpoch ();

			lock (manageLock) {
				if (isInitialized) {
					bool isInit = false;
					bool forceBegin = false;
					if (gameEvent.GetType () == typeof(Init)) {
						isInit = true;
						forceBegin = true;
					} else if (gameEvent.GetType () == typeof(Progress)) {
						Progress progress = (Progress)gameEvent;
						currentHeader.atLevel = progress.GetLevel ();
						currentSerializedHeader = json.JsonifyHeader (currentHeader.ToRaw ());
					}
					string serializedEvent = json.JsonifyEvent (gameEvent.ToRaw ());

					bool forceEnd = now - httpLastAttempt > sendIntervalSec;

					int appendResult = persistence.WriteSerialized (
						                   gameEvent.timestamp,
						                   serializedEvent,
						                   currentSerializedHeader,
						                   currentSerializedTags,
						                   forceBegin,
						                   forceEnd);
					willSend = isInit || appendResult == 1;
				}
			}

			if (willSend)
				SendBatch (now, false);
		}

		private static void FlushEvents ()
		{
			bool willCheck = false;
			long now = GameEvent.NowFromEpoch ();

			lock (manageLock) {
				isFlushing = true;
				persistence.PersistToDisk ();
				http.SetRequestTimeout (5);
				willCheck = checkExit;
			}

			SendBatch (now, true);

			if (willCheck) {
				//
				// NOTE: if a previous async request hasn't finished and
				//       the host environment exits, we have to wait
				//       Make sure Unity doesn't execute this!
				//
				bool done = false;
				int i = 0;
				while (!done) {
					i++;
					lock (manageLock) {
						done = canExit || i > 10;
					}
					System.Threading.Thread.Sleep (500);
				}
			}
		}

		public static void Deinitialize (Initializer initializer = null)
		{
			GameEvent deinit = new Deinit ();

			if (initializer != null)
				deinit = deinit.WithTimestamp (initializer.timestamp);
			string serializedEvent = json.JsonifyEvent (deinit.ToRaw ());

			lock (manageLock) {
				if (isInitialized)
					persistence.WriteSerialized (
						deinit.timestamp,
						serializedEvent,
						currentSerializedHeader,
						currentSerializedTags,
						false,
						false);
			}

			FlushEvents ();
		}

	}

}
