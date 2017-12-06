using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using AutofireClient.Iface;

namespace AutofireClient.Unity
{

	public class HTTPImpl : MonoBehaviour, IHTTPProvider
	{

		// in seconds - used only for synchronous requests
		private int timeoutHint = 10;

		public bool IsOnline ()
		{
			return true;
		}

		public void SetRequestTimeout (int secs)
		{
			if (secs > 0)
				timeoutHint = secs;
		}

		private static int ParseResponseCode (string statusLine)
		{
			int ret = 0;

			string[] components = statusLine.Split (' ');
			if (components.Length >= 3) {
				if (!int.TryParse (components [1], out ret))
					ret = 0;
			}

			return ret;
		}

		private static int GetResponseCode (WWW request)
		{
			int ret = 0;
			if (request.responseHeaders != null) {
				if (request.responseHeaders.ContainsKey ("STATUS"))
					ret = ParseResponseCode (request.responseHeaders ["STATUS"]);
			}

			return ret;
		}

		private void HandleHTTPResponse (HelperHTTPResponseHandler responseHandler,
		                                 int code,
		                                 string body)
		{
			HelperHTTPResponse response = new HelperHTTPResponse (code, body);
			responseHandler.HandleResponse (response);
		}

		private void DoResponse (HelperHTTPResponseHandler responseHandler,
		                         WWW www)
		{
			int code = 0;
			string body = "";

			if (www.error == null) {
				code = GetResponseCode (www);
				body = www.text;
			} else {
				code = GetResponseCode (www);
				body = www.error;
			}

			HandleHTTPResponse (responseHandler, code, body);
		}

		// reference: http://blog.cyberiansoftware.com.ar/post/142258870245/synchronous-web-request-using-unity3d-api
		private IEnumerator SyncFlushEvents (HelperHTTPResponseHandler responseHandler,
		                                     WWW www)
		{
			while (!www.isDone)
				yield return www;

			DoResponse (responseHandler, www);
		}

		private IEnumerator ContinueFlushEvents (HelperHTTPResponseHandler responseHandler,
		                                         WWW www)
		{
			yield return www;

			DoResponse (responseHandler, www);
		}

		public void PostData (HelperHTTPResponseHandler responseHandler,
		                      string url,
		                      string contentType,
		                      string acceptType,
		                      Dictionary<string, string> headers,
		                      string body,
		                      bool forceSync = false)
		{
			byte[] data = Encoding.ASCII.GetBytes (body.ToCharArray ());
			Dictionary<string, string> headers2 = new Dictionary<string, string> (headers);
			if (!string.IsNullOrEmpty (contentType))
				headers2.Add ("Content-Type", contentType);
			if (!string.IsNullOrEmpty (acceptType))
				headers2.Add ("Accept", acceptType);

			WWW www = new WWW (url, data, headers);
			if (!forceSync)
				StartCoroutine (ContinueFlushEvents (responseHandler, www));
			else {
				long strt = AutofireClient.Event.GameEvent.NowFromEpoch ();
				IEnumerator e = SyncFlushEvents (responseHandler, www);
				while (e.MoveNext ()) {
					if (AutofireClient.Event.GameEvent.NowFromEpoch () - strt >= timeoutHint)
						break;
				}
			}
		}

	}

}
