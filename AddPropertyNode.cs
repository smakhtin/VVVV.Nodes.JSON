using System.ComponentModel.Composition;
using Newtonsoft.Json.Linq;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Nodes
{
	[PluginInfo(Name = "AddProperty", Category = "JSON", Help = "Add Property to JSON object", Tags = "json")]
	public class AddPropertyNode : IPluginEvaluate, IPartImportsSatisfiedNotification
	{
		[Input("JObject")]
		private ISpread<JObject> FJObjectIn;

		readonly Spread<IIOContainer<ISpread<ISpread<string>>>> FValueIn = new Spread<IIOContainer<ISpread<ISpread<string>>>>();

		[Config("Property Names", DefaultString = "VVVV Rocks", IsSingle = true)]
		IDiffSpread<string> FPropertyNamesIn;

		[Output("Output")]
		ISpread<JObject> FOutput;

		string[] FPropertyNames = new string[0];
		
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
					return FIOFactory.CreateIOContainer<ISpread<ISpread<string>>>(ioAttribute);
				}
			);
		}

		public void Evaluate(int spreadMax)
		{
			int propertiesCount = FPropertyNames.Length;

			for (int i = 0; i < spreadMax; i++)
			{
				var jObject = (dynamic) FJObjectIn[i] ?? new JObject();

				for (int j = 0; j < propertiesCount; j++)
				{

					var propertyName = FPropertyNames[j];
					ISpread<ISpread<string>> propertyValues = FValueIn[j].IOObject;
					int valuesCount = propertyValues[i].SliceCount;

					if (valuesCount > 1)
					{
						jObject[propertyName] = new JArray();
						jObject[propertyName] = propertyValues[i];
					}
					else
					{
						jObject[propertyName] = propertyValues[i][0];
					}
				}

				FOutput[i] = jObject;
			}
		}
	}
}