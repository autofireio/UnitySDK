namespace AutofireClient.Event
{

	public class Resource : GameEvent
	{

		public const string RESOURCE_NAME = "RESOURCE";

		private string resourceName;
		private int qty;

		public Resource (string name, int qty) :
			base (RESOURCE_NAME)
		{
			this.resourceName = name;
			this.qty = qty;
			WithPredefinedFeature ("name", name);
			WithPredefinedFeature ("qty", qty);
		}

		public string GetName ()
		{
			return resourceName;
		}

		public int GetQty ()
		{
			return qty;
		}

	}

}
