#if OBSOLETE_EVENTBUS

using NUnit.Framework;
using RR.Core.EventSystem;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace RR.Core.Tests
{
	public class EventBusTest
	{
		private class TestComponent : MonoBehaviour
		{

		}

		private class TestClass2
		{
			public string GambinoTest { get; set; }
		}

		private class TestClass
		{
			public TestClass2 InnerTest { get; set; } = new TestClass2();
			public bool Test = false;
		}

		private class TestMessage3 : EventMessage { }
		private class TestMessage2 : EventMessage { }

		private class TestMessage : EventMessage
		{
			public float TestMe { get; set; } = -10;
			public bool TestMeBool { get; set; }

			public TestClass TestClass { get; set; } = new TestClass();
			public string TestString { get; set; }
		}

		public class TestGenericMessage<T> where T : class
		{
			public T Value { get; set; }
		}

		private enum TestEnum
		{
			None = 0,
			TestValue1 = 1,
			TestValue2 = 2,
		}

		[Test]
		public void ScopeInvokeOnce()
		{
			var i = 0;
			EventBus.Scope<int>("myScope235").Subscribe(x => i = x).InvokeOnce();
			EventBus.Scope<int>("myScope235").Publish(24);
			EventBus.Scope<int>("myScope235").Publish(25);

			Assert.AreEqual(24, i);
		}

		[Test]
		public void ScopeUnsubscribe()
		{
			var i = 0;

			EventBus.Scope<int>("myScope235").Subscribe(x => i = x);
			EventBus.Scope<int>("myScope235").Unsubscribe();
			EventBus.Scope<int>("myScope235").Publish(24);

			Assert.AreEqual(0, i);

			EventBus.Scope<int>("myScope235").Subscribe(x => i = x);
			EventBus.Unsubscribe("myScope235");
			EventBus.Scope<int>("myScope235").Publish(24);

			Assert.AreEqual(0, i);

			void SetMe(int x)
			{
				i = x;
			}

			EventBus.Scope<int>("myScope235").Subscribe(SetMe);
			EventBus.Unsubscribe<int>(SetMe);
			EventBus.Scope<int>("myScope235").Publish(24);

			Assert.AreEqual(0, i);

			void VoidMe()
			{
				i = 1;
			}

			EventBus.Scope<int>("myScope235").Subscribe(VoidMe);
			EventBus.Unsubscribe<double>(VoidMe);
			EventBus.Scope<int>("myScope235").Publish(24);

			Assert.AreEqual(1, i);

			EventBus.Unsubscribe<int>(VoidMe);
			EventBus.Scope<int>("myScope235").Publish(24);

			Assert.AreEqual(1, i);
		}

		[Test]
		public void IsScopeInit()
		{
			Assert.DoesNotThrow(() => EventBus.IndependentScope<bool>());
			Assert.DoesNotThrow(() => EventBus.IndependentScope<double>());
			Assert.DoesNotThrow(() => EventBus.Scope<string>());
			Assert.DoesNotThrow(() => EventBus.Scope<string>("myScope1"));
			Assert.DoesNotThrow(() => EventBus.Scope<int>("myScope2").Subscribe(x => Assert.Pass()));
			Assert.DoesNotThrow(() => EventBus.Scope<double>("myScope2").Subscribe(x => Assert.Pass()));
			Assert.DoesNotThrow(() => EventBus.Scope<TestMessage>("myScope3").Subscribe(x => Assert.Pass()));
			Assert.DoesNotThrow(() => EventBus.Scope<TestMessage2>("myScope3").Subscribe(x => Assert.Pass()));
			Assert.DoesNotThrow(() => EventBus.Scope<int>("myScope4").Where(x => x == 5).Subscribe(x => Assert.Pass()));
			Assert.DoesNotThrow(() => EventBus.Scope("myScope5").Where<int>(x => x == 5));
			Assert.DoesNotThrow(() => EventBus.Scope<int>("myScope6").Where(x => x == 5).Subscribe(x => Assert.Pass()));
		}

		[Test]
		public void ScopeConditionalStreaming()
		{
			var i = 0;

			EventBus.Scope<int>("myScope2").Where(x => x == 5).Subscribe(x => i = x);

			for (var j = 0; j < 5; j++)
			{
				EventBus.Scope<int>("myScope2").Stream(j);
				Assert.IsTrue(i == 0);
			}

			EventBus.Scope<int>("myScope2").Stream(5);
			Assert.IsTrue(i == 5);

			EventBus.Scope<int>("myScope2").Stream(1);
			Assert.IsTrue(i == 5);

			// non typed scopes
			var e = 0;
			var isIntStreamOn = false;

			EventBus.Scope("myScope3").Where(() => isIntStreamOn).Subscribe<int>(x => e++);
			EventBus.Scope("myScope3").Subscribe<double>(x => e++);

			EventBus.Scope("myScope3").Stream(5);
			Assert.AreEqual(0, e);
			isIntStreamOn = true;

			EventBus.Scope("myScope3").Stream(10D);
			Assert.AreEqual(1, e);

			EventBus.Scope("myScope3").Stream(10);
			Assert.AreEqual(2, e);

			EventBus.Scope<int>("myScope3").Where<int>(x => x != 5).Subscribe<int>(x => e++);

			EventBus.Scope("myScope3").Stream(5);
			Assert.AreEqual(3, e);

			EventBus.Scope("myScope3").Stream(15);
			Assert.AreEqual(5, e);
		}

		[Test]
		public void ScopeStreaming()
		{
			var i = 0;

			EventBus.Scope<int>("myScope").Subscribe(x =>
			{
				i = x;
			});

			EventBus.Scope<int>("myScope").Stream(1);
			Assert.IsTrue(i == 1);

			EventBus.Scope<int>("myScope2").Stream(15);
			Assert.IsTrue(i == 1);

			EventBus.Scope<int>("mYsCOpE").Stream(2534);
			Assert.IsTrue(i == 1);

			EventBus.Scope("myScope").Stream(3);
			Assert.IsTrue(i == 3);

			// error cases
			LogAssert.ignoreFailingMessages = true;

			EventBus.Scope("myScope").Stream(7.0f);
			Assert.IsTrue(i == 3);

			EventBus.Scope("myScope").Publish(new TestMessage());
			Assert.IsTrue(i == 3);

			LogAssert.ignoreFailingMessages = false;

			// unsubscribe case = after unsubscribing old scope is disposed
			EventBus.Scope("myScope").Unsubscribe();
			EventBus.Scope<int>("myScope").Stream(0); // new scope created here
			Assert.IsTrue(i == 3); // there is no subscription on new scope, so I will remain unchanged

			EventBus.Scope("myScope").Unsubscribe();
		}

		[Test]
		public void FromPropertyDeepCase()
		{
			var test1 = false;

			EventBus.FromProperty<TestMessage, TestClass2>(x => x.TestClass.InnerTest)
				.Subscribe(x =>
				{
					Assert.IsTrue(x.GetType() == typeof(TestClass2));
					test1 = true;
				});

			EventBus.Publish(new TestMessage());
			Assert.IsTrue(test1);
		}

		[Test]
		public void GenericMessage()
		{
			var test1 = false;
			var test2 = false;
			var test3 = false;

			// test if we can pass and store generic types inside message.
			EventBus.Subscribe<TestGenericMessage<TestClass>>(x =>
			{
				test1 = true;
				Assert.IsInstanceOf<TestClass>(x.Value);
				Assert.IsAssignableFrom<TestClass>(x.Value);
			});

			EventBus.Publish(new TestGenericMessage<TestClass> { Value = new TestClass() });
			Assert.IsTrue(test1);

			EventBus.UnsubscribeAll();

			// Test if we can listen to specific generic message

			EventBus.Subscribe(typeof(TestGenericMessage<TestClass>), x =>
			{
				test2 = true;
			});

			EventBus.Publish<TestGenericMessage<TestClass>>();
			Assert.IsTrue(test2);

			// Test if we can listen to any-type generic message

			EventBus.UnsubscribeAll();

			EventBus.Subscribe(typeof(TestGenericMessage<>), x => { test3 = true; });

			EventBus.Publish<TestGenericMessage<TestClass>>();
			Assert.IsTrue(test3);

			EventBus.UnsubscribeAll();
		}

		[Test]
		public void OnAfterAndBeforeInvocation()
		{
			var test0 = false;
			var test1 = false;

			EventBus.Subscribe<TestMessage>(x => test0 = true);
			EventBus.Publish<TestMessage>();

			Assert.IsTrue(test0);

			EventBus.Publish(new TestMessage().AfterReceived(() =>
			{
				Assert.IsTrue(test0);
				test0 = false;
			}));

			Assert.IsFalse(test0);

			// test 2
			EventBus.UnsubscribeAll();

			EventBus.Subscribe<TestMessage>(x => test0 = true);
			EventBus.Publish(new TestMessage().BeforeReceived(() =>
			{
				test0 = false;
				test1 = true;
			}));

			Assert.IsTrue(test0);
			Assert.IsTrue(test1);

			// test 3
			EventBus.UnsubscribeAll();

			test0 = false;
			test1 = false;

			void TestAfterAndBefore(TestMessage x)
			{
				Assert.IsFalse(test0);
				Assert.IsTrue(test1);
			}

			EventBus.Subscribe<TestMessage>(TestAfterAndBefore);
			EventBus.Publish(new TestMessage()
				.AfterReceived(() => test0 = true)
				.BeforeReceived(() => test1 = true));

			Assert.IsTrue(test0);
			Assert.IsTrue(test1);

			EventBus.UnsubscribeAll();
		}

		[Test]
		public void InvokeOnlyOnce()
		{
			var test1 = false;
			EventBus.Subscribe<TestMessage>(() => test1 = !test1)
				.InvokeOnce();

			EventBus.Publish<TestMessage>();
			Assert.IsTrue(test1);

			EventBus.Publish<TestMessage>();
			Assert.IsTrue(test1);

			EventBus.Subscribe<TestMessage>(() => test1 = !test1)
				.InvokeOnce();

			EventBus.Publish<TestMessage>();
			Assert.IsFalse(test1);
		}

		[Test]
		public void InvokeOnlyOnceWithCondition()
		{
			var test1 = false;
			EventBus
				.Where<TestMessage>(x => x.TestMeBool)
				.Subscribe<TestMessage>(() => test1 = !test1)
				.InvokeOnce();

			EventBus.Publish<TestMessage>();
			Assert.IsFalse(test1);

			// here we assume its invoked and unsubscribed
			EventBus.Publish(new TestMessage { TestMeBool = true });
			Assert.IsTrue(test1);

			EventBus.Publish(new TestMessage { TestMeBool = true });
			Assert.IsTrue(test1);

			EventBus.Publish(new TestMessage());
			Assert.IsTrue(test1);

			EventBus
				.Where<TestMessage>(x => !x.TestMeBool)
				.Subscribe<TestMessage>(() => test1 = !test1)
				.InvokeOnce();

			EventBus.Publish<TestMessage>();
			Assert.IsFalse(test1);
		}

		[Test]
		public void DisallowAnonymousType()
		{
			var test1 = false;
			var anonymous = new { a = 1, b = 2 };

			EventBus.Subscribe(anonymous.GetType(), () => test1 = true);
			EventBus.Publish(anonymous);

			Assert.IsFalse(test1);

			EventBus.UnsubscribeAll();

			EventBus.Subscribe<object>(() => test1 = true);
			EventBus.Publish(anonymous);

			Assert.IsTrue(test1);
			EventBus.UnsubscribeAll();
		}

		[Test]
		public void MultiMessagePublish()
		{
			var test1 = true;
			var test2 = true;
			var test3 = true;

			EventBus.Subscribe<TestMessage>(x => test1 = !test1);
			EventBus.Subscribe<TestMessage2>(x => test2 = !test2);
			EventBus.Subscribe<TestMessage3>(x => test3 = !test3);

			EventBus.Publish(new TestMessage(), new TestMessage2(), new TestMessage3(), new TestMessage3());

			Assert.IsFalse(test1);
			Assert.IsFalse(test2);
			Assert.IsTrue(test3);

			EventBus.UnsubscribeAll();

			var test4 = true;
			var test5 = true;
			var test6 = true;

			EventBus.Subscribe<TestMessage>(x => test4 = !test4);
			EventBus.Subscribe<TestMessage2>(x => test5 = !test5);
			EventBus.Subscribe<TestMessage3>(x => test6 = !test6).InvokeOnce();

			EventBus.Publish(new TestMessage3(), new TestMessage(), new TestMessage2(), new TestMessage3(), new TestMessage3());

			Assert.IsFalse(test4);
			Assert.IsFalse(test5);
			Assert.IsFalse(test6);

			EventBus.UnsubscribeAll();
		}

		[Test]
		public void BusOutsideGeneration()
		{
			var test1 = false;
			TestMessage2 test2 = null;

			var bus = new BusUnit();
			bus.Subscribe(typeof(TestMessage), () => test1 = true);

			EventBus.Publish<TestMessage>();
			Assert.IsTrue(test1);

			test1 = false;

			bus = new BusUnit().Where(() => true);
			bus.Subscribe(typeof(TestMessage), () => test1 = true);
			bus.Subscribe(typeof(TestMessage2), x => test2 = x as TestMessage2);

			EventBus.Publish<TestMessage>();
			Assert.IsTrue(test1);

			var tm2 = new TestMessage2();
			EventBus.Publish(tm2);

			Assert.IsNotNull(test2);
			Assert.AreEqual(tm2, test2);

			EventBus.UnsubscribeAll();

			// == Part 2 ==

			test1 = false;
			test2 = null;

			bus = new BusUnit();

			bus = bus.Where(() => false);
			bus.Subscribe(typeof(TestMessage), () => test1 = true);

			EventBus.Publish<TestMessage>();
			Assert.IsFalse(test1);

			EventBus.UnsubscribeAll();
		}

		[Test]
		public void StaticInstance()
		{
			var test1 = false;

			void TestMe(TestMessage2 test)
			{
				test1 = !test1;
			}

			EventBus.Subscribe<TestMessage2>(TestMe);
			EventBus.Publish<TestMessage2>();

			Assert.IsTrue(test1);
			EventBus.Publish<TestMessage2>();

			Assert.IsFalse(test1);
			EventBus.UnsubscribeAll();
		}

		[Test]
		public void BindToMono()
		{
			var go = new GameObject();
			var component = go.AddComponent<TestComponent>();
			var component2 = go.AddComponent<TestComponent>();

			var test1 = 0;

			EventBus.Subscribe<TestMessage>(x => ++test1).BindTo(component);

			Assert.AreEqual(0, test1);

			EventBus.Publish(new TestMessage());
			Assert.AreEqual(1, test1);

			Object.DestroyImmediate(component.gameObject);

			EventBus.Publish(new TestMessage());
			Assert.AreEqual(1, test1);

			EventBus.Subscribe<TestMessage>(x => ++test1).BindTo(component2);

			EventBus.Publish(new TestMessage());
			Assert.AreEqual(1, test1);

			// second part
			var go2 = new GameObject();
			component = go2.AddComponent<TestComponent>();
			component2 = go2.AddComponent<TestComponent>();
			test1 = 0;

			EventBus.Subscribe<TestMessage>(x => ++test1).BindTo(component);

			EventBus.Publish(new TestMessage());
			Assert.AreEqual(1, test1);

			EventBus.Subscribe<TestMessage>(x => ++test1).BindTo(component2);

			EventBus.Publish(new TestMessage());
			Assert.AreEqual(3, test1);

			Object.DestroyImmediate(component);

			EventBus.Publish(new TestMessage());
			Assert.AreEqual(4, test1);

			Object.DestroyImmediate(component2);

			EventBus.Publish(new TestMessage());
			Assert.AreEqual(4, test1);
		}

		[Test]
		public void Unsubscribe()
		{
			var test1 = false;
			var test2 = false;

			void TestSet(TestMessage test)
			{
				test1 = !test1;
			}

			EventBus.Subscribe<TestMessage>(TestSet);

			EventBus.Publish(new TestMessage());
			Assert.IsTrue(test1);

			EventBus.Unsubscribe<TestMessage>(TestSet);

			EventBus.Publish(new TestMessage());
			Assert.IsTrue(test1);

			EventBus.UnsubscribeAll();

			//part 2
			void TestSetAnonymous()
			{
				test2 = !test2;
			}

			EventBus.Subscribe<TestMessage>(TestSetAnonymous);

			EventBus.Publish(new TestMessage());
			Assert.IsTrue(test2);

			EventBus.Unsubscribe<TestMessage>(TestSetAnonymous);

			EventBus.Publish(new TestMessage());
			Assert.IsTrue(test2);

			EventBus.UnsubscribeAll();
		}

		[Test]
		public void SimplePasses()
		{
			Assert.DoesNotThrow(() => EventBus.Subscribe<TestMessage>(x => { }), "");
			Assert.DoesNotThrow(() => EventBus.Publish(new TestMessage()), "");
			Assert.DoesNotThrow(() => EventBus.Unsubscribe<TestMessage>(x => { }), "");

			var b = false;

			EventBus.Subscribe<TestMessage>(x => b = true);
			EventBus.Publish(new TestMessage());
			EventBus.UnsubscribeAll<TestMessage>();

			Assert.IsTrue(b);
		}

		[Test]
		public void WhereClause()
		{
			var whereTest = false;
			var whereTest2 = false;

			void TestMethod1(TestMessage x)
			{
				whereTest = true;
			}

			void TestMethod2(TestMessage x)
			{
				whereTest2 = true;
			}

			bool SimpleCondition(TestMessage x)
			{
				return true;
			}

			EventBus.Where<TestMessage>(SimpleCondition)
				.Subscribe(TestMethod1);

			EventBus.Where<TestMessage>(x => Mathf.Approximately(x.TestMe, 10))
				.Subscribe(TestMethod2);

			EventBus.Publish(new TestMessage());

			Assert.IsTrue(whereTest);
			Assert.IsFalse(whereTest2);

			EventBus.Publish(new TestMessage { TestMe = 10 });

			Assert.IsTrue(whereTest2);
			EventBus.UnsubscribeAll<TestMessage>();
		}

		[Test]
		public void DynamicNotThrow()
		{
			Assert.DoesNotThrow(() => EventBus.Publish(.5f));

			Assert.DoesNotThrow(() => EventBus.Where(x => x is string)
				.Subscribe(x => { }));

			Assert.DoesNotThrow(() => EventBus.Publish(123));
			Assert.DoesNotThrow(() => EventBus.Publish("test"));

			Assert.DoesNotThrow(() => EventBus.Where(x => !(x is null))
				.Subscribe(x => { }));

			Assert.DoesNotThrow(() => EventBus.Where(x => true)
				.Subscribe(x => { }));

			EventBus.UnsubscribeAll();
		}

		[Test]
		public void AdvancedUnsubscribe()
		{
			var test1 = true;
			var test2 = true;

			void SomeFloatFunc(float f)
			{
				test1 = !test1;
			}

			void SomeClassFunc(TestClass test)
			{
				test2 = !test2;
			}

			EventBus.Where<TestMessage>(x => true)
				.FromProperty(x => x.TestMe)
				.Subscribe(SomeFloatFunc);

			EventBus.Where<TestMessage>(x => true)
				.FromProperty(x => x.TestClass)
				.Subscribe(SomeClassFunc);

			Assert.IsTrue(test1);
			Assert.IsTrue(test2);

			EventBus.Publish(new TestMessage());

			Assert.IsFalse(test1);
			Assert.IsFalse(test2);

			EventBus.Unsubscribe<TestMessage, TestClass>(SomeClassFunc);

			EventBus.Publish(new TestMessage());

			Assert.IsTrue(test1);
			Assert.IsFalse(test2);
		}

		[Test]
		public void AdvancedUsage2()
		{
			var test1 = 0f;

			void TestCase(string @case)
			{
				++test1;
			}

			var go = new GameObject();
			var component = go.AddComponent<TestComponent>();

			EventBus.Where<TestMessage>(x => !string.IsNullOrEmpty(x.TestString))
				.FromProperty(x => x.TestString)
				.Subscribe(TestCase)
				.BindTo(component);

			EventBus.Publish(new TestMessage());
			Assert.AreEqual(0f, test1);

			EventBus.Publish(new TestMessage { TestString = "Value" });
			Assert.AreEqual(1, test1);

			EventBus.Publish(new TestMessage { TestString = "Value" });
			Assert.AreEqual(2, test1);

			EventBus.Publish(new TestMessage());
			Assert.AreEqual(2, test1);

			EventBus.Unsubscribe<TestMessage, string>(TestCase);

			EventBus.Publish(new TestMessage());
			Assert.AreEqual(2, test1);

			EventBus.Publish(new TestMessage { TestString = "Value" });
			Assert.AreEqual(2, test1);

			EventBus.Where<TestMessage>(x => !string.IsNullOrEmpty(x.TestString))
				.FromProperty(x => x.TestString)
				.Subscribe(TestCase)
				.BindTo(component);

			EventBus.Publish(new TestMessage { TestString = "Value" });
			Assert.AreEqual(3, test1);
		}

		[Test]
		public void AdvancedUsage()
		{
			var test1 = 0f;
			var test2 = true;
			var test3 = true;

			EventBus.Where<TestMessage>(x => true)
				.FromProperty(x => x.TestMe)
				.Subscribe(x => test1 = x);

			EventBus.Where<TestMessage>(x => true)
				.FromProperty(x => x.TestMeBool)
				.Subscribe(x => test2 = x);

			EventBus.Where<TestMessage>(x => true)
				.FromProperty(x => x.TestClass)
				.Subscribe(x => test3 = x.Test);

			Assert.IsTrue(test2);
			Assert.AreNotEqual(-10f, test1);

			EventBus.Publish(new TestMessage());

			Assert.IsFalse(test2);
			Assert.IsFalse(test3);
			Assert.AreEqual(-10f, test1);

			EventBus.UnsubscribeAll();
		}

		[Test]
		public void EnumUsage()
		{
			var test1 = TestEnum.None;
			var test2 = TestEnum.None;

			EventBus.Where<TestEnum>(x => x == TestEnum.TestValue1)
				.Subscribe(x => test1 = x);

			EventBus.Publish(TestEnum.TestValue1);

			Assert.AreEqual(test1, TestEnum.TestValue1);
			EventBus.UnsubscribeAll();

			//case 2
			test1 = TestEnum.None;
			test2 = TestEnum.None;

			EventBus.Subscribe<TestEnum>(x => test1 = x);

			EventBus.Publish(TestEnum.TestValue2);

			Assert.AreEqual(test1, TestEnum.TestValue2);

			EventBus.Subscribe<TestEnum>(x => test2 = x);
			EventBus.Publish(TestEnum.TestValue1);

			Assert.AreEqual(test1, TestEnum.TestValue1);
			Assert.AreEqual(test2, TestEnum.TestValue1);

			EventBus.UnsubscribeAll();
		}

		[Test]
		public void DynamicSimpleTypes()
		{
			var dynTest = false;
			var dynTest2 = false;
			var dynTest3 = false;
			var dynTest4 = false;

			void TestMethod1(string x)
			{
				dynTest = true;
			}

			void TestMethod2(int x)
			{
				dynTest2 = true;
			}

			void TestMethod3(dynamic x)
			{
				dynTest3 = true;
			}

			bool DynamicStringCondition(dynamic x)
			{
				return x is string && x.Equals("123");
			}

			bool DynamicStringCondition2(dynamic x)
				=> x?.a.Equals("123");

			EventBus.Where<string>(x => !string.IsNullOrEmpty(x))
				.Subscribe(TestMethod1);

			EventBus.Where<int>(x => x.Equals(123))
				.Subscribe(TestMethod2);

			EventBus.Where(DynamicStringCondition)
				.Subscribe(TestMethod3);

			EventBus.Publish(123);
			EventBus.Publish("123");

			Assert.IsTrue(dynTest);
			Assert.IsTrue(dynTest2);
			Assert.IsFalse(dynTest3);

			EventBus.UnsubscribeAll();

			EventBus.Where(DynamicStringCondition2)
				.Subscribe(TestMethod3);

			EventBus.Publish<object>(new { a = 10 });
			Assert.IsFalse(dynTest3);

			EventBus.Publish(new { a = "15" });
			Assert.IsFalse(dynTest3);

			EventBus.Publish(new { a = "123" });
			Assert.IsTrue(dynTest3);

			EventBus.Where(DynamicStringCondition2)
				.Subscribe(x => dynTest4 = true);

			EventBus.Publish<object>(new { a = "123" });
			Assert.IsTrue(dynTest4);

			EventBus.UnsubscribeAll();
		}
	}
}

#endif