using Newtonsoft.Json.Linq;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Nodes
{
	[PluginInfo(Name = "AddProperty", Category = "JSON", Help = "Add Property to JSON object", Tags = "json")]
	public class AddPropertyNode : DynamicPinNodeBase<ISpread<ISpread<string>>>
	{
		[Input("JObject")]
		private ISpread<JObject> FJObjectIn;

		[Output("Output")]
		ISpread<JObject> FOutput;

		public override void Evaluate(int spreadMax)
		{
			int propertiesCount = FPropertyNames.Length;

			for (int i = 0; i < spreadMax; i++)
			{
				var jObject = (dynamic)FJObjectIn[i] ?? new JObject();

				for (int j = 0; j < propertiesCount; j++)
				{
					var propertyName = FPropertyNames[j];
					ISpread<ISpread<string>> propertyValues = ValueIn[j].IOObject;
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