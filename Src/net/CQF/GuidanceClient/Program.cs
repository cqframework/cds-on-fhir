using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Hl7.Fhir.Serialization;

namespace GuidanceClient
{
	class Program
	{
		static void Main(string[] args)
		{
			var client = new FhirClient(AppSettings.Default.FhirEndpoint);
			var inputFileName = args.Length > 0 ? args[1] : AppSettings.Default.InputFile;
			client.PreferredFormat = ResourceFormat.Json;
			Parameters inputParameters = null;
			using (var inputFile = File.Open(inputFileName, FileMode.Open))
			{
				using (var inputReader = XmlReader.Create(inputFile))
				{
					inputParameters = FhirParser.ParseResource(inputReader) as Parameters;
				}
			}

			if (inputParameters == null)
			{
				Console.WriteLine("Could not parse input parameters.");
			}
			else
			{
				//var result = client.Create<Parameters>(inputParameters);
				var result = client.WholeSystemOperation("guidance", inputParameters);
				if (result != null)
				{
					using (var outputFile = File.Create("Examples\\guidance-operation-result-example-v2.xml"))
					{
						using (var outputWriter = XmlWriter.Create(outputFile))
						{
							FhirSerializer.SerializeResource(result, outputWriter);
						}
					}
					Console.WriteLine("Operation output written to file.");
				}
				else
				{
					Console.WriteLine("Operation returned null.");
				}
			}
			Console.ReadLine();
		}
	}
}
