using RR.Core.DebugSystem;
using RR.Core.EventLayer.Attributes;
using RR.Core.Extensions;
using System;
using System.Linq;

namespace RR.Core.EventLayer
{
	/// <summary>
	/// Base class for any Bus unit.
	/// Usage:
	/// private class MyBus : EventBus
	/// {
	///		public static RREvent EventZero;
	///		public static RREvent<string> RREventOne;
	///		public static State<int> StateInt
	/// 
	///		static MyBus() => InitFields<MyBus>();
	/// }
	/// </summary>
	public class EventBus
	{
		/// <summary>
		/// Auto create fields inherited from EventBase
		/// </summary>
		protected static void InitFields<T>()
		{
			var thisType = typeof(T);
			thisType
				.GetFields()
				.Where(fi =>
					typeof(RREventBase).IsAssignableFrom(fi.FieldType)
					&& fi.GetValue(null) == null)
				.ForEach(fi =>
				{
					var @event = (RREventBase)Activator.CreateInstance(fi.FieldType);
					@event.Name = $"{thisType.Name}.{fi.Name}";
					@event.HideInLog = fi.GetCustomAttributes(typeof(HideInLogAttribute), false).Any();
					@event.EnableChainPublishing = fi.GetCustomAttributes(typeof(AllowChainAttribute), false).Any();
					fi.SetValue(null, @event);
				});

			RRLogger.Log($"{thisType.Name} has been instantiated.");
		}
	}

	public class EventBus<T> : EventBus where T : class
	{
		static EventBus() => InitFields<T>();
	}
}