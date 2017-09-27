using UnityEngine;

namespace AutofireClient.Unity
{

	public class AutofireMonetizeEvent : MonoBehaviour
	{

		public string item;
		public int ac;
		public int qty;

		public void MonetizeEvent ()
		{
			Autofire.Monetize (item, ac, qty);
		}

	}

}
