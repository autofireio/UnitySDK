namespace AutofireClient.Event
{

	public class Resource : GameEvent
	{

		public Resource (string name, int qty) :
			base ("RESOURCE")
		{
			WithPredefinedFeature ("name", name);
			WithPredefinedFeature ("qty", qty);
		}

	}

}
