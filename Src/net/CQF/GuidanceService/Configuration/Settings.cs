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
using System.Configuration;
using System.Collections.Specialized;
using System.IO;
using Hl7.Fhir.Model;

namespace Spark.App
{
    public static class Settings
    {
        public static int MaxBinarySize
        {
            get
            {
                try
                {
                    int max = Convert.ToInt16(GetRequiredKey("MaxBinarySize"));
                    if (max == 0) max = Int16.MaxValue;
                    return max;
                }
                catch
                {
                    return Int16.MaxValue;
                }
            }
        }

        public static Uri Endpoint
        {
            get 
            {
                string endpoint = GetRequiredKey("FHIR_ENDPOINT");
                return new Uri(endpoint, UriKind.Absolute); 
            }
        }

		public static Uri GuidanceEndpoint
		{
			get
			{
				string endpoint = GetRequiredKey("GUIDANCE_ENDPOINT");
				return new Uri(endpoint, UriKind.Absolute);
			}
		}
       
        public static string AuthorUri
        {
            get 
            {
                return Endpoint.Host;
            }
        }

        public static string ExamplesFile
        {
            get 
            {
                string path = System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath;

                if (String.IsNullOrEmpty(path))
                {
                    path = ".";
                }
            
                return Path.Combine(path, "files", "examples.zip");
            }
        }

        private static string GetRequiredKey(string key)
        {
            string s = ConfigurationManager.AppSettings.Get(key);

            if (string.IsNullOrEmpty(s))
                throw new ArgumentException(string.Format("The configuration variable {0} is missing.", key));

            return s;
        }
    }
}