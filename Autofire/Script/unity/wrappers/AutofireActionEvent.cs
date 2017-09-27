using UnityEngine;

namespace AutofireClient.Unity
{
	
	public class AutofireActionEvent : MonoBehaviour
	{

		public string what;

		public void ActionEvent ()
		{
			Autofire.Action (what);
		}

	}

}
