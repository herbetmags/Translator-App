namespace TranslatorApp.Models
{
    public class SdlFilterFramework
	{
		public SdlFilterFrameworkBody Bd
		{
			get;
			set;
		}

		public SdlFilterFramework()
		{
			this.Bd = new SdlFilterFrameworkBody();
		}
	}
}