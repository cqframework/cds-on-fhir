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
			try
			{
				switch (resourceType)
				{
					case ResourceType.DecisionSupportServiceModule: return EvaluateModule(id, input);
					case ResourceType.DecisionSupportRule: return EvaluateRule(id, input);
					default: throw new InvalidOperationException(String.Format("Evaluation is not supported for resources of type {0}.", resourceType.ToString()));
				}
			}
			catch (Exception ex)
			{
				var outcome = new OperationOutcome();
				outcome.Issue.Add
				(
					new OperationOutcome.IssueComponent()
					{
						Code = OperationOutcome.IssueType.Exception,
						Severity = OperationOutcome.IssueSeverity.Error,
						Diagnostics = ex.Message
					}
				);

				return outcome;
			}
		}

		private Resource EvaluateModule(string id, Parameters input)
		{
			switch (id.ToLower())
			{
				case "ecrs-fhir-hello-world":
				case "ecrs-fhir-cdc-immunizations":
					var inputParameters = input.Parameter.FirstOrDefault(p => p.Name == "inputParameters")?.Resource as Parameters;
					if (inputParameters == null)
					{
						throw new ArgumentNullException("inputParameters");
					}

					var patient = inputParameters.Parameter.FirstOrDefault(p => p.Name == "patient")?.Resource as Patient;
					if (patient == null)
					{
						throw new ArgumentNullException("patient");
					}

					var organizationId = inputParameters.Parameter.FirstOrDefault(p => p.Name == "organizationId")?.Value as Id;
					if (organizationId == null)
					{
						throw new ArgumentNullException("organizationId");
					}

					var patientXml = patient.ToXml();

					var parameters = new Parameters();
					parameters.Add("inputParameters", new Parameters().Add("PATIENT_XML", new FhirString(patientXml)).Add("CONSUMER_ID", new FhirString(organizationId.Value)));

					Resource result = null;
					switch (id.ToLower())
					{
						case "ecrs-fhir-hello-world": result = CallECRSModule("HelloWorld", parameters); break;
						case "ecrs-fhir-cdc-immunizations": result = CallECRSModule("CDCImmunizations", parameters); break;
					}

					var response = result as GuidanceResponse;
					if (response != null)
					{
						response.Module = new ResourceReference();
						response.Module.Reference = String.Format("DecisionSupportServiceModule/{0}", id);
					}

					return result;
			}

			return EchoEvaluate(input);
		}

		private string HtmlAttributeEncode(string xml)
		{
			string encodedString = HttpUtility.HtmlAttributeEncode(xml);
			StringBuilder result = new StringBuilder(encodedString.Length);
			for (int index = 0; index < encodedString.Length; index++)
			{
				switch (encodedString[index])
				{
					case '\r': result.Append("&#xD;"); break;
					case '\n': result.Append("&#xA;"); break;
					case '>': result.Append("&gt;"); break;
					default : result.Append(encodedString[index]); break;
				}
			}

			return result.ToString();
		}

		private Resource CallECRSModule(string moduleId, Parameters parameters)
		{
			var client = new FhirClient("https://phsfhir.partners.org/ECRS-on-FHIR");
			client.OnBeforeRequest += Client_OnBeforeRequest;
			var result = client.InstanceOperation(new Uri(String.Format("DecisionSupportServiceModule/{0}", moduleId), UriKind.Relative), "evaluate", parameters);

			var guidanceResult = result as GuidanceResponse;
			if (guidanceResult != null)
			{
				var outputParameters = guidanceResult.Contained.FirstOrDefault(r => ('#' + r.Id) == guidanceResult?.OutputParameters?.Reference) as Parameters;
				if (outputParameters != null)
				{
					var recommendationXml = (outputParameters.Parameter.FirstOrDefault(p => p.Name == "Data").Value as FhirString).Value;
					return recommendationXml.ToGuidanceResponse(guidanceResult);
				}

				return guidanceResult;
			}

			return result;
		}

		private void Client_OnBeforeRequest(object sender, BeforeRequestEventArgs e)
		{
			// HACK: Switch the header because the ECRS server is rejecting the xml+fhir...
			e.RawRequest.ContentType = "application/xml";
			e.RawRequest.Accept = "application/xml";
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

		public Resource CDSHook(Parameters input)
		{
			var activity = input.ByName<Coding>("activity");
			if (activity == null)
			{
				throw new ArgumentNullException("activity");
			}

			if (activity.Code == "medication-prescribe")
			{
				var prefetchData = input.ResourceByName<Bundle>("preFetchData");
				Patient patient = null;
				foreach (var entry in prefetchData.Entry)
				{
					if (entry.Resource is Patient)
					{
						patient = (Patient)entry.Resource;
					}
				}

				if (patient == null)
				{
					throw new ArgumentNullException("patient");
				}

				var parameters = new Parameters();
				var inputParameters = new Parameters();
				parameters.Add("inputParameters", inputParameters);
				inputParameters.Add("patient", patient);
				inputParameters.Add("organizationId", new Id("FHIR_CONNECT_A_THON"));

				var result = EvaluateModule("ecrs-fhir-cdc-immunizations", parameters);
				var guidance = result as GuidanceResponse;
				if (guidance != null)
				{
					var outputParameters = new Parameters();
					var card = new Parameters.ParameterComponent();
					card.Name = "card";
					card.Part.Add(new Parameters.ParameterComponent() { Name = "indicator", Value = new Code("success") });
					card.Part.Add(new Parameters.ParameterComponent() { Name = "source", Value = new FhirString("CDC Immunization Guidelines") });
					outputParameters.Parameter.Add(card);
					foreach (var action in guidance.Action)
					{
						var suggestion = new Parameters.ParameterComponent();
						suggestion.Name = "suggestion";
						suggestion.Part.Add(new Parameters.ParameterComponent() { Name = "label", Value = new FhirString(action.TextEquivalent) });
						suggestion.Part.Add(new Parameters.ParameterComponent() { Name = "create", Resource = guidance.Contained.Single(r => "#" + r.Id == action.Resource.Reference) });
						card.Part.Add(suggestion);
					}
					return outputParameters;

				}
				else
				{
					return result;
				}
			}
			else
			{
				throw new NotImplementedException(String.Format("Activity {0} is not implemented", activity.Code));
			}
		}
	}
}
