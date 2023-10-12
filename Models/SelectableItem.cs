namespace TranslatorApp.Models
{
    public class SelectableItem
	{
		public string Code
		{
			get;
			set;
		}

		public bool Enabled
		{
			get;
			set;
		}

		public string Name
		{
			get;
			set;
		}

		public SelectableItem()
		{
		}
	}
}