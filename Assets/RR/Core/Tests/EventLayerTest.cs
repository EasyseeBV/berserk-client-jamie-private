using NUnit.Framework;
using System;
using RR.Core.EventLayer;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace RR.Core.Tests
{
	public class EventBusTest
	{
		// ReSharper disable once ClassNeverInstantiated.Local
		private class Bus : EventBus
		{
			public static RREvent EventZero;
			public static RREvent<string> RREventOne;
			public static RREvent<int, float> RREventTwo;
			public static State<int> StateInt;
			public static State<bool, int> StateDouble;

			static Bus() => InitFields<Bus>();
		}

		// ReSharper disable once ClassNeverInstantiated.Local
		private class TestComponent : MonoBehaviour
		{
		}

		[Test]
		public void StateWithTwoParams()
		{
			var test = (false, 0);
			var testInnerValues = false;
			var prevValueWillBe = (true, 150);

			Bus.StateDouble.Value = test;
			Bus.StateDouble.SubscribeRaw((i, y) =>
			{
				test = (!test.Item1, test.Item2 + 1);

				if (testInnerValues)
				{
					Assert.AreEqual(prevValueWillBe, Bus.StateDouble.PrevValue);
					testInnerValues = false;
				}
			});

			Bus.StateDouble.Value = prevValueWillBe;
			Assert.AreEqual(test, (true, 1));

			testInnerValues = true;
			Bus.StateDouble.Value = (false, 100);
			Assert.AreEqual(test, (false, 2));

			// test that prev one has reset to default
			Assert.AreEqual(Bus.StateDouble.PrevValue, (false, 0));

			Bus.StateDouble.Publish(false, 100);
			Assert.AreEqual(test, (true, 3));

			Bus.StateDouble.Assign(false, 100);
			Assert.AreEqual(test, (true, 3));

			Bus.StateDouble.UnsubscribeAll();
			Bus.StateDouble.Value = test;
		}

		[Test]
		public void SubscribeRaw()
		{
			var test1 = false;
			var test2 = false;
			var test3 = false;
			var test4 = false;

			Bus.EventZero.SubscribeRaw(() => test1 = true);
			Bus.EventZero.Publish();

			Assert.IsTrue(test1);

			Bus.RREventOne.SubscribeRaw((s) => { test2 = true; Assert.AreEqual(s, "hello"); });
			Bus.RREventOne.Publish("hello");

			Assert.IsTrue(test2);

			Bus.RREventTwo.SubscribeRaw((i, f) => { test3 = true; Assert.AreEqual(i, 12); Assert.AreEqual(f, 0.2f); });
			Bus.RREventTwo.Publish(12, 0.2f);

			Assert.IsTrue(test3);

			Bus.StateInt.SubscribeRaw((i) => { test4 = true; Assert.AreEqual(i, 12); });
			Bus.StateInt.Publish(12);

			Assert.AreEqual(Bus.StateInt.Value, 12);
			Assert.IsTrue(test4);

			Bus.EventZero.UnsubscribeAll();
			Bus.RREventOne.UnsubscribeAll();
			Bus.RREventTwo.UnsubscribeAll();
			Bus.StateInt.UnsubscribeAll();
			Bus.StateInt.Value = 0;
		}

		[Test]
		public void SimplePublish()
		{
			var test1 = false;
			var test2 = false;
			var test3 = false;
			var test4 = false;

			Bus.EventZero.SubscribeRaw(() => test1 = true);
			Bus.EventZero += true;

			Assert.IsTrue(test1);

			Bus.RREventOne.SubscribeRaw((s) => { test2 = true; Assert.AreEqual(s, "hello"); });
			Bus.RREventOne += "hello";

			Assert.IsTrue(test2);

			Bus.RREventTwo.SubscribeRaw((i, f) => { test3 = true; Assert.AreEqual(i, 12); Assert.AreEqual(f, 0.2f); });
			Bus.RREventTwo += new Tuple<int, float>(12, 0.2f);

			Assert.IsTrue(test3);

			Bus.StateInt.SubscribeRaw((i) => { test4 = true; Assert.AreEqual(i, 12); });
			Bus.StateInt += 12;

			Assert.AreEqual(Bus.StateInt.Value, 12);
			Assert.IsTrue(test4);

			Bus.EventZero.UnsubscribeAll();
			Bus.RREventOne.UnsubscribeAll();
			Bus.RREventTwo.UnsubscribeAll();
			Bus.StateInt.UnsubscribeAll();
			Bus.StateInt.Value = 0;
		}

		[Test]
		public void SubscribeRawCondition()
		{
			var test1 = false;
			Bus.RREventOne.SubscribeRaw((i) => test1 = true).Condition(i => i == "12");
			Bus.RREventOne.Publish("13");

			Assert.IsFalse(test1);

			Bus.RREventOne.Publish("12");

			Assert.IsTrue(test1);

			Bus.RREventOne.UnsubscribeAll();
		}

		[Test]
		public void SubscribeCondition()
		{
			var go = new GameObject();
			var component = go.AddComponent<TestComponent>();

			var test1 = false;
			Bus.RREventOne.Subscribe(component, i => i == "12", (i) => test1 = true);
			Bus.RREventOne.Publish("13");

			Assert.IsFalse(test1);

			Bus.RREventOne.Publish("12");

			Assert.IsTrue(test1);

			Bus.RREventOne.UnsubscribeAll();
		}

		[Test]
		public void InvokeOnlyOnce()
		{
			var test1 = false;
			Bus.EventZero.SubscribeRaw(() => test1 = !test1).InvokeOnce();

			Bus.EventZero.Publish();
			Assert.IsTrue(test1);

			Bus.EventZero.Publish();
			Assert.IsTrue(test1);

			Bus.EventZero.SubscribeRaw(() => test1 = !test1).InvokeOnce();

			Bus.EventZero.Publish();
			Assert.IsFalse(test1);

			Bus.EventZero.UnsubscribeAll();
		}

		[Test]
		public void JoinToComponent()
		{
			var go = new GameObject();
			var component = go.AddComponent<TestComponent>();
			var component2 = go.AddComponent<TestComponent>();

			var test1 = 0;

			Bus.EventZero.Subscribe(component, () => ++test1);

			Assert.AreEqual(0, test1);

			Bus.EventZero.Publish();
			Assert.AreEqual(1, test1);

			Object.DestroyImmediate(component.gameObject);

			Bus.EventZero.Publish();
			Assert.AreEqual(1, test1);

			Bus.EventZero.Subscribe(component2, () => ++test1);

			Bus.EventZero.Publish();
			Assert.AreEqual(1, test1);

			Bus.EventZero.UnsubscribeAll();

			// second part
			var go2 = new GameObject();
			component = go2.AddComponent<TestComponent>();
			component2 = go2.AddComponent<TestComponent>();
			test1 = 0;

			Bus.EventZero.Subscribe(component, () => ++test1);

			Bus.EventZero.Publish();
			Assert.AreEqual(1, test1);

			Bus.EventZero.Subscribe(component2, () => ++test1);

			Bus.EventZero.Publish();
			Assert.AreEqual(3, test1);

			Object.DestroyImmediate(component);

			Bus.EventZero.Publish();
			Assert.AreEqual(4, test1);

			Object.DestroyImmediate(component2);

			Bus.EventZero.Publish();
			Assert.AreEqual(4, test1);

			Bus.EventZero.UnsubscribeAll();
		}

		[Test]
		public void Unsubscribe()
		{
			var test1 = false;

			void TestSet()
			{
				test1 = !test1;
			}

			Bus.EventZero.SubscribeRaw(TestSet);

			Bus.EventZero.Publish();
			Assert.IsTrue(test1);

			Bus.EventZero.Unsubscribe(TestSet);

			Bus.EventZero.Publish();
			Assert.IsTrue(test1);

			Bus.EventZero.UnsubscribeAll();
		}

		[Test]
		public void OnUnsubscribe()
		{
			var test1 = false;

			void Test()
			{
			}

			Bus.EventZero.SubscribeRaw(Test).OnUnsubscribed(() => test1 = true);
			Assert.IsFalse(test1);

			Bus.EventZero.Unsubscribe(Test);
			Assert.IsTrue(test1);

			Bus.EventZero.UnsubscribeAll();
		}

		[Test]
		public void SerializedEvents()
		{
			var str = "";

			var se = Bus.RREventOne as ISerializedEvent;
			se.SubscribeSerialized(bytes => str = bytes.DeSerialize<string>());
			se.PublishSerialized("12".Serialize());

			Assert.AreEqual(str, "12");

			Bus.RREventOne.UnsubscribeAll();
		}

		[Test]
		public void CircularEvents()
		{
			int test = 0;

			Bus.EventZero.SubscribeRaw(() => { test++; Bus.EventZero.Publish(); });
			Bus.EventZero.Publish();

			Assert.AreEqual(test, 1);

			Bus.EventZero.UnsubscribeAll();
		}

		[Test]
		public void CallbackWithException()
		{
			LogAssert.ignoreFailingMessages = true;

			var test = 0;

			Bus.EventZero.SubscribeRaw(() => { test++; throw new Exception(); });
			Bus.EventZero.SubscribeRaw(() => { test++; });
			Bus.EventZero.Publish();

			Assert.AreEqual(test, 2);

			Bus.EventZero.UnsubscribeAll();

			LogAssert.ignoreFailingMessages = false;
		}

		[Test]
		public void StateForcedAndSilent()
		{
			int test = 0;

			Bus.StateInt.Value = 0;

			Bus.StateInt.SubscribeRaw((i) => test++);

			//
			Bus.StateInt.Value = 1;
			Assert.AreEqual(test, 1);

			Bus.StateInt.Value = 1;
			Assert.AreEqual(test, 2);

			Bus.StateInt.Value = 2;
			Assert.AreEqual(test, 3);

			Bus.StateInt.Publish(2);
			Assert.AreEqual(test, 4);

			Bus.StateInt.Assign(3);
			Assert.AreEqual(test, 4);

			Bus.StateInt.UnsubscribeAll();
			Bus.StateInt.Value = 0;
		}

		[Test]
		public void ScopeBusTest()
		{
			int test = 0;

			Scope.Event<int>("MyEvent1").SubscribeRaw((i) => test++);
			Scope.Event<int>("MyEvent2").SubscribeRaw((i) => test++);
			Scope.Event<string>("MyEvent1").SubscribeRaw((i) => test++);

			Scope.Event<int>("MyEvent1").Publish(0);
			Assert.AreEqual(test, 1);

			Scope.Event<int>("MyEvent2").Publish(0);
			Assert.AreEqual(test, 2);

			Scope.Event<int>("MyEvent1").UnsubscribeAll();
			Scope.Event<int>("MyEvent1").Publish(0);

			Assert.AreEqual(test, 2);

			Scope.UnsubscribeAndRemoveAll();
		}
	}
}