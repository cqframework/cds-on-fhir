using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Spark.Core;

namespace GuidanceService.Store
{
	public class SimpleStore : IFhirStore, IGenerator, ISnapshotStore, IFhirIndex
	{
		public void Add(IEnumerable<Interaction> entries)
		{
			foreach (var entry in entries)
			{
				Add(entry);
			}
		}

		public void Add(Interaction entry)
		{
			switch (entry.Method)
			{
				case Bundle.HTTPVerb.PUT:
				case Bundle.HTTPVerb.POST:
					SaveResourceToXmlFile(entry.Key, entry.Resource);
				break;
				default: throw new InvalidOperationException(String.Format("Unknown interaction method %s.", entry.Method.ToString()));
			}
		}

		public void Clean()
		{
			throw new NotImplementedException();
		}

		public bool Exists(IKey key)
		{
			var fileName = GetResourceFileName(key);
			return File.Exists(fileName);
		}

		public IList<Interaction> Get(IEnumerable<string> identifiers, string sortby)
		{
			throw new NotImplementedException();
		}

		public Interaction Get(string primarykey)
		{
			throw new NotImplementedException();
		}

		public Interaction Get(IKey key)
		{
			var resource = LoadResourceFromXmlFile(key);
			if (resource != null)
			{
				return Interaction.Create(Bundle.HTTPVerb.GET, key, resource);
			}
			else
			{
				return null;
			}
		}

		public IList<string> History(DateTimeOffset? since = null)
		{
			throw new NotImplementedException();
		}

		public IList<string> History(IKey key, DateTimeOffset? since = null)
		{
			throw new NotImplementedException();
		}

		public IList<string> History(string typename, DateTimeOffset? since = null)
		{
			throw new NotImplementedException();
		}

		public IList<string> List(string typename, DateTimeOffset? since = null)
		{
			var resourceDirectory = GetResourceDirectory(typename);

			return (from f in Directory.EnumerateFiles(resourceDirectory, "*.xml") select Path.GetFileNameWithoutExtension(f)).ToList();
		}

		public void Replace(Interaction entry)
		{
			throw new NotImplementedException();
		}

		public bool CustomResourceIdAllowed(string value)
		{
			throw new NotImplementedException();
		}

		public string NextResourceId(string resource)
		{
			throw new NotImplementedException();
		}

		public string NextVersionId(string resource)
		{
			throw new NotImplementedException();
		}

		public void AddSnapshot(Snapshot snapshot)
		{
			throw new NotImplementedException();
		}

		public Snapshot GetSnapshot(string snapshotid)
		{
			throw new NotImplementedException();
		}


		public Key FindSingle(string resource, Hl7.Fhir.Rest.SearchParams searchCommand)
		{
			throw new NotImplementedException();
		}

		public void Process(Interaction interaction)
		{
			throw new NotImplementedException();
		}

		public void Process(IEnumerable<Interaction> interactions)
		{
			throw new NotImplementedException();
		}

		public SearchResults Search(string resource, Hl7.Fhir.Rest.SearchParams searchCommand)
		{
			throw new NotImplementedException();
		}

		private string GetResourceDirectory(string typeName)
		{
			return Path.Combine(HttpRuntime.BinDirectory, "Resources", typeName);
		}

		private string GetResourceFileName(IKey key)
		{
			switch (key.TypeName)
			{
				case "OperationDefinition": return Path.Combine(GetResourceDirectory(key.TypeName), String.Format("operation-{0}.xml", key.ResourceId.ToLower()));
				case "StructureDefinition": return Path.Combine(GetResourceDirectory(key.TypeName), String.Format("{0}.profile.xml", key.ResourceId.ToLower()));
				default: return Path.Combine(GetResourceDirectory(key.TypeName), Path.ChangeExtension(key.ResourceId, "xml"));
			}
		}

		private Resource LoadResourceFromXmlFile(IKey key)
		{
			// Reads the resource from an Xml file in the subdirectory specified by the type name
			var fileName = GetResourceFileName(key);
			if (File.Exists(fileName))
			{
				using (var resourceStream = File.Open(fileName, FileMode.Open))
				{
					using (var reader = XmlReader.Create(resourceStream))
					{
						return FhirParser.ParseResource(reader);
					}
				}
			}

			return null;
		}

		private void SaveResourceToXmlFile(IKey key, Resource resource)
		{
			var fileName = GetResourceFileName(key);
			using (var resourceStream = File.Create(fileName))
			{
				using (var writer = XmlWriter.Create(resourceStream))
				{
					FhirSerializer.SerializeResource(resource, writer);
				}
			}
		}
	}
}