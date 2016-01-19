using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hl7.Fhir.Model;

namespace GuidanceEngine
{
	public static class ParameterExtensions
	{
		public static T ResourceByName<T>(this Parameters parameters, string name) where T : Resource
		{
			var parameter = parameters.Parameter.FirstOrDefault(p => p.Name == name)?.Resource;
			if (parameter == null)
			{
				throw new ArgumentNullException(name);
			}

			return (T)parameter;
		}

		public static T ByName<T>(this Parameters parameters, string name) where T : Element
		{
			var parameter = parameters.Parameter.FirstOrDefault(p => p.Name == name)?.Value;
			if (parameter == null)
			{
				throw new ArgumentNullException(name);
			}

			return (T)parameter;
		}
	}
}
