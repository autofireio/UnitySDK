namespace AutofireClient.Event
{

	public class Monetize : GameEvent
	{

		public Monetize (string name, int ac, int qty) :
			base ("MONETIZE")
		{
			WithPredefinedFeature ("name", name);
			WithPredefinedFeature ("ac", ac);
			WithPredefinedFeature ("qty", qty);
		}

	}

}
