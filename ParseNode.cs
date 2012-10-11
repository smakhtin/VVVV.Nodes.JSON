#region usings
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Newtonsoft.Json.Linq;
using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V2;
#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "Parse", Category = "JSON", Help = "Parse JSON using JSONPath Query", Tags = "json")]
	#endregion PluginInfo
	public class ParseNode : IPluginEvaluate
	{
		#region fields & pins
		[Input("JSON")]
		IDiffSpread<string> FJsonIn;

		[Input("Reload", IsSingle = true, IsBang = true)] 
		private ISpread<bool> FReload;

		[Output("Output")]
		ISpread<JObject> FObjectsOut;

		List<JObject> FObjects = new List<JObject>();

		[Import]
		ILogger FLogger;
		#endregion fields & pins

		public void Evaluate(int spreadMax)
		{
			FObjectsOut.SliceCount = spreadMax;

			if (FJsonIn.IsChanged || FReload[0])
			{
				FObjects.Clear();

				for (int i = 0; i < spreadMax; i++)
				{
					try
					{
						JObject jObject = JObject.Parse(FJsonIn[i]);
						if (jObject != null) FObjects.Add(jObject);
					}
					catch
					{
						FLogger.Log(LogType.Error, "Can't parse JSON at slice " + i);
					}
				}
			}

			FObjectsOut.AssignFrom(FObjects);
		}
	}
}
