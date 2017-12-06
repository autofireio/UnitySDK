namespace AutofireClient.Event
{

	internal class Init : GameEvent
	{

		public const string INIT_NAME = "INIT";

		public Init () :
			base (INIT_NAME)
		{
		}

	}

}
