/* 
 * Copyright (c) 2014, Furore (info@furore.com) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.github.com/furore-fhir/spark/master/LICENSE
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Hl7.Fhir.Model;
using Hl7.Fhir.Support;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Spark.Service;
using Spark.Config;
using System.Configuration;
using Spark.Core;

namespace Spark.App
{

    public static class Factory
    {

        public static Conformance GetSparkConformance()
        {
            Conformance conformance = ConformanceBuilder.CreateServer("CDS-Spark", Info.Version, "ONC", fhirVersion: "0.5.0");

			// Knowledge Modules
			conformance.AddSingleResourceComponent
			(
				"OperationDefinition", 
				false, 
				true, 
				Conformance.ResourceVersionPolicy.NoVersion, 
				new ResourceReference { Reference = "/fhir/StructureDefinition/knowledgemodule-cqf-cqf-knowledgemodule" }
			);

			// Guidance Operation
			conformance.AddOperation("guidance", new ResourceReference { Reference = "/fhir/OperationDefinition/Basic-guidance" });

			// GuidanceRequirements Operation
			conformance.AddOperation("guidanceRequirements", new ResourceReference { Reference = "/fhir/OperationDefinition/Basic-guidancerequirements" });

            conformance.AcceptUnknown = true;
            conformance.Experimental = true;
            conformance.Format = new string[] { "xml", "json" };
            conformance.Description = "This FHIR SERVER is a prototype implementation of the FHIR-Based Clinical Quality Framework Implementation Guide built in C# and based on the Spark FHIR Server";
            
            return conformance;
        }
       
    }


}