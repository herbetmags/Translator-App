namespace TranslatorApp.Models
{
    public class SdlFilterFrameworkGroup
	{
		public SdlFilterFrameworkGroupTransUnit Tr
		{
			get;
			set;
		}

		public SdlFilterFrameworkGroup()
		{
			this.Tr = new SdlFilterFrameworkGroupTransUnit();
		}
	}
}