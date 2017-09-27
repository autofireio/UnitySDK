using UnityEngine;

namespace AutofireClient.Unity
{

	public class AutofireResourceEvent : MonoBehaviour
	{

		public string resourceName;
		public int qty;

		public void ResourceEvent ()
		{
			Autofire.Resource (resourceName, qty);
		}

	}

}
