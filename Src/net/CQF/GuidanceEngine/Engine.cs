using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
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

		public Resource Evaluate(ResourceType resourceType, string id, Parameters input)
		{
			switch (resourceType)
			{
				case ResourceType.DecisionSupportServiceModule: return EvaluateModule(id, input);
				case ResourceType.DecisionSupportRule: return EvaluateRule(id, input);
				default: throw new InvalidOperationException(String.Format("Evaluation is not supported for resources of type {0}.", resourceType.ToString()));
			}
		}

		private Resource EvaluateModule(string id, Parameters input)
		{
			var patient = input.Parameter.First(p => p.Name == "patient").Resource as Patient;
			if (patient == null)
			{
				throw new ArgumentNullException("patient");
			}

			var organizationId = input.Parameter.First(p => p.Name == "organization").Value as Id;
			if (organizationId == null)
			{
				throw new ArgumentNullException("organizationId");
			}

			var patientXml = patient.ToXml();

			var parameters = new Parameters();
			parameters.Add("inputParameters", new Parameters().Add("PATIENT_XML", new FhirString(HttpUtility.HtmlAttributeEncode(patient.ToXml()))).Add("CONSUMER_ID", new FhirString(organizationId.Value)));

			switch (id.ToLower())
			{
				case "ecrs-fhir-hello-world": return CallECRSModule("HelloWorld", parameters);
				case "ecrs-fhir-cdc-immunizations": return CallECRSModule("CDCImmunizations", parameters);
			}

			return EchoEvaluate(input);
		}

		private Resource CallECRSModule(string moduleId, Parameters parameters)
		{
			var client = new FhirClient("https://phsfhir.partners.org/ECRS-on-FHIR");
			var result = client.InstanceOperation(new Uri(String.Format("DecisionSupportModule/{0}", moduleId)), "evaluate", parameters);

			var guidanceResult = result as GuidanceResponse;
			if (guidanceResult != null)
			{
				
			}

			return result;
		}

		private Resource EchoEvaluate(Parameters input)
		{
			GuidanceResponse result = null;
			var responseFileName = Path.Combine(ResourceDirectory, "GuidanceResponse", String.Format("{0}.xml", input.Id.Replace("request", "response")));
			using (var outputFile = File.Open(responseFileName, FileMode.Open))
			{
				using (var outputReader = XmlReader.Create(outputFile))
				{
					result = FhirParser.ParseResource(outputReader) as GuidanceResponse;
				}
			}

			if (result == null)
			{
				throw new InvalidOperationException("Could not parse guidance response.");
			}

			return result;
		}

		private GuidanceResponse EvaluateRule(string id, Parameters input)
		{
			throw new NotImplementedException();
		}
	}
}
