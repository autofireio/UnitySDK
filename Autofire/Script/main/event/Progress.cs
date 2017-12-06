namespace AutofireClient.Event
{
	
	public class Progress : GameEvent
	{

		public const string PROGRESS_NAME = "PROGRESS";

		private string level;
		private int score;

		public Progress (string level, int score) :
			base (PROGRESS_NAME)
		{
			this.level = level;
			this.score = score;
			WithPredefinedFeature ("level", level);
			WithPredefinedFeature ("score", score);
		}

		public string GetLevel ()
		{
			return level;
		}

		public int GetScore ()
		{
			return score;
		}

	}

}
