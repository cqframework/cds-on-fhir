using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;

namespace GuidanceEngine
{
	public class Engine
	{
		/// <summary>
		/// The resource directory for the simple store that is hosting this example engine.
		/// </summary>
		public string ResourceDirectory { get; set; }

		public Parameters Guidance(Parameters input)
		{
			Parameters outputParameters = null;
			//var responseStreamName = String.Format("GuidanceEngine.Examples.{0}.xml", input.Id.Replace("request", "response"));
			//using (var outputFile = GetType().Assembly.GetManifestResourceStream(responseFileName))
			var responseFileName = Path.Combine(ResourceDirectory, "Parameters", String.Format("{0}.xml", input.Id.Replace("request", "response")));
			using (var outputFile = File.Open(responseFileName, FileMode.Open))
			{
				using (var outputReader = XmlReader.Create(outputFile))
				{
					outputParameters = FhirParser.ParseResource(outputReader) as Parameters;
				}
			}

			if (outputParameters == null)
			{
				throw new InvalidOperationException("Could not parse output parameters.");
			}

			return outputParameters;
		}
	}
}
