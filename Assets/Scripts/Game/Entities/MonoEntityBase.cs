using Berserk.Shared.Data.Enums;
using RR.Core.DebugSystem;
using RR.Core.Extensions;
using RR.UI.FrameSystem;
using UI;
using UnityEngine;
using Vulcan.Data;

namespace Game.Entities
{
	public interface IMonoEntity
	{
		MonoEntityBaseView View { get; }
		RectTransform RectTransform { get; }
		DataBase DataBase { get; }
		bool IsDead { get; }
	}

	public abstract class MonoEntityBase<T> : MonoBehaviour, IMonoEntity where T : DataBase
	{
		public MonoEntityBaseView View => baseView;
		public DataBase DataBase => Data;

		public RectTransform RectTransform => (RectTransform)transform;

		public bool IsDead { get; private set; }

		public virtual Owner Owner
		{
			get => Data.Owner;
			protected set => Data.Owner = value;
		}

		public T Data;

		protected MonoEntityBaseView baseView;

		/// <summary>
		///     Base entry point for any in-game round instance
		///     Must be called once after instantiate.
		/// </summary>
		public MonoEntityBase<T> Init(T data)
		{
			Data = data;
			if (TryGetComponent(out baseView))
			{
				baseView.SetUp(data);
				baseView.Initialize();
				baseView.SetVisibleState(VisibleState.Visible);
				baseView.SetDynamicallyCreated(true);
			}
			else
			{
				RRLogger.Log($"There is no view for {GetType().Name.Orange()} [{Data.Title.Orange()}].".Yellow());
			}

			OnInit();

			return this;
		}

		protected virtual void OnInit()
		{
		}

		protected virtual void OnDeath()
		{
			IsDead = true;
		}

		protected virtual void Destroy()
		{
			Destroy(gameObject, 0.1f);
		}

		private void OnDestroy()
		{
			OnDeath();
			RRLogger.Log($"{name} destroyed!".Brown());
		}

		public override string ToString()
		{
			return Data.ToString();
		}
	}
}