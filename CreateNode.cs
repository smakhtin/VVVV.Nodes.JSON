using System;
using System.ComponentModel.Composition;
using System.Dynamic;
using System.IO;
using System.Text;
using System.ComponentModel;
using Newtonsoft.Json;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

namespace VVVV.Nodes
{
	[PluginInfo(Name = "Create", Category = "JSON", Help = "Create JSON object from values spreads", Tags = "json")]
	public class CreateNode : IPluginEvaluate, IPartImportsSatisfiedNotification
	{
		Spread<IIOContainer<ISpread <string>>> FValueIn = new Spread<IIOContainer<ISpread<string>>>();
		
		[Config("Property Names", DefaultString = "VVVV Rocks", IsSingle = true)]
		IDiffSpread<string> FPropertyNamesIn;
		
		[Output("Output")]
		ISpread<string> FOutput;
		
		string[] FPropertyNames = new string[0];
		JsonWriter jsonWriter;
		
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
				(i) =>
				{
					InputAttribute ioAttribute = new InputAttribute(FPropertyNames[i] + " Value");
					return FIOFactory.CreateIOContainer<ISpread<string>>(ioAttribute);
				}
			);
		}
		
		public void Evaluate(int spreadSize)
		{
			int propertiesCount = FPropertyNames.Length;
			dynamic obj = new ExpandoObject();
			
			StringBuilder stringBuilder = new StringBuilder();
			StringWriter stringWriter = new StringWriter(stringBuilder);
			
			jsonWriter = new JsonTextWriter(stringWriter);
			//JsonWriter.Formatting(Formatting.Indented);
			jsonWriter.WriteStartObject();
			
			for (int i = 0; i < propertiesCount; i++) 
			{
				jsonWriter.WritePropertyName(FPropertyNames[i]);
				ISpread<string> values = FValueIn[i].IOObject;
				int valuesCount = values.SliceCount;
				
				if(valuesCount > 1)
				{
					jsonWriter.WriteStartArray();
					
					for (int j = 0; j < valuesCount; j++) 
					{
						WriteCastedString(values[i]);
					}
					
					jsonWriter.WriteEndArray();
				}
				else
				{
					WriteCastedString(values[0]);
				}
			}
			
			jsonWriter.WriteEndObject();
			
			FOutput[0] = stringBuilder.ToString();
		}
		
		private void WriteCastedString(string input)
		{
			double result;
			if(Double.TryParse(input, out result)) jsonWriter.WriteValue(result);
			else 
			{
				object obj = JsonConvert.DeserializeObject(input);
				
				if ( obj != null) 
				{
					jsonWriter.WriteRawValue(input);
				}
				else
				{
					jsonWriter.WriteValue(input);
				}
			}
		}
	}
}