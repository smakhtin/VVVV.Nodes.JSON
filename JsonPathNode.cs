using System;
using System.ComponentModel.Composition;
using System.Linq;
using JsonPath;
using Newtonsoft.Json.Linq;
using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "JSONPath", Category = "JSON", Help = "Get values by JSONPath query", Tags = "")]
	#endregion PluginInfo
	public class JsonPathNode : IPluginEvaluate
	{
		[Input("Objects")]
		private Pin<JObject> FJObjectIn;

		[Input("JSONPath Query")]
		private ISpread<string> FQueryIn;

		[Output("Output")]
		private ISpread<ISpread<string>> FDataOut;

		[Import]
		ILogger FLogger;

		private readonly JsonPathContext FParser = new JsonPathContext { ValueSystem = new JsonNetValueSystem() };

		public void Evaluate(int spreadMax)
		{
			if (FJObjectIn[0] == null)
			{
				FDataOut.SliceCount = 0;
				return;
			}

			FDataOut.SliceCount = spreadMax;

			for (int i = 0; i < spreadMax; i++)
			{
				try
				{
					var values = FParser.SelectNodes(FJObjectIn[i], FQueryIn[i]).Select(node => node.Value);
					Spread<Object> list = values.ToSpread();
					
					int count = values.Count();
					FDataOut[i].SliceCount = count;

					for (int j = 0; j < count; j++)
					{
						FDataOut[i][j] = list[j].ToString();
					}
				}
				catch (Exception ex)
				{
					FLogger.Log(LogType.Error, ex.ToString());
				}
			}
		}
	}
}
