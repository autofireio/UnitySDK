namespace AutofireClient.Event
{
	
	public class Progress : GameEvent
	{

		private string level;

		public Progress (string level, int score) :
			base ("PROGRESS")
		{
			this.level = level;
			WithPredefinedFeature ("level", level);
			WithPredefinedFeature ("score", score);
		}

		public string GetLevel ()
		{
			return this.level;
		}

	}

}
