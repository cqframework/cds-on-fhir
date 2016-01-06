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
			var personId = patient.Identifier.FirstOrDefault(i => i.System == PartnersURI).Value;
			if (personId == null)
			{
				throw new ArgumentException("personId", "Could not determine person id.");
			}

			var pimId = patient.Identifier.FirstOrDefault(i => i.Assigner.Display == "Partners Healthcare").Value; // Not a great way to do this...
			if (pimId == null)
			{
				throw new ArgumentException("pimId", "Could not determine PIM ID.");
			}

			var result =
				new XElement
				(
					"Patient",
					new XElement
					(
						"PIMID",
						new XAttribute("assigningAuthorityName", "Partners Healthcare"),
						new XAttribute("root", pimId)
					),
					new XElement
					(
						"personID",
						new XAttribute("extension", personId),
						new XAttribute("root", PartnersOID)
					),
					new XElement
					(
						"demographics",
						new XElement
						(
							"personDateOfBirth", 
							new XAttribute("value", patient.BirthDate)
						),
						new XElement
						(
							"gender", 
							new XAttribute("displayName", patient.Gender.ToString()),
							new XAttribute("codeSystem", "2.16.840.1.113883.5.1"),
							new XAttribute("code", patient.Gender.ToString().Substring(0, 1))
						)
					)
				);

			return result.ToString();
		}
	}
}
