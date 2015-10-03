using Spark.App;
using Spark.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using GuidanceService.Store;

namespace Spark.App
{
    public static class Infra
    {
        static Infra()
        {
            Infra.Simple = Infrastructure.Default().AddLocalhost(Settings.GuidanceEndpoint).AddSimpleStore();
        }

        // Use as: FhirService service = Infra.Simple.CreateService()
        public static Infrastructure Simple;
    }
}