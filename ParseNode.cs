using Newtonsoft.Json.Linq;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "Parse", Category = "JSON", Help = "Parse JSON using JSONPath Query", Tags = "json")]
	#endregion PluginInfo
	public class ParseNode : IPluginEvaluate
	{
		[Input("JSON")]
		private IDiffSpread<string> FJsonIn;

		[Input("Reload", IsSingle = true, IsBang = true)] 
		private ISpread<bool> FReload;

		[Output("Output")]
		private ISpread<JObject> FObjectsOut;

		[Output("Valid")] 
		private ISpread<bool> FValidOut;

		public void Evaluate(int spreadMax)
		{
			FObjectsOut.SliceCount = spreadMax;

			if (!FJsonIn.IsChanged && !FReload[0]) return;
			
			for (int i = 0; i < spreadMax; i++)
			{
				try
				{
					FObjectsOut[i] = JObject.Parse(FJsonIn[i]);
					FValidOut[i] = true;
				}
				catch
				{
					FValidOut[i] = false;
				}
			}
		}
	}
}
