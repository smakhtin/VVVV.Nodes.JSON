using System.ComponentModel.Composition;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Nodes
{
	public abstract class DynamicPinNodeBase<T> : IPluginEvaluate, IPartImportsSatisfiedNotification where T : class
	{
		protected readonly Spread<IIOContainer<T>> ValueIn = new Spread<IIOContainer<T>>();

		[Config("Property Names", DefaultString = "VVVV Rocks", IsSingle = true)]
		private IDiffSpread<string> FPropertyNamesIn;
		protected string[] FPropertyNames = new string[0];

		[Import]
		private IIOFactory FIOFactory;

		public void OnImportsSatisfied()
		{
			FPropertyNamesIn.Changed += PropertyNamesInChanged;
		}

		private void PropertyNamesInChanged(IDiffSpread<string> sender)
		{
			FPropertyNames = FPropertyNamesIn[0].Split(' ');
			int spreadCount = FPropertyNames.Length;

			ValueIn.ResizeAndDispose(0, Factory);
			ValueIn.ResizeAndDispose(spreadCount, Factory);
		}

		private IIOContainer<T> Factory(int i)
		{
			InputAttribute ioAttribute = new InputAttribute(FPropertyNames[i] + " Value");
			return FIOFactory.CreateIOContainer<T>(ioAttribute);
		}

		public abstract void Evaluate(int spreadMax);
	}
}
