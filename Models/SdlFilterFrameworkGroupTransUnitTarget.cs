using System.Collections.ObjectModel;

namespace TranslatorApp.Models
{
    public class SdlFilterFrameworkGroupTransUnitTarget
	{
		public Collection<SdlFilterFrameworkGroupTransUnitItem> Xgm
		{
			get;
			set;
		}

		public SdlFilterFrameworkGroupTransUnitTarget()
		{
			this.Xgm = new Collection<SdlFilterFrameworkGroupTransUnitItem>();
		}
	}
}