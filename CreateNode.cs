using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Nodes
{
	[PluginInfo(Name = "Create", Category = "JSON", Help = "Create JSON object from values spreads", Tags = "json")]
	public class CreateNode : IPluginEvaluate, IPartImportsSatisfiedNotification
	{
	    readonly Spread<IIOContainer<ISpread <string>>> FValueIn = new Spread<IIOContainer<ISpread<string>>>();
		
		[Config("Property Names", DefaultString = "VVVV Rocks", IsSingle = true)]
		IDiffSpread<string> FPropertyNamesIn;
		
		[Output("Output")]
		ISpread<string> FOutput;
		
		string[] FPropertyNames = new string[0];
		JsonWriter FJsonWriter;
		
		[Import]
		IIOFactory FIOFactory;
		
		public void OnImportsSatisfied()
		{
			FPropertyNamesIn.Changed += PropertyNamesInChanged;
		}
		
		private void PropertyNamesInChanged(IDiffSpread<string> sender)
		{
			FPropertyNames = FPropertyNamesIn[0].Split(' ');
			int spreadCount = FPropertyNames.Length;
			
			FValueIn.ResizeAndDispose(
				spreadCount, 
				i =>
				{
					InputAttribute ioAttribute = new InputAttribute(FPropertyNames[i] + " Value");
					return FIOFactory.CreateIOContainer<ISpread<string>>(ioAttribute);
				}
			);
		}
		
		public void Evaluate(int spreadSize)
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
				ISpread<string> values = FValueIn[i].IOObject;
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