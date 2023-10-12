using System.Collections.ObjectModel;

namespace TranslatorApp.Models
{
    public class SdlFilterFrameworkBody
	{
		public Collection<SdlFilterFrameworkGroup> Gr
		{
			get;
			set;
		}

		public SdlFilterFrameworkBody()
		{
			this.Gr = new Collection<SdlFilterFrameworkGroup>();
		}
	}
}