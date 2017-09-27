namespace AutofireClient.Event
{

	internal class Custom : GameEvent
	{

		public Custom (string name) :
			base (GameEvent.SanitizeName (name))
		{
		}

	}

}
