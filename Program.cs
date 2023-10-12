using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Windows.Forms;
using System.Xml;
using TranslatorApp.Models;

namespace TranslatorApp
{
    internal static class Program
	{
		private static Configurations _configuration;

		private static Form1 _form;

		private static Collection<SdlFilterFrameworkGroup> _translationItems;

		static Program()
		{
			Program._translationItems = new Collection<SdlFilterFrameworkGroup>();
		}

		public static bool CheckInstance(bool allowMultiple)
		{
			bool flag;
			try
			{
				Process[] processesByName = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName);
				flag = (allowMultiple ? true : processesByName.Count<Process>() <= 1);
			}
			catch
			{
				flag = false;
			}
			return flag;
		}

		private static string DenormalizeContent(string content)
		{
			if (!string.IsNullOrWhiteSpace(content))
			{
				foreach (KeyValuePair<string, string> codedString in Program._configuration.CodedStrings)
				{
					char chr = '\u00A0';
					if (content.Contains<char>(chr))
					{
						content = content.Replace(chr, ' ');
					}
					content = content.Replace(codedString.Key, codedString.Value);
				}
			}
			return content;
		}

		public static bool Extract(FormModel model)
		{
			bool flag;
			try
			{
				Program._translationItems = Program.ExtractTranslationItems(Program.GetFile<XmlDocument>(model.FilePath));
				flag = Program._translationItems.Any<SdlFilterFrameworkGroup>();
				MessageBox.Show(Program._form, "Translation resource has been extracted successfully.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				MessageBox.Show(Program._form, exception.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				flag = false;
			}
			return flag;
		}

		private static string ExtractHtmlContent(string content, string elementClass)
		{
			string innerText;
			HtmlAgilityPack.HtmlDocument htmlDocument = new HtmlAgilityPack.HtmlDocument();
			htmlDocument.LoadHtml(content);
			HtmlNode htmlNode = htmlDocument.DocumentNode.Element("html").Element("body").Descendants("div").FirstOrDefault<HtmlNode>((HtmlNode d) => d.HasClass(elementClass));
			if (htmlNode != null)
			{
				innerText = htmlNode.InnerText;
			}
			else
			{
				innerText = null;
			}
			return Program.RemoveSpaces(HttpUtility.HtmlDecode(innerText));
		}

		private static Collection<SdlFilterFrameworkGroup> ExtractTranslationItems(XmlDocument document)
		{
			XmlElement xmlElement;
			XmlElement xmlElement1;
			Collection<SdlFilterFrameworkGroup> sdlFilterFrameworkGroups = new Collection<SdlFilterFrameworkGroup>();
			foreach (XmlElement groupElement in Program.GetGroupElements(document))
			{
				XmlElement xmlElement2 = groupElement.GetElementsByTagName("trans-unit").Cast<XmlElement>().First<XmlElement>();
				string attribute = xmlElement2.GetAttribute("id");
				XmlNodeList elementsByTagName = xmlElement2.GetElementsByTagName("target");
				if (elementsByTagName.Count > 0)
				{
					xmlElement = elementsByTagName.Cast<XmlElement>().First<XmlElement>();
				}
				else
				{
					xmlElement = null;
				}
				XmlElement xmlElement3 = xmlElement;
				XmlNodeList xmlNodeLists = xmlElement2.GetElementsByTagName("sdl:seg-defs");
				if (xmlNodeLists.Count > 0)
				{
					xmlElement1 = xmlNodeLists.Cast<XmlElement>().First<XmlElement>();
				}
				else
				{
					xmlElement1 = null;
				}
				XmlElement xmlElement4 = xmlElement1;
				Collection<SdlFilterFrameworkGroupTransUnitItem> sdlFilterFrameworkGroupTransUnitItems = new Collection<SdlFilterFrameworkGroupTransUnitItem>();
				SdlFilterFrameworkGroupTransUnit sdlFilterFrameworkGroupTransUnit = new SdlFilterFrameworkGroupTransUnit();
				if (xmlElement3 != null)
				{
					Program.InitializeTransUnitItems(xmlElement3, sdlFilterFrameworkGroupTransUnitItems);
					sdlFilterFrameworkGroupTransUnit.Id = attribute.ToUpperInvariant();
					sdlFilterFrameworkGroupTransUnit.Tg.Xgm = sdlFilterFrameworkGroupTransUnitItems;
				}
				if (xmlElement4 != null)
				{
					sdlFilterFrameworkGroupTransUnit.Sgd.Seg = Program.InitializeTransUnitItemSegments(xmlElement4);
				}
				sdlFilterFrameworkGroups.Add(new SdlFilterFrameworkGroup()
				{
					Tr = sdlFilterFrameworkGroupTransUnit
				});
			}
			return sdlFilterFrameworkGroups;
		}

		private static string GenerateElementId(object node, int index, params string[] identifiers)
		{
			string value;
			XmlNode xmlNodes = (XmlNode)node;
			XmlElement parentNode = (XmlElement)xmlNodes.ParentNode;
			string attribute = null;
			string str = null;
			string[] strArrays = identifiers;
			for (int i = 0; i < (int)strArrays.Length; i++)
			{
				string str1 = strArrays[i];
				if (xmlNodes.Attributes != null)
				{
					XmlAttribute itemOf = xmlNodes.Attributes[str1];
					if (itemOf != null)
					{
						value = itemOf.Value;
					}
					else
					{
						value = null;
					}
				}
				else
				{
					value = null;
				}
				str = value;
				if (string.IsNullOrEmpty(attribute))
				{
					attribute = parentNode.GetAttribute(str1);
				}
				if (!string.IsNullOrEmpty(str))
				{
					break;
				}
			}
			return string.Format("{0}.{1}", str ?? attribute, index);
		}

		public static AssemblyName GetAssemblyName()
		{
			return Assembly.GetExecutingAssembly().GetName();
		}

		public static Configurations GetConfigurations()
		{
			return Program._configuration;
		}

		public static T GetFile<T>(string filePath)
		where T : class, new()
		{
			if (!File.Exists(filePath))
			{
				throw new FileNotFoundException(string.Concat("File was missing.\n", filePath));
			}
			Type type = typeof(T);
			if (type != typeof(XmlDocument))
			{
				return default(T);
			}
			string str = File.ReadAllText(filePath);
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(str);
			return (T)Convert.ChangeType(xmlDocument, type);
		}

		private static IEnumerable<XmlElement> GetGroupElements(XmlDocument document)
		{
			return document.GetElementsByTagName("file").Cast<XmlElement>().First<XmlElement>().GetElementsByTagName("body").Cast<XmlElement>().First<XmlElement>().GetElementsByTagName("group").Cast<XmlElement>();
		}

		private static IEnumerable<XmlElement> GetMatchedChildElement(IEnumerable<XmlElement> parents, string childName)
		{
			IEnumerable<XmlElement> matchedChildType = Program.GetMatchedChildType<XmlElement>(parents);
			IEnumerable<XmlElement> xmlElements = 
				from x in matchedChildType
				where x.Name.Equals(childName, StringComparison.InvariantCultureIgnoreCase)
				select x;
			if (!string.IsNullOrEmpty(childName))
			{
				return xmlElements;
			}
			return matchedChildType;
		}

		private static XmlElement GetMatchedChildElement(IEnumerable<XmlElement> parents, string childName, string childId)
		{
			return Program.GetMatchedChildElement(parents, childName).FirstOrDefault<XmlElement>((XmlElement x) => {
				if (x.GetAttribute("id") != null && x.GetAttribute("id").Equals(childId, StringComparison.InvariantCultureIgnoreCase))
				{
					return true;
				}
				return x.GetAttribute("mid") == childId;
			});
		}

		private static IEnumerable<T> GetMatchedChildType<T>(IEnumerable<XmlElement> parents)
		{
			Type type = typeof(T);
			return parents.SelectMany<XmlElement, object>((XmlElement x) => x.ChildNodes.Cast<object>()).Where<object>((object x) => x.GetType().Name == type.Name).Cast<T>();
		}

		private static SdlFilterFrameworkGroupTransUnitItem GetMatchedMarkItem(IEnumerable<SdlFilterFrameworkGroupTransUnitItem> items, string elementId)
		{
			SdlFilterFrameworkGroupTransUnitItem matchedMarkItem = items.FirstOrDefault<SdlFilterFrameworkGroupTransUnitItem>((SdlFilterFrameworkGroupTransUnitItem x) => {
				if (x.Id.Trim() != elementId)
				{
					return false;
				}
				return !string.IsNullOrWhiteSpace(x.Txt);
			});
			IEnumerable<SdlFilterFrameworkGroupTransUnitItem> sdlFilterFrameworkGroupTransUnitItems = items.SelectMany<SdlFilterFrameworkGroupTransUnitItem, SdlFilterFrameworkGroupTransUnitItem>((SdlFilterFrameworkGroupTransUnitItem x) => x.Xg);
			if (matchedMarkItem == null && sdlFilterFrameworkGroupTransUnitItems.Any<SdlFilterFrameworkGroupTransUnitItem>())
			{
				matchedMarkItem = Program.GetMatchedMarkItem(sdlFilterFrameworkGroupTransUnitItems, elementId);
			}
			return matchedMarkItem;
		}

		private static int GetNodeIndex(XmlNodeList childNodes, object child)
		{
			XmlNode xmlNodes = child as XmlNode;
			return childNodes.Cast<XmlNode>().ToList<XmlNode>().IndexOf(xmlNodes);
		}

		public static Collection<SdlFilterFrameworkGroup> GetTranslations()
		{
			return Program._translationItems;
		}

		private static void InitializeTransUnitItems(XmlNode parent, Collection<SdlFilterFrameworkGroupTransUnitItem> items)
		{
			foreach (object childNode in parent.ChildNodes)
			{
				Type type = childNode.GetType();
				SdlFilterFrameworkGroupTransUnitItem sdlFilterFrameworkGroupTransUnitItem = new SdlFilterFrameworkGroupTransUnitItem();
				int nodeIndex = Program.GetNodeIndex(parent.ChildNodes, childNode);
				sdlFilterFrameworkGroupTransUnitItem.Id = Program.GenerateElementId(childNode, nodeIndex, new string[] { "id", "mid" });
				if (type.Name != typeof(XmlElement).Name)
				{
					sdlFilterFrameworkGroupTransUnitItem.Txt = Program.DenormalizeContent((childNode as XmlText).Value);
				}
				else
				{
					Program.InitializeTransUnitItems(childNode as XmlElement, sdlFilterFrameworkGroupTransUnitItem.Xg);
				}
				items.Add(sdlFilterFrameworkGroupTransUnitItem);
			}
		}

		private static Collection<SdlFilterFrameworkGroupTransUnitSegment> InitializeTransUnitItemSegments(XmlNode parent)
		{
			string value;
			string str;
			Collection<SdlFilterFrameworkGroupTransUnitSegment> sdlFilterFrameworkGroupTransUnitSegments = new Collection<SdlFilterFrameworkGroupTransUnitSegment>();
			foreach (object childNode in parent.ChildNodes)
			{
				SdlFilterFrameworkGroupTransUnitSegment sdlFilterFrameworkGroupTransUnitSegment = new SdlFilterFrameworkGroupTransUnitSegment();
				XmlNode xmlNodes = (XmlNode)childNode;
				int num = 0;
				sdlFilterFrameworkGroupTransUnitSegment.Id = Program.GenerateElementId(childNode, num, new string[] { "id" });
				SdlFilterFrameworkGroupTransUnitSegment sdlFilterFrameworkGroupTransUnitSegment1 = sdlFilterFrameworkGroupTransUnitSegment;
				XmlAttribute itemOf = xmlNodes.Attributes["struct-match"];
				if (itemOf != null)
				{
					value = itemOf.Value;
				}
				else
				{
					value = null;
				}
				sdlFilterFrameworkGroupTransUnitSegment1.StructMatch = value;
				SdlFilterFrameworkGroupTransUnitSegment sdlFilterFrameworkGroupTransUnitSegment2 = sdlFilterFrameworkGroupTransUnitSegment;
				XmlAttribute xmlAttribute = xmlNodes.Attributes["percent"];
				if (xmlAttribute != null)
				{
					str = xmlAttribute.Value;
				}
				else
				{
					str = null;
				}
				sdlFilterFrameworkGroupTransUnitSegment2.Percent = str;
				sdlFilterFrameworkGroupTransUnitSegments.Add(sdlFilterFrameworkGroupTransUnitSegment);
			}
			return sdlFilterFrameworkGroupTransUnitSegments;
		}

		public static bool IsAllowedPercentage(string value)
		{
			ushort num;
			if (string.IsNullOrWhiteSpace(value))
			{
				return true;
			}
			if (!ushort.TryParse(value, out num))
			{
				return false;
			}
			return num < Program._configuration.MinimumTranslationPercentage;
		}

		private static bool IsAllowedToUpdate(string text, SdlFilterFrameworkGroupTransUnitSegment segment, FormModel model, out string result)
		{
			bool flag;
			bool flag1;
			result = string.Empty;
			if (string.IsNullOrWhiteSpace(text))
			{
				flag = false;
			}
			else
			{
				flag = (!text.Contains("http") ? true : text.Contains("\\"));
			}
			bool flag2 = flag;
			if (flag2)
			{
				result = Program.TranslateContent(model, text);
				if (string.IsNullOrWhiteSpace(result))
				{
					flag1 = false;
				}
				else
				{
					flag1 = (segment == null ? true : Program.IsAllowedPercentage(segment.Percent));
				}
				flag2 = flag1;
			}
			return flag2;
		}

		private static bool IsStructMatched(string value)
		{
			bool flag;
			if (string.IsNullOrWhiteSpace(value))
			{
				return true;
			}
			return bool.TryParse(value, out flag) & flag;
		}

		[STAThread]
		private static void Main()
		{
			Program._configuration = JsonConvert.DeserializeObject<Configurations>(File.ReadAllText(string.Concat(AppDomain.CurrentDomain.BaseDirectory, "\\Resources\\Configurations.json")));
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			if (Program.CheckInstance(Program._configuration.AllowMultipleInstance))
			{
				Program._form = new Form1();
				Application.Run(Program._form);
				return;
			}
			MessageBox.Show(Program._form, "An instance of this application is already running!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
		}

		private static string NormalizeContent(string content)
		{
			if (!string.IsNullOrWhiteSpace(content))
			{
				foreach (KeyValuePair<string, string> codedString in Program._configuration.CodedStrings)
				{
					content = content.Replace(codedString.Value, codedString.Key);
				}
			}
			return content;
		}

		private static string RemoveSpaces(string content)
		{
			if (!string.IsNullOrWhiteSpace(content))
			{
				foreach (string whiteSpacedString in Program._configuration.WhiteSpacedStrings)
				{
					if (!content.Contains(whiteSpacedString))
					{
						continue;
					}
					content = content.Replace(whiteSpacedString, whiteSpacedString.Trim());
				}
			}
			return content;
		}

		public static void SaveFile(XmlDocument document, string fileName)
		{
			document.Save(fileName);
		}

		public static string TranslateContent(FormModel model, string data)
		{
			string empty = string.Empty;
			try
			{
				using (RestClient restClient = new RestClient((ConfigureRestClient)null, null, null, false))
				{
					JObject googleApi = Program._configuration.GoogleApi;
					string str = googleApi.Value<string>("ResourceUrl");
					string str1 = googleApi.Value<string>("UserAgent");
					string str2 = googleApi.Value<string>("ElementClass");
					RestRequest restRequest = new RestRequest(str, Method.Get);
					restRequest.AddHeader("user-agent", str1);
					restRequest.AddQueryParameter("sl", model.SourceLang, true);
					restRequest.AddQueryParameter("tl", model.TargetLang, true);
					restRequest.AddQueryParameter("hl", "en-US", true);
					restRequest.AddQueryParameter("ie", "UTF-8", true);
					restRequest.AddQueryParameter("q", data, true);
					CancellationToken cancellationToken = new CancellationToken();
					empty = Program.ExtractHtmlContent(restClient.Execute(restRequest, cancellationToken).Content, str2);
				}
			}
			catch (Exception exception)
			{
				throw exception;
			}
			return empty;
		}

		public static string TranslateContent(FormModel model, IEnumerable<SdlFilterFrameworkGroup> items)
		{
			string empty = string.Empty;
			try
			{
				using (RestClient restClient = new RestClient((ConfigureRestClient)null, null, null, false))
				{
					string str = JsonConvert.SerializeObject(items);
					JObject googleApi = Program._configuration.GoogleApi;
					string str1 = googleApi.Value<string>("ResourceUrl");
					string str2 = googleApi.Value<string>("UserAgent");
					string str3 = googleApi.Value<string>("ElementClass");
					RestRequest restRequest = new RestRequest(str1, Method.Get);
					restRequest.AddHeader("user-agent", str2);
					restRequest.AddQueryParameter("sl", model.SourceLang, true);
					restRequest.AddQueryParameter("tl", model.TargetLang, true);
					restRequest.AddQueryParameter("hl", "en-US", true);
					restRequest.AddQueryParameter("ie", "UTF-8", true);
					restRequest.AddQueryParameter("q", str, true);
					CancellationToken cancellationToken = new CancellationToken();
					empty = Program.ExtractHtmlContent(restClient.Execute(restRequest, cancellationToken).Content, str3);
				}
			}
			catch (Exception exception)
			{
				throw exception;
			}
			return empty;
		}

		public static void UpdateFile(ref XmlDocument document, SdlFilterFrameworkGroup item, FormModel model)
		{
			try
			{
				IEnumerable<XmlElement> groupElements = Program.GetGroupElements(document);
				SdlFilterFrameworkGroupTransUnit tr = item.Tr;
				string id = tr.Id;
				if (!string.IsNullOrWhiteSpace(id))
				{
					XmlElement matchedChildElement = Program.GetMatchedChildElement(groupElements, "trans-unit", id);
					if (matchedChildElement != null)
					{
						XmlNodeList elementsByTagName = matchedChildElement.GetElementsByTagName("target");
						if (elementsByTagName != null && elementsByTagName.Count > 0)
						{
							XmlElement xmlElement = (XmlElement)elementsByTagName.Item(0);
							Collection<SdlFilterFrameworkGroupTransUnitSegment> seg = tr.Sgd.Seg;
							Program.UpdateTargetItems(xmlElement, seg, tr.Tg.Xgm, model);
						}
					}
				}
			}
			catch (Exception exception)
			{
				throw exception;
			}
		}

		public static void UpdateFile(ref XmlDocument document, string data)
		{
			try
			{
				Collection<SdlFilterFrameworkGroup> sdlFilterFrameworkGroups = JsonConvert.DeserializeObject<Collection<SdlFilterFrameworkGroup>>(data);
				IEnumerable<XmlElement> groupElements = Program.GetGroupElements(document);
				foreach (SdlFilterFrameworkGroupTransUnit sdlFilterFrameworkGroupTransUnit in 
					from x in sdlFilterFrameworkGroups
					select x.Tr)
				{
					string id = sdlFilterFrameworkGroupTransUnit.Id;
					Program.UpdateTargetItems(Program.GetMatchedChildElement(groupElements, "trans-unit", id).GetElementsByTagName("target").Cast<XmlElement>().First<XmlElement>(), sdlFilterFrameworkGroupTransUnit.Tg.Xgm);
				}
			}
			catch (Exception exception)
			{
				throw exception;
			}
		}

		private static void UpdateTargetItems(XmlElement parent, Collection<SdlFilterFrameworkGroupTransUnitSegment> segments, Collection<SdlFilterFrameworkGroupTransUnitItem> items, FormModel model)
		{
			string str;
			string txt;
			foreach (object childNode in parent.ChildNodes)
			{
				if (childNode.GetType().Name != typeof(XmlElement).Name)
				{
					XmlText xmlText = (XmlText)childNode;
					int nodeIndex = Program.GetNodeIndex(parent.ChildNodes, (XmlNode)childNode);
					string str1 = Program.GenerateElementId(xmlText, nodeIndex, new string[] { "id", "mid" });
					SdlFilterFrameworkGroupTransUnitItem matchedMarkItem = Program.GetMatchedMarkItem(items, str1);
					SdlFilterFrameworkGroupTransUnitSegment sdlFilterFrameworkGroupTransUnitSegment = segments.FirstOrDefault<SdlFilterFrameworkGroupTransUnitSegment>((SdlFilterFrameworkGroupTransUnitSegment i) => i.Id.Trim() == str1);
					if (matchedMarkItem != null)
					{
						txt = matchedMarkItem.Txt;
					}
					else
					{
						txt = null;
					}
					if (!Program.IsAllowedToUpdate(txt, sdlFilterFrameworkGroupTransUnitSegment, model, out str))
					{
						continue;
					}
					xmlText.Value = Program.NormalizeContent(str);
				}
				else
				{
					Program.UpdateTargetItems((XmlElement)childNode, segments, items, model);
				}
			}
		}

		private static void UpdateTargetItems(XmlElement parent, Collection<SdlFilterFrameworkGroupTransUnitItem> items)
		{
			string str;
			foreach (object childNode in parent.ChildNodes)
			{
				if (childNode.GetType().Name != typeof(XmlElement).Name)
				{
					XmlText xmlText = (XmlText)childNode;
					int nodeIndex = Program.GetNodeIndex(parent.ChildNodes, (XmlNode)childNode);
					string str1 = Program.GenerateElementId(xmlText, nodeIndex, new string[] { "id", "mid" });
					SdlFilterFrameworkGroupTransUnitItem matchedMarkItem = Program.GetMatchedMarkItem(items, str1);
					if (matchedMarkItem != null)
					{
						string txt = matchedMarkItem.Txt;
						if (txt != null)
						{
							str = txt.Trim();
						}
						else
						{
							str = null;
						}
					}
					else
					{
						str = null;
					}
					string str2 = str;
					if (string.IsNullOrWhiteSpace(str2))
					{
						continue;
					}
					xmlText.Value = Program.NormalizeContent(str2);
				}
				else
				{
					Program.UpdateTargetItems((XmlElement)childNode, items);
				}
			}
		}
	}
}