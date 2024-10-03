using NUnit.Framework;
using RR.Core;
using RR.Core.Async;
using RR.Network.CacheSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using RR.Network.Rest;
using UnityEngine;
using UnityEngine.TestTools;
using Debug = UnityEngine.Debug;

namespace RR.Network.Tests
{
	/// <summary>
	/// Don't use Cache.Clear(type); and Sockets.CloseSocket(); at the end of methods! This will break the IEnumerator cache and fetch!
	/// </summary>
	[TestOf(typeof(RRCache))]
	internal class CacheTest
	{
		private const string FakeApiGetSingleBase = "https://reqres.in/api/users";

		private static Dispatcher _dispatcher;

		private string apiUrl;

		private interface ITestItem
		{
		}

		public class FakeAPI : API<FakeAPI> { public override string BaseUrl => FakeApiGetSingleBase; }

		private class TestItemBase
		{
			public string DataProperty { get; set; }
			public string DataField;

			private string dataField;

			public string DataFieldPrivate => dataField;

			public TestItemBase() { }

			public TestItemBase(string data)
			{
				dataField = data;
			}
		}

		private class TestItem : TestItemBase, ITestItem
		{
			public string TestId = Guid.NewGuid().ToString();

			public TestItem() { }
			public TestItem(string data) : base(data) { }
		}

		private class TestModel : TestItemBase
		{
			public class DataWrap
			{
				public int Id { get; set; }
				public string Email { get; set; }
				public string FirstName { get; set; }
				public string LastName { get; set; }
				public string Avatar { get; set; }
			}

			public DataWrap Data { get; set; }
		}

		[OneTimeSetUp]
		public void PrepareTestEnvironment()
		{
			if (!_dispatcher)
				_dispatcher = new GameObject($"{nameof(CacheTest)}.{nameof(Dispatcher)}").AddComponent<Dispatcher>();
		}

		[OneTimeTearDown]
		public void TearsDown()
		{
			_dispatcher = null;
		}

		[UnityTest]
		public IEnumerator NoDoubleFetchingSameItem()
		{
			var type = Guid.NewGuid().ToString();
			var requestsFinished = 0;
			var sw = new Stopwatch();
			sw.Start();

			var p1 = false;
			var res = string.Empty;

			void OnFirstSuccess(string response)
			{
				res = response;
				requestsFinished++;
				Debug.Log($"1 {nameof(RRCache.Get)} done");

				// this check that second promise wont start fetching
				{
					Assert.IsFalse(p1);
					p1 = !p1;
					Assert.IsTrue(p1);
				}
			}

			void OnSecondSuccess(string response)
			{
				requestsFinished++;
				Debug.Log($"2 {nameof(RRCache.Get)} done");

				Assert.AreEqual(response, res);
			}

			void OnThirdSuccess(string response)
			{
				requestsFinished++;
				Debug.Log($"3 {nameof(RRCache.Get)} done");

				Assert.AreEqual(response, res);
			}

			RRCache.Get(type, OnFirstSuccess, () => FakeAPI.GetAsync<string>("2").ContinueWith(x => x.Result.Data), false);
			RRCache.Get(type, OnSecondSuccess, () => FakeAPI.GetAsync<string>("2").ContinueWith(x => x.Result.Data), false);
			RRCache.Get(type, OnThirdSuccess, () => FakeAPI.GetAsync<string>("2").ContinueWith(x => x.Result.Data), false);

			yield return new WaitUntil(() => requestsFinished > 2 || sw.ElapsedMilliseconds > 12000);
			sw.Stop();

			Assert.Throws<InvalidCacheTypeException>(() => RRCache.Get<TestItem>(type));

			var result = RRCache.Get<string>(type);
			Assert.NotNull(result);
			Assert.AreEqual(result, res);

			Assert.IsTrue(p1);

			RRCache.Clear(type);
		}

		[Test]
		public void CacheCanReturnInheritedTypes()
		{
			var type = Guid.NewGuid().ToString();
			var item = new TestItem();

			RRCache.Add(type, item);

			Assert.AreSame(item, RRCache.Get<TestItem>(type));
			Assert.AreEqual(item, RRCache.Get<TestItem>(type));

			Assert.AreSame(item, RRCache.Get<TestItemBase>(type));
			Assert.AreEqual(item, RRCache.Get<TestItemBase>(type));

			RRCache.Clear(type);

			var itemBase = new TestItemBase();
			RRCache.Add(type, itemBase);

			var result = RRCache.Get<TestItem>(type);

			Assert.NotNull(result);

			var testItem = RRCache.Get<TestItem>(type);
			Assert.AreNotSame(itemBase, testItem);
			Assert.AreNotEqual(itemBase, testItem);

			Assert.AreSame(itemBase, RRCache.Get<TestItemBase>(type));
			Assert.AreEqual(itemBase, RRCache.Get<TestItemBase>(type));

			RRCache.Clear(type);
		}

		[Test]
		public void InheritedTypesHasSameValuesFromBaseAndViceVersa()
		{
			var type = Guid.NewGuid().ToString();
			var item = new TestItem("v") { DataProperty = "test", DataField = "f" };

			RRCache.Add(type, item);

			Assert.AreEqual("test", RRCache.Get<TestItem>(type).DataProperty);
			Assert.AreEqual("test", RRCache.Get<TestItemBase>(type).DataProperty);

			Assert.AreEqual("f", RRCache.Get<TestItem>(type).DataField);
			Assert.AreEqual("f", RRCache.Get<TestItemBase>(type).DataField);

			RRCache.Clear(type);

			var itemBase = new TestItemBase("v") { DataProperty = "test", DataField = "f" };
			RRCache.Add(type, itemBase);

			var testItem = RRCache.Get<TestItem>(type);

			Assert.NotNull(testItem);

			Assert.AreEqual("test", testItem.DataProperty);
			Assert.AreEqual("test", RRCache.Get<TestItemBase>(type).DataProperty);

			Assert.AreEqual("f", testItem.DataField);
			Assert.AreEqual("f", RRCache.Get<TestItemBase>(type).DataField);

			Assert.Null(testItem.DataFieldPrivate);
			Assert.AreEqual("v", RRCache.Get<TestItemBase>(type).DataFieldPrivate);

			Assert.AreNotSame(itemBase, testItem);
			Assert.AreNotEqual(itemBase, testItem);

			RRCache.Clear(type);
		}

		[UnityTest]
		public IEnumerator ItemAsyncFetch()
		{
			var type = Guid.NewGuid().ToString();

			var yieldInstruction = RRCache.Add(type,
					() => FakeAPI.GetAsync<TestItem>("2")
						.ContinueWith(x => x.Result.Data))
				.FetchAsync()
				.ToYieldInstruction();

			yield return yieldInstruction;
			var item = yieldInstruction.Result;

			Assert.NotNull(item);

			var itemSame = RRCache.Get<TestItem>(type);
			Assert.NotNull(itemSame);

			var yieldInstruction2 = RRCache.Add(type,
					() => FakeAPI.GetAsync<TestItem>("2")
						.ContinueWith(x => x.Result.Data))
				.FetchAsync()
				.ToYieldInstruction();

			yield return yieldInstruction2;
			var item2 = yieldInstruction2.Result;

			var itemSame2 = RRCache.Get<TestItem>(type);

			Assert.AreSame(item2, itemSame2);
			Assert.AreEqual(item2, itemSame2);

			RRCache.Clear(type);
		}

		[UnityTest]
		public IEnumerator GetFetchOnAnotherThreadSync()
		{
			var type = Guid.NewGuid().ToString();

			var sw = new Stopwatch();
			sw.Start();

			TestModel item = null;
			var requestFinished = false;

			var entry = RRCache.Add(type, () => FakeAPI.GetAsync<TestModel>("2").ContinueWith(x => x.Result.Data));

			entry.Fetch(x =>
			{
				requestFinished = true;

				Assert.NotNull(x);
				item = x;

				Assert.Throws<UnityException>(() => UnityEngine.Object.DestroyImmediate(new GameObject("test")));

			}); // dont dispatch. Call on another thread.

			Assert.Null(item);
			Assert.NotNull(entry);

			yield return new WaitUntil(() => requestFinished || sw.ElapsedMilliseconds > 5000);
			sw.Stop();

			Assert.NotNull(item);
			Assert.NotNull(entry);

			var tryGetItem = RRCache.Get<TestModel>(type);
			Assert.NotNull(tryGetItem);
			Assert.AreSame(tryGetItem, item);
			Assert.AreEqual(tryGetItem, item);

			RRCache.Clear(type);
		}

		[UnityTest]
		public IEnumerator GetFetchOnMainUnityThreadSync()
		{
			var type = Guid.NewGuid().ToString();

			RRCache.SetDispatcher(new GameObject("Dispatcher").AddComponent<Dispatcher>());

			var sw = new Stopwatch();
			sw.Start();

			TestItem item = null;
			var requestFinished = false;

			var fetchingItem = RRCache.Add(type, () => FakeAPI.GetAsync<string>("2").ContinueWith(x => new TestItem(x.Result.Data)))
				.Fetch(x =>
				{
					Assert.NotNull(x);
					item = x;
					requestFinished = true;

					Assert.DoesNotThrow(() => UnityEngine.Object.DestroyImmediate(new GameObject("test")));

				}, _dispatcher); // will dispatch on main thread

			Assert.NotNull(fetchingItem);
			Assert.Null(item);

			yield return new WaitUntil(() => requestFinished || sw.ElapsedMilliseconds > 5000);

			Assert.NotNull(fetchingItem);
			Assert.NotNull(item);

			RRCache.Clear(type);
		}

		[UnityTest]
		public IEnumerator SingleAddGet()
		{
			var type = Guid.NewGuid().ToString();
			var item = new TestItem();
			var entry = RRCache.Add(type, item);

			var yieldInstruction1 = entry.FetchAsync().ToYieldInstruction();
			yield return yieldInstruction1;
			Assert.AreEqual(item, yieldInstruction1.Result);
			Assert.AreSame(item, entry.GetCachedValue());

			var gotEntry = RRCache.Get<TestItem>(type);

			Assert.AreEqual(item, gotEntry);
			Assert.AreSame(item, gotEntry);

			var yieldInstruction2 = entry.FetchAsync().ToYieldInstruction();
			yield return yieldInstruction2;
			Assert.AreSame(item, yieldInstruction2.Result);

			var @interface = RRCache.Get<ITestItem>(type);

			Assert.NotNull(@interface);
			Assert.IsInstanceOf(item.GetType(), @interface);

			RRCache.Clear(type);
		}

		[Test]
		public void CollectionAddGet()
		{
			var type = Guid.NewGuid().ToString();
			var item = new List<TestItem> { new TestItem() };
			var entry = RRCache.AddMany(type, item);

			Assert.IsNotNull(entry);
			Assert.IsNotNull(entry.GetCachedValue());

			Assert.AreEqual(item, entry.GetCachedValue());
			Assert.AreSame(item, entry.GetCachedValue());

			var gotEntry = RRCache.GetMany<TestItem>(type);

			Assert.AreEqual(item, gotEntry);
			Assert.AreSame(item, gotEntry);

			Assert.Throws<InvalidCacheTypeException>(() => RRCache.Get<TestItem>(type));
			Assert.DoesNotThrow(() => RRCache.Get<List<TestItem>>(type));

			var allowedEntry = RRCache.Get<List<TestItem>>(type);

			Assert.IsNotNull(allowedEntry);
			Assert.AreEqual(item, allowedEntry);
			Assert.AreSame(item, allowedEntry);

			var enumerable = RRCache.Get<IEnumerable<TestItem>>(type);

			Assert.IsNotNull(enumerable);
			Assert.AreEqual(item, enumerable);
			Assert.AreSame(item, enumerable);

			RRCache.Clear(type);
		}
	}
}
