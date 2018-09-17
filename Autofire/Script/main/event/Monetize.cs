namespace AutofireClient.Event
{

	public class Monetize : GameEvent
	{

		public const string MONETIZE_NAME = "MONETIZE";

		private string monetizeName;
		private int ac;
		private int qty;

		public Monetize (string name, int ac, int qty) :
			base (MONETIZE_NAME)
		{
			this.monetizeName = name;
			this.ac = ac;
			this.qty = qty;
			WithPredefinedFeature ("name", name);
			WithPredefinedFeature ("ac", ac);
			WithPredefinedFeature ("qty", qty);
		}

		public string GetName ()
		{
			return monetizeName;
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
