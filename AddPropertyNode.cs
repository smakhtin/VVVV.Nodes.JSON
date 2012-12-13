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

			FValueIn.ResizeAndDispose(0, Factory);
			FValueIn.ResizeAndDispose(spreadCount, Factory);
		}

		private IIOContainer<ISpread<ISpread<string>>> Factory(int i)
		{
			InputAttribute ioAttribute = new InputAttribute(FPropertyNames[i] + " Value");
			return FIOFactory.CreateIOContainer<ISpread<ISpread<string>>>(ioAttribute);
		}

		public void Evaluate(int spreadMax)
		{
			int propertiesCount = FPropertyNames.Length;

			//if (!FValueIn.IsChanged && !FPropertyNamesIn.IsChanged) return;
			
			for (int i = 0; i < spreadMax; i++)
			{
				var jObject = (dynamic)FJObjectIn[i] ?? new JObject();

				for (int j = 0; j < propertiesCount; j++)
				{

					var propertyName = FPropertyNames[j];
					ISpread<ISpread<string>> propertyValues = FValueIn[j].IOObject;
					int valuesCount = propertyValues[i].SliceCount;

					var firstValue = propertyValues[i][0];
					double castedValue;

					bool isStrings = !double.TryParse(firstValue, out castedValue);

					if (valuesCount > 1)
					{
						jObject[propertyName] = new JArray();

						if (isStrings)
						{
							jObject[propertyName] = propertyValues[i];
						}
						else
						{
							double[] doublesArray = new double[valuesCount];

							for (int k = 0; k < valuesCount; k++)
							{
								double value;
								double.TryParse(propertyValues[i][k], out value);

								doublesArray[k] = value;
							}

							jObject[propertyName] = doublesArray;
						}
					}
					else
					{
						if (isStrings) jObject[propertyName] = firstValue;
						else jObject[propertyName] = castedValue;
					}
				}

				FOutput[i] = jObject;
			}
		}
	}
}