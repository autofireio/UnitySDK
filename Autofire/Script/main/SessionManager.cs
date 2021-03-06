﻿using System;
using System.Collections.Generic;
using AutofireClient.Event;
using AutofireClient.Iface;

namespace AutofireClient
{

	public static class SessionManager
	{

		private const int MAX_SEND_INTERVAL_SEC = 90;
		private const int HTTP_REQUEST_TIMEOUT_SEC = 10;
		private const int HTTP_REQUEST_TIMEOUT_FLUSH_SEC = 5;

		private static object manageLock = new object ();

		private static ILoggerProvider logger;
		private static IEnvironmentProvider environment;
		private static IPersistenceProvider persistence;
		private static IGUIDProvider guid;
		private static IBatchEncoderProvider encoder;
		private static IHTTPProvider http;

		private static bool checkExit = false;
		private static bool isSetUp = false;
		private static bool isInitialized = false;
		private static bool isFlushing = false;
		private static bool canExit = true;
		private static bool httpSending = false;
		// NOTE: set to 0 in order to send the INIT asap (i.e, bypass batching for INIT)
		private static long httpLastAttempt = GameEvent.NowFromEpoch ();
		private static Header currentHeader;
		private static string currentSerializedHeader;
		private static List<string> currentTags;
		private static string currentSerializedTags;

		private static int sendIntervalSec = MAX_SEND_INTERVAL_SEC;
		private static string apiURL = "https://service.autofire.io/api/v1";
		private static string dataPointsPath = "command/clients/datapoints";
		private static Dictionary<string, string> requestHeaders;

		private static void WriteErrLine (string what)
		{
			Console.Error.WriteLine (what);
		}

		private static void WriteInitErrLine (string what)
		{
			WriteErrLine ("Autofire initialization error: " + what);
		}

		private static void HandleHTTPResponse (HelperHTTPResponse response)
		{
			bool checkNext = true;
			bool willFlush = false;
			long now = GameEvent.NowFromEpoch ();

			lock (manageLock) {
				if (isInitialized) {
					logger.LogDebug ("Autofire HTTP response", response.ToString ());

					willFlush = isFlushing;
					httpSending = false;
					if (response.IsDiscardable ())
						checkNext = persistence.CommitReadSerialized ();
					else {
						checkNext = false;
						if (isFlushing) {
							isFlushing = false;
							willFlush = false;
							http.SetRequestTimeout (HTTP_REQUEST_TIMEOUT_SEC);
						}
					}
					canExit = !checkNext;
				} else {
					checkNext = false;

					WriteInitErrLine ("Cannot handle HTTP response" + response.ToString ());
				}
			}

			if (checkNext)
				SendBatch (now, willFlush);
		}

		private class ResponseHandler : HelperHTTPResponseHandler
		{
			public void HandleResponse (HelperHTTPResponse response)
			{
				HandleHTTPResponse (response);
			}
		}

		private static ResponseHandler responseHandler = new ResponseHandler ();

		public static void SetEnvironmentProvider (IEnvironmentProvider environment)
		{
			lock (manageLock) {
				if (environment == null) {
					WriteInitErrLine ("Null IEnvironmentProvider");
					return;
				}
				SessionManager.environment = environment;
			}
		}

		public static void SetProviders (ILoggerProvider logger,
		                                 IEnvironmentProvider environment,
		                                 IPersistenceProvider persistence,
		                                 IGUIDProvider guid,
		                                 IBatchEncoderProvider encoder,
		                                 IHTTPProvider http)
		{
			lock (manageLock) {
				if (logger == null) {
					WriteInitErrLine ("Null ILoggerProvider");
					return;
				}
				SessionManager.logger = logger;

				if (environment == null) {
					WriteInitErrLine ("Null IEnvironmentProvider");
					return;
				}
				SessionManager.environment = environment;

				if (persistence != null && SessionManager.persistence != null) {
					SessionManager.logger.LogDebug ("Autofire IPersistenceProvider changed",
						"Resetting - Possible loss of events");
					String playerId = SessionManager.persistence.ReadUUID ();
					SessionManager.persistence.Reset ();
					SessionManager.persistence.WriteUUID (playerId);
				} else if (persistence == null) {
					WriteInitErrLine ("Null IPersistenceProvider");
					return;
				} else if (SessionManager.persistence != null)
					SessionManager.persistence.PersistToDisk ();
				SessionManager.persistence = persistence;
				if (!SessionManager.persistence.IsAvailable ())
					SessionManager.persistence = new AutofireClient.Util.MemPersistenceImpl ();

				if (guid == null) {
					WriteInitErrLine ("Null IGUIDProvider");
					return;
				}
				SessionManager.guid = guid;

				if (encoder == null) {
					WriteInitErrLine ("Null IBatchEncoderProvider");
					return;
				}
				SessionManager.encoder = encoder;

				if (http == null) {
					WriteInitErrLine ("Null IHTTPProvider");
					return;
				}
				SessionManager.http = http;
				SessionManager.http.SetRequestTimeout (HTTP_REQUEST_TIMEOUT_SEC);

				isSetUp = true;
			}
		}

		public static void SetCheckExit (bool checkExit)
		{
			lock (manageLock) {
				SessionManager.checkExit = checkExit;
			}
		}

		public static void SetRetention (long retentionInSecs)
		{
			lock (manageLock) {
				if (isSetUp)
					persistence.SetRetention (retentionInSecs);
				else
					WriteInitErrLine ("Cannot set retention");
			}
		}

		public static void SetSendInterval (int intervalSecs)
		{
			lock (manageLock) {
				if (intervalSecs <= MAX_SEND_INTERVAL_SEC)
					sendIntervalSec = intervalSecs;
				else
					sendIntervalSec = MAX_SEND_INTERVAL_SEC;
			}
		}

		public static void SetApiURL (string apiURL)
		{
			lock (manageLock) {
				if (!string.IsNullOrEmpty (apiURL))
					SessionManager.apiURL = apiURL;
			}
		}

		public static void SetDataPointsPath (string dataPointsPath)
		{
			lock (manageLock) {
				if (!string.IsNullOrEmpty (dataPointsPath))
					SessionManager.dataPointsPath = dataPointsPath;
			}
		}

		private static void InitializeParameters (string gameId, string playerId,
		                                          long now, string atLevel, 
		                                          Dictionary<string, string> headerFeatures, List<string> tags)
		{
			currentHeader = new Header (environment, headerFeatures, now, atLevel);
			currentSerializedHeader = encoder.EncodeHeader (currentHeader.ToRaw ());
			currentTags = new List<string> ();
			if (tags != null)
				currentTags = new List<string> (tags);
			currentSerializedTags = encoder.EncodeTags (currentTags);

			requestHeaders = new Dictionary<string, string> ();
			requestHeaders.Add ("X-Autofire-Game-Id", gameId);
			requestHeaders.Add ("X-Autofire-Player-Id", playerId);
		}

		private static string GetUUID (string hint)
		{
			string uuid = persistence.ReadUUID ();

			if (!string.IsNullOrEmpty (hint)) {
				if (hint != uuid)
					persistence.WriteUUID (hint);

				return hint;
			} else if (!string.IsNullOrEmpty (uuid))
				return uuid;

			uuid = guid.NewGUID ();
			persistence.WriteUUID (uuid);

			return uuid;
		}

		public static void Initialize (Initializer initializer)
		{
			long initTs = 0L;

			lock (manageLock) {
				if (isSetUp) {
					initTs = initializer.timestamp;
					string gameId = initializer.gameId;
					string playerId = initializer.playerId;

					persistence.SetGameId (gameId);
					string ver = persistence.GetAutofireVersion ();

					// NOTE: handle Autofire version update here
					if (string.IsNullOrEmpty (ver) || ver != Version.VERSION) {
						string guid = persistence.ReadUUID ();
						persistence.Reset ();
						if (!string.IsNullOrEmpty (guid))
							persistence.WriteUUID (guid);
					}

					if (ver != Version.VERSION)
						persistence.SetAutofireVersion (Version.VERSION);

					playerId = GetUUID (playerId);
					if (string.IsNullOrEmpty (playerId))
						playerId = "nobody";

					logger.LogDebug ("Autofire initializing with Game Id", gameId);
					logger.LogDebug ("Autofire initializing with Player Id", playerId);

					InitializeParameters (
						gameId, playerId,
						initTs, "",
						initializer.headers, initializer.tags);
					isInitialized = true;
				} else
					WriteInitErrLine ("Not properly set up");
			}

			GameEvent init = new Init ().WithTimestamp (initTs);
			SendEvent (init);
		}

		private static void SendBatch (long now, bool willFlush)
		{
			bool willPost = false;
			string dataPointsURL = "";
			string payload = "";

			lock (manageLock) {
				if (isInitialized && !httpSending && http.IsOnline ()) {
					dataPointsURL = apiURL + "/" + dataPointsPath;
					payload = persistence.ReadSerialized (
						now,
						encoder.GetEventsEnd (),
						encoder.GetBatchEnd (),
						willFlush);
					if (!string.IsNullOrEmpty (payload)) {
						willPost = true;
						httpLastAttempt = GameEvent.NowFromEpoch ();

						logger.LogDebug ("Autofire HTTP request payload", payload);
					}
					httpSending = willPost;
					canExit = !willPost;
				}
			}

			if (willPost)
				http.PostData (responseHandler, dataPointsURL,
					encoder.GetContentType (), "application/json",
					requestHeaders,
					payload,
					willFlush);
		}

		public static void SendEvents (IEnumerable<GameEvent> gameEvents)
		{
			bool willSend = false;
			long now = GameEvent.NowFromEpoch ();

			lock (manageLock) {
				if (isInitialized) {
					long lastTimestamp = now;
					bool forceBegin = false;
					string atLevel = null;
					List<string> serializedEvents = new List<string> ();
					foreach (GameEvent gameEvent in gameEvents) {
						if (gameEvent.GetType () == typeof(Init))
							forceBegin = true;
						else if (gameEvent.GetType () == typeof(Progress)) {
							Progress progress = (Progress)gameEvent;
							atLevel = progress.GetLevel ();
						}
						serializedEvents.Add (encoder.EncodeEvent (gameEvent.ToRaw ()));
						lastTimestamp = gameEvent.timestamp;
					}
					bool forceEnd = now - httpLastAttempt > sendIntervalSec;

					int appendResult = persistence.WriteSerialized (
						                   encoder.GetSeparator (),
						                   encoder.GetBatchBegin (),
						                   currentSerializedHeader,
						                   currentSerializedTags,
						                   encoder.GetEventsBegin (),
						                   serializedEvents,
						                   lastTimestamp,
						                   forceBegin,
						                   forceEnd);
					willSend = appendResult == 1;

					if (!string.IsNullOrEmpty (atLevel)) {
						currentHeader.atLevel = atLevel;
						currentSerializedHeader = encoder.EncodeHeader (currentHeader.ToRaw ());
					}
				} else
					WriteInitErrLine ("Cannot send events");
			}

			if (willSend)
				SendBatch (now, false);
		}

		public static void SendEvent (GameEvent gameEvent)
		{
			SendEvents (new GameEvent[] { gameEvent });
		}

		public static void PersistToDisk ()
		{
			lock (manageLock) {
				if (isInitialized)
					persistence.PersistToDisk ();
				else
					WriteInitErrLine ("Cannot persist to disk");
			}
		}

		private static void PrepareFlush ()
		{
			isFlushing = true;
			persistence.PersistToDisk ();
			http.SetRequestTimeout (HTTP_REQUEST_TIMEOUT_FLUSH_SEC);
		}

		public static void FlushEvents ()
		{
			long now = GameEvent.NowFromEpoch ();

			lock (manageLock) {
				if (isInitialized)
					PrepareFlush ();
				else
					WriteInitErrLine ("Cannot flush events");
			}

			SendBatch (now, true);
		}

		public static void Deinitialize (Initializer initializer = null)
		{
			long now = GameEvent.NowFromEpoch ();

			GameEvent deinit = new Deinit ();
			if (initializer != null)
				deinit = deinit.WithTimestamp (initializer.timestamp);

			lock (manageLock) {
				if (isInitialized) {
					string serializedEvent = encoder.EncodeEvent (deinit.ToRaw ());

					// NOTE: best effort
					persistence.WriteSerialized (
						encoder.GetSeparator (),
						encoder.GetBatchBegin (),
						currentSerializedHeader,
						currentSerializedTags,
						encoder.GetEventsBegin (),
						new string[] { serializedEvent },
						deinit.timestamp,
						false,
						false);
					PrepareFlush ();
					// NOTE: don't set isInitialized to false, due to pending batches over HTTP
				} else
					WriteErrLine ("Autofire de-initialization error: Not initialized");
			}

			SendBatch (now, true);
		}

		public static void Shutdown ()
		{
			bool willCheck = false;

			lock (manageLock) {
				willCheck = checkExit;
			}

			if (willCheck) {
				//
				// NOTE: if a previous async request hasn't finished and
				//       the host environment exits, we have to wait.
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

	}

}
