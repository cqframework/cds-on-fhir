using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Spark.Core;

namespace GuidanceService
{
	public static class WebApiConfig
	{
		public static void Register(HttpConfiguration config)
		{
			// Web API configuration and services

			// Web API routes
			config.MapHttpAttributeRoutes();

			//config.Routes.MapHttpRoute(
			//	name: "DefaultApi",
			//	routeTemplate: "api/{controller}/{id}",
			//	defaults: new { id = RouteParameter.Optional }
			//);

			config.EnableCors();
			config.AddFhir();
		}
	}
}
