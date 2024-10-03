using NUnit.Framework;
using RR.Core.EventLayer;
using RR.UI.DataBinding;
using UnityEngine;
using UnityEngine.UI;

namespace RR.UI.Tests
{
	public class DataBindingTest
	{
		// ReSharper disable once ClassNeverInstantiated.Local
		private class Bus : EventBus
		{
			public static RREvent EventZero;
			public static RREvent<string> RREventOne;
			public static RREvent<int, float> RREventTwo;
			public static State<int> StateInt;

			static Bus() => InitFields<Bus>();
		}

		[Test]
		public void TwoWayBinding()
		{
			var go = new GameObject();
			var inputField = go.AddComponent<InputField>();

			//two way
			Bus.StateInt.Value = 11;
			Bus.StateInt.Bind(inputField, "0", BindDirection.TwoWay);

			Assert.AreEqual(inputField.text, "11");

			Bus.StateInt.Value = 12;
			Assert.AreEqual(inputField.text, "12");

			inputField.text = "13";
			Assert.AreEqual(Bus.StateInt.Value, 13);

			Bus.StateInt.UnsubscribeAll();
		}

		[Test]
		public void PublishOnlyBinding()
		{
			var go = new GameObject();
			var inputField = go.AddComponent<InputField>();

			//publish only
			Bus.StateInt.Value = 11;
			Bus.StateInt.Bind(inputField, "0", BindDirection.OnlyPublish);

			Assert.AreEqual(inputField.text, "");

			Bus.StateInt.Value = 12;
			Assert.AreEqual(inputField.text, "");

			inputField.text = "13";
			Assert.AreEqual(Bus.StateInt.Value, 13);

			Bus.StateInt.UnsubscribeAll();
		}

		[Test]
		public void SimpleDataBinding()
		{
			var go = new GameObject();
			var inputField = go.AddComponent<InputField>();

			//two way
			Bus.StateInt.Value = 11;
			Bus.StateInt.Bind(inputField);

			Assert.AreEqual(inputField.text, "11");

			Bus.StateInt.Value = 12;
			Assert.AreEqual(inputField.text, "12");

			inputField.text = "13";
			Assert.AreEqual(Bus.StateInt.Value, 13);

			Bus.StateInt.UnsubscribeAll();
		}

		[Test]
		public void SubscribeOnlyBinding()
		{
			var go = new GameObject();
			var inputField = go.AddComponent<InputField>();

			//subscribe only
			Bus.StateInt.Value = 11;
			Bus.StateInt.Bind(inputField, "0", BindDirection.OnlySubscribe);

			Assert.AreEqual(inputField.text, "11");

			Bus.StateInt.Value = 12;
			Assert.AreEqual(inputField.text, "12");

			inputField.text = "13";
			Assert.AreEqual(Bus.StateInt.Value, 12);

			Bus.StateInt.UnsubscribeAll();
		}

		[Test]
		public void UnBinding()
		{
			var go = new GameObject();
			var inputField = go.AddComponent<InputField>();

			//two way
			Bus.StateInt.Value = 11;
			Bus.StateInt.Bind(inputField, "0", BindDirection.TwoWay);

			Assert.AreEqual(inputField.text, "11");

			Bus.StateInt.Unsubscribe(inputField);

			Bus.StateInt.Value = 12;
			Assert.AreEqual(inputField.text, "11");

			inputField.text = "13";
			Assert.AreEqual(Bus.StateInt.Value, 12);

			Bus.StateInt.UnsubscribeAll();
		}
	}
}