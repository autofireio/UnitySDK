namespace AutofireClient.Event
{

	internal class Deinit : GameEvent
	{

		public const string DEINIT_NAME = "DEINIT";

		public Deinit () :
			base (DEINIT_NAME)
		{
		}

	}

}
