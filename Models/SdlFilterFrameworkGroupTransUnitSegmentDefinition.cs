using System.Collections.ObjectModel;

namespace TranslatorApp.Models
{
    public class SdlFilterFrameworkGroupTransUnitSegmentDefinition
	{
		public Collection<SdlFilterFrameworkGroupTransUnitSegment> Seg
		{
			get;
			set;
		}

		public SdlFilterFrameworkGroupTransUnitSegmentDefinition()
		{
			this.Seg = new Collection<SdlFilterFrameworkGroupTransUnitSegment>();
		}
	}
}