namespace AutofireClient.Iface
{

	public class HelperHTTPResponse
	{
		
		public int code;
		public string body;

		public HelperHTTPResponse (int code, string body)
		{
			this.code = code;
			this.body = body;
		}

		public bool Succeeded ()
		{
			return code >= 200 && code < 300;
		}

		public bool IsDiscardable ()
		{
			return Succeeded () || (code == 400 || code == 404);
		}

		public override string ToString ()
		{
			return "{\t\"code\": " + code + ",\t\"body\": \"" + body + "\"\t}";
		}

	}

}
