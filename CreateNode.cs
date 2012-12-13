using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Nodes
{
	[PluginInfo(Name = "Create", Category = "JSON", Help = "Create JSON object from values spreads", Tags = "json")]
	public class CreateNode : DynamicPinNodeBase<ISpread<String>>
	{
		[Output("Output")]
		ISpread<string> FOutput;
		
		JsonWriter FJsonWriter;
		
		public override void Evaluate(int spreadSize)
		{
			int propertiesCount = FPropertyNames.Length;
			
			StringBuilder stringBuilder = new StringBuilder();
			StringWriter stringWriter = new StringWriter(stringBuilder);
			
			FJsonWriter = new JsonTextWriter(stringWriter);
			//JsonWriter.Formatting(Formatting.Indented);
			FJsonWriter.WriteStartObject();
			
			for (int i = 0; i < propertiesCount; i++) 
			{
				FJsonWriter.WritePropertyName(FPropertyNames[i]);
				ISpread<string> values = ValueIn[i].IOObject;
				int valuesCount = values.SliceCount;
				
				if(valuesCount > 1)
				{
					FJsonWriter.WriteStartArray();
					
					for (int j = 0; j < valuesCount; j++) 
					{
						WriteCastedString(values[i]);
					}
					
					FJsonWriter.WriteEndArray();
				}
				else
				{
					WriteCastedString(values[0]);
				}
			}
			
			FJsonWriter.WriteEndObject();
			
			FOutput[0] = stringBuilder.ToString();
		}
		
		private void WriteCastedString(string input)
		{
			double result;
			if(Double.TryParse(input, out result))
			{
				FJsonWriter.WriteValue(result);
			}
			else
			{
				try
				{
					JsonConvert.DeserializeObject(input);
					FJsonWriter.WriteRawValue(input);
				}
				catch
				{
					FJsonWriter.WriteValue(input);
				}
			}
		}
	}
}