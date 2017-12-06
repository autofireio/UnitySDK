namespace AutofireClient.Event
{

	internal class Custom : GameEvent
	{

		private string SanitizeCustom (string customName)
		{
			if (customName == Action.ACTION_NAME ||
			    customName == Deinit.DEINIT_NAME ||
			    customName == Init.INIT_NAME ||
			    customName == Monetize.MONETIZE_NAME ||
			    customName == Progress.PROGRESS_NAME ||
			    customName == Resource.RESOURCE_NAME)
				return "_" + customName;
			else
				return customName;
		}

		public Custom (string name) :
			base (GameEvent.SanitizeName (name))
		{
			this.name = SanitizeCustom (this.name);
		}

	}

}
