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
using System.Configuration;
using Spark.Core;

namespace Spark.App
{

    public static class Factory
    {

        public static Conformance GetSparkConformance()
        {
            Conformance conformance = ConformanceBuilder.CreateServer("CDS-Spark", Info.Version, "ONC", fhirVersion: "1.2.0");

            conformance.AddSingleResourceComponent("Basic", false, true, Conformance.ResourceVersionPolicy.NoVersion);
            conformance.AddSingleResourceComponent("Conformance", false, false, Conformance.ResourceVersionPolicy.NoVersion);
            conformance.AddSingleResourceComponent("DecisionSupportRule", false, true, Conformance.ResourceVersionPolicy.NoVersion);
            conformance.AddSingleResourceComponent("DecisionSupportServiceModule", false, true, Conformance.ResourceVersionPolicy.NoVersion);
            conformance.AddSingleResourceComponent("GuidanceResponse", false, true, Conformance.ResourceVersionPolicy.NoVersion);
            conformance.AddSingleResourceComponent("Library", false, true, Conformance.ResourceVersionPolicy.NoVersion);
            conformance.AddSingleResourceComponent("ModuleDefinition", false, true, Conformance.ResourceVersionPolicy.NoVersion);
            conformance.AddSingleResourceComponent("ModuleMetadata", false, true, Conformance.ResourceVersionPolicy.NoVersion);
            conformance.AddSingleResourceComponent("OperationDefinition", false, false, Conformance.ResourceVersionPolicy.NoVersion);
            conformance.AddSingleResourceComponent("OrderSet", false, true, Conformance.ResourceVersionPolicy.NoVersion);
            conformance.AddSingleResourceComponent("Parameters", false, true, Conformance.ResourceVersionPolicy.NoVersion);
            conformance.AddSingleResourceComponent("Questionnaire", false, true, Conformance.ResourceVersionPolicy.NoVersion);
            conformance.AddSingleResourceComponent("StructureDefinition", false, false, Conformance.ResourceVersionPolicy.NoVersion);
            conformance.AddSingleResourceComponent("ValueSet", false, false, Conformance.ResourceVersionPolicy.NoVersion);

            conformance.AddOperation("evaluate", new ResourceReference { Reference = "/fhir/OperationDefinition/decisionsupportrule-evaluate" });
            conformance.AddOperation("evaluate", new ResourceReference { Reference = "/fhir/OperationDefinition/decisionsupportservicemodule-evaluate" });

            conformance.AcceptUnknown = Conformance.UnknownContentCode.Both;
            conformance.Experimental = true;
            conformance.Format = new string[] { "xml", "json" };
            conformance.Description = "This FHIR SERVER is a prototype implementation of the FHIR-Based Clinical Quality Framework Implementation Guide built in C# and based on the Spark FHIR Server";
            
            return conformance;
        }
       
    }


}