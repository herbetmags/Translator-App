using System.Collections.ObjectModel;

namespace TranslatorApp.Models
{
    public class SdlFilterFrameworkGroupTransUnitItem
	{
		public string Id
		{
			get;
			set;
		}

		public string Txt
		{
			get;
			set;
		}

		public Collection<SdlFilterFrameworkGroupTransUnitItem> Xg
		{
			get;
			set;
		}

		public SdlFilterFrameworkGroupTransUnitItem()
		{
			this.Xg = new Collection<SdlFilterFrameworkGroupTransUnitItem>();
		}
	}
}