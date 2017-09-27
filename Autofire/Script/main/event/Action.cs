namespace AutofireClient.Event
{

	public class Action : GameEvent
	{

		public Action (string what) :
			base ("ACTION")
		{
			WithPredefinedFeature ("what", what);
		}

	}

}
