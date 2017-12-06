namespace AutofireClient.Event
{

	public class Monetize : GameEvent
	{

		public const string MONETIZE_NAME = "MONETIZE";

		private string item;
		private int ac;
		private int qty;

		public Monetize (string item, int ac, int qty) :
			base (MONETIZE_NAME)
		{
			this.item = item;
			this.ac = ac;
			this.qty = qty;
			WithPredefinedFeature ("name", item);
			WithPredefinedFeature ("ac", ac);
			WithPredefinedFeature ("qty", qty);
		}

		public string GetItem ()
		{
			return item;
		}

		public int GetAc ()
		{
			return ac;
		}

		public int GetQty ()
		{
			return qty;
		}

	}

}
