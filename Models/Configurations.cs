using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace TranslatorApp.Models
{
    public class Configurations
	{
		public bool AllowMultipleInstance
		{
			get;
			set;
		}

		public Dictionary<string, string> CodedStrings
		{
			get;
			set;
		}

		public List<SelectableItem> FileTypes
		{
			get;
			set;
		}

		public JObject GoogleApi
		{
			get;
			set;
		}

		public List<SelectableItem> Languages
		{
			get;
			set;
		}

		public ushort MinimumTranslationPercentage
		{
			get;
			set;
		}

		public List<string> WhiteSpacedStrings
		{
			get;
			set;
		}

		public Configurations()
		{
			this.Languages = new List<SelectableItem>();
			this.FileTypes = new List<SelectableItem>();
			this.CodedStrings = new Dictionary<string, string>();
			this.WhiteSpacedStrings = new List<string>();
		}
	}
}