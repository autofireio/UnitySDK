namespace AutofireClient.Event
{

	public class Action : GameEvent
	{

		public const string ACTION_NAME = "ACTION";

		private string what;

		public Action (string what) :
			base (ACTION_NAME)
		{
			this.what = what;
			WithPredefinedFeature ("what", what);
		}

		public string GetWhat ()
		{
			return what;
		}

	}

}
