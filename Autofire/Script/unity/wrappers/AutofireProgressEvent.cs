using UnityEngine;

namespace AutofireClient.Unity
{

	public class AutofireProgressEvent : MonoBehaviour
	{

		public string level;
		public int score;

		public void ProgressEvent ()
		{
			Autofire.Progress (level, score);
		}

	}

}
