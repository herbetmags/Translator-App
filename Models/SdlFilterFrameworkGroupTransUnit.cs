namespace TranslatorApp.Models
{
    public class SdlFilterFrameworkGroupTransUnit
	{
		public string Id
		{
			get;
			set;
		}

		public SdlFilterFrameworkGroupTransUnitSegmentDefinition Sgd
		{
			get;
			set;
		}

		public SdlFilterFrameworkGroupTransUnitTarget Tg
		{
			get;
			set;
		}

		public SdlFilterFrameworkGroupTransUnit()
		{
			this.Tg = new SdlFilterFrameworkGroupTransUnitTarget();
			this.Sgd = new SdlFilterFrameworkGroupTransUnitSegmentDefinition();
		}
	}
}