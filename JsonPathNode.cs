using System;
using System.Collections.Generic;
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
		private IDiffSpread<JObject> FJObjectIn;

		[Input("JSONPath Query")] 
		private IDiffSpread<string> FQueryIn;
		
		[Input("Parse", IsBang = true, IsSingle = true)] 
		private ISpread<bool> FParseIn;
		
		[Output("Output")] 
		private ISpread<ISpread<string>> FDataOut;

		[Import]
		ILogger FLogger;

		private readonly List<List<string>> FData = new List<List<string>>(1);
		private readonly JsonPathContext FParser = new JsonPathContext { ValueSystem = new JsonNetValueSystem() };

		public void Evaluate(int spreadMax)
		{
			FDataOut.SliceCount = spreadMax;

			if(FParseIn[0] || FQueryIn.IsChanged)
			{
				FData.Clear();
				for (int i = 0; i < spreadMax; i++)
				{
					try
					{
						var values = FParser.SelectNodes(FJObjectIn[i], FQueryIn[i]).Select(node => node.Value);
						List<Object> list = values.ToList();

						FData.Add(new List<string>());

						for (int j = 0; j < values.Count(); j++)
						{
							FData[i].Add(list[j].ToString());
						}
					}
					catch (Exception ex)
					{
						//FLogger.Log(LogType.Error, "Query is wrong at slice " + i);
						
						FLogger.Log(LogType.Error, ex.ToString());
					}
				}
			}

			if(FData.Count == 0) return;

			for (int i = 0; i < spreadMax; i++)
			{
				FDataOut[i].AssignFrom(FData[i]);
			}
		}
	}
}
