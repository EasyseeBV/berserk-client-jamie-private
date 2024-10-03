using System;

namespace RR.Core.Utilities
{
	public class Promise<T> : IEquatable<Promise<T>>
	{
		public bool Equals(Promise<T> other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Equals(OnSuccess, other.OnSuccess) && Equals(OnFailed, other.OnFailed);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((Promise<T>)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return ((OnSuccess != null ? OnSuccess.GetHashCode() : 0) * 397) ^ (OnFailed != null ? OnFailed.GetHashCode() : 0);
			}
		}

		public Action<T> OnSuccess;
		public Action<Exception> OnFailed;

		public event Action OnComplete;

		public void Resolve(T data = default)
		{
			OnSuccess?.Invoke(data);
			OnComplete?.Invoke();
		}

		public void Reject(Exception ex = null)
		{
			OnFailed?.Invoke(ex);
			OnComplete?.Invoke();
		}

		public Promise(Action<T> onCompleted, Action<Exception> onFailed)
		{
			OnSuccess = onCompleted;
			OnFailed = onFailed;
		}

		public static bool operator ==(Promise<T> p1, Promise<T> p2)
		{
			return p1?.Equals(p2) ?? false;
		}

		public static bool operator !=(Promise<T> p1, Promise<T> p2)
		{
			return !(p1 == p2);
		}
	}
}