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
		public Parameters Guidance(Parameters input)
		{
			Parameters outputParameters = null;
			using (var outputFile = GetType().Assembly.GetManifestResourceStream("GuidanceEngine.Examples.guidance-operation-response-example-v2.xml"))
			//using (var outputFile = File.Open("bin\\Examples\\guidance-operation-response-example-v2.xml", FileMode.Open))
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
