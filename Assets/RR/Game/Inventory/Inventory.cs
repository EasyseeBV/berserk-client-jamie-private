using RR.Core;
using RR.Core.DebugSystem;
using RR.Core.Extensions;
using RR.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RR.Game.Inventory
{
	public class Inventory<T> : List<T> where T : BaseResourceData, new()
	{
		private const string DefaultSavePath = "Dynamic/Inventory.rr";
		private string currentSavePath = DefaultSavePath;

		public event Action<T> OnResourceChanged;

		public Inventory(string savePath = null) : base()
		{
			if (!string.IsNullOrEmpty(savePath))
				currentSavePath = savePath;
		}

		public Inventory(IEnumerable<T> resourceReference, string savePath = null) : base(resourceReference)
		{
			if (!string.IsNullOrEmpty(savePath))
				currentSavePath = savePath;
		}

		public static Inventory<T> LoadOrCreate(string savePath = null)
		{
			savePath ??= DefaultSavePath;
			var res = RRFile.Load<List<T>>(savePath) ?? new List<T>();
			return new Inventory<T>(res) { currentSavePath = savePath };
		}

		/// <summary>
		/// Sets Resource.Type for specified resources ids if they exist.
		/// </summary>
		public void AssignType(string type, params string[] ids)
		{
			this
				.Where(x => ids.Any(id => id.Same(x.Id)))
				.ForEach(x => x.Type = type);
		}

		public void AddResources(IEnumerable<T> resourcesDelta)
		{
			resourcesDelta.ForEach(AddResource);
			SaveChanges();
		}

		public void AddResource(T resource)
			=> AddResource(resource.Id, resource.Amount, resource.Type);

		public void AddResource(string id, double amount, string type = null)
		{
			var existingResource = this.FirstOrDefault(x => x.Id.Same(id));
			if (existingResource == null)
			{
				var res = new T { Amount = new DoubleStat(amount), Id = id, Type = type };
				Add(res);
				OnResourceChanged?.Invoke(res);
				return;
			}

			existingResource.Amount.Add(amount);
			OnResourceChanged?.Invoke(existingResource);
		}

		/// <summary>
		/// Removes existing maximum for every existing resource in the player's inventory.
		/// </summary>
		public void SetAllMaxToUnlimited()
		{
			foreach (var resource in this)
				resource.Amount.SetMax(double.MaxValue);
		}

		public void Load(string path = null)
		{
			var load = RRFile.Load<List<T>>(path ?? currentSavePath);
			Clear();

			if (load != null) AddRange(load);
			else RRLogger.Error($"Null load attempt on {path}");
		}

		/// <summary>
		/// Serializes resource data into save folder.
		/// </summary>
		public void SaveChanges()
			=> RRFile.Save(currentSavePath, this);

		/// <summary>
		/// resource.Amount.Add(-x.Amount);
		/// </summary>
		public void SpendResources(IEnumerable<T> resources)
		{
			resources.ForEach(x => AddResource(x.Id, -x.Amount, x.Type));
			SaveChanges();
		}

		/// <summary>
		/// Checks if there is enough resources before spending.
		/// </summary>
		public bool TrySpendResources(IEnumerable<T> resources)
		{
			var resourceList = resources.ToList();
			if (!HasMoreOrEqual(resourceList))
				return false;

			resourceList.ForEach(x => AddResource(x.Id, -x.Amount, x.Type));
			SaveChanges();

			return true;
		}

		/// <summary>
		/// Checks if there is enough resource before spending.
		/// </summary>
		public bool TrySpendResource(string id, double amount)
		{
			var absAmount = Math.Abs(amount);
			if (!HasMoreOrEqual(id, absAmount))
				return false;

			AddResource(id, -absAmount);

			return true;
		}

		/// <summary>
		/// Returns true in case inventory has enough of requested resource.
		/// </summary>
		public bool HasMoreOrEqual(IEnumerable<T> requestedResources)
			=> requestedResources.All(x => HasMoreOrEqual(x.Id, x.Amount));

		/// <summary>
		/// Returns true in case inventory has enough of requested resource.
		/// </summary>
		public bool HasMoreOrEqual(IEnumerable<(string Id, double Amount)> requestedResources)
			=> requestedResources.All(x => HasMoreOrEqual(x.Id, x.Amount));

		/// <summary>
		/// Returns true in case inventory has enough of requested resource.
		/// </summary>
		public bool HasMoreOrEqual(string id, double value)
		{
			var existingResource = this.FirstOrDefault(p => p.Id.Same(id));
			if (existingResource == null)
				return false;

			return existingResource.Amount >= value;
		}

		public T Find(string id)
			=> this.FirstOrDefault(x => x.Id == id);

		/// <summary>
		/// Same as <see cref="FirstOrDefault"/> but
		/// Creates new resource with requested Id in case there is no such resource in Inventory.
		/// </summary>
		public T FirstOrNew(string id)
		{
			var res = Find(id);
			if (res != null)
				return res;

			RRLogger.Warning($"Creating new resource object in case there is no resource by {id.Red()}");
			res = new T { Amount = new DoubleStat(0), Id = id };
			Add(res);
			return res;
		}

		public IEnumerable<T> Get(IEnumerable<T> requestedResources)
			=> this.Where(x => requestedResources.Any(r => r.Id.Same(x.Id)));

		public IEnumerable<T> GetByType(string type)
			=> this.Where(x => x.Type == type);

		public IEnumerable<T> GetById(params string[] ids)
			=> this.Where(x => ids.Any(id => id.Same(x.Id)));
	}
}
