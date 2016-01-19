using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Hl7.Fhir.Model;

namespace GuidanceEngine
{
	public static class PartnersExtensions
	{
		public const string PartnersOID = "1.3.6.1.4.1.16517";
		public const string PartnersURI = "urn:oid:1.3.6.1.4.1.16517";

		public static string ToXml(this Patient patient)
		{
			var personId = patient.Identifier.FirstOrDefault(i => i.System == PartnersURI)?.Value;
			if (personId == null)
			{
				personId = patient.Identifier.FirstOrDefault()?.Value;
			}

			if (personId == null)
			{
				throw new ArgumentException("personId", "Could not determine person id.");
			}

			var result =
				new XElement
				(
					"Patient",
					new XElement
					(
						"personID",
						new XAttribute("extension", personId),
						new XAttribute("root", PartnersOID)
					),
					new XElement
					(
						"demographics",
						!String.IsNullOrEmpty(patient.BirthDate)
							? new XElement
							(
								"personDateOfBirth", 
								new XAttribute("value", patient.BirthDate)
							)
							: null,
						patient.Gender != null
							? new XElement
							(
								"gender", 
								new XAttribute("displayName", patient.Gender.ToString()),
								new XAttribute("codeSystem", "2.16.840.1.113883.5.1"),
								new XAttribute("code", patient.Gender.ToString().Substring(0, 1))
							)
							: null
					)
				);

			return result.ToString();
		}

		public static GuidanceResponse ToGuidanceResponse(this string recommendationXml, GuidanceResponse ecrsResult)
		{
			var result = new GuidanceResponse();

			result.Status = ecrsResult.Status;

			var recommendation = XDocument.Parse(recommendationXml);

			result.Id = recommendation.Root.Element("ID").Attribute("root").Value;
			foreach (var action in recommendation.Root.Elements("Action"))
			{
				result.Action.Add(action.ToAction(result));
			}

			return result;
		}

		private static GuidanceResponse.ActionComponent ToAction(this XElement action, GuidanceResponse response)
		{
			var result = new GuidanceResponse.ActionComponent();

			// TODO: Map to "Mode" attribute of the request list...
			result.Type = GuidanceResponse.GuidanceResponseActionType.Create;

			// TODO: Handle "Operation" attribute of the request list...
			// TODO: Handle the fact that an ECRS action can contain multiple requests

			var requestList = action.Element("RequestList");

			var substanceAdministrationRequest = requestList.Element("SubstanceAdministrationRequest");
			if (substanceAdministrationRequest != null)
			{
				result.TextEquivalent = substanceAdministrationRequest.Element("textAlternative").Value;

				var immunizationRecommendation = new ImmunizationRecommendation();
				immunizationRecommendation.Id = Guid.NewGuid().ToString();
				// TODO: Set immunizationRecommendation.Patient...
				var irc = new ImmunizationRecommendation.RecommendationComponent();
				irc.DateCriterion.Add
				(
					new ImmunizationRecommendation.DateCriterionComponent() 
					{
						Value = substanceAdministrationRequest.Element("suggestedAdminDate").Element("{urn:hl7-org:v3}phase").Element("{urn:hl7-org:v3}low").Attribute("value").Value 
					}
				);

				var codedProductClass = substanceAdministrationRequest.Element("codedProductClass");
				irc.VaccineCode = new CodeableConcept("urn:oid:" + codedProductClass.Attribute("codeSystem").Value, codedProductClass.Attribute("code").Value);
				irc.DoseNumber = Int32.Parse(substanceAdministrationRequest.Element("doseNumberInSeries").Attribute("value").Value);

				// TODO: Hardstop indication...

				immunizationRecommendation.Recommendation.Add(irc);

				response.Contained.Add(immunizationRecommendation);
				result.Resource = new ResourceReference();
				result.Resource.Display = result.TextEquivalent;
				result.Resource.Reference = "#" + immunizationRecommendation.Id;
			}

			var messageRequest = requestList.Element("MessageRequest");
			if (messageRequest != null)
			{
				result.TextEquivalent = messageRequest.Element("{urn:partners-org:ecrs:actions}Message").Element("freeTextMessage").Attribute("value").Value;

				var communicationRequest = new CommunicationRequest();
				communicationRequest.Id = Guid.NewGuid().ToString();
				communicationRequest.Payload.Add(new CommunicationRequest.PayloadComponent() { Content = new FhirString(result.TextEquivalent) });

				response.Contained.Add(communicationRequest);
				result.Resource = new ResourceReference();
				result.Resource.Display = result.TextEquivalent;
				result.Resource.Reference = "#" + communicationRequest.Id;
			}

			return result;
		}
	}
}
