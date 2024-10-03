using System;
using RR.Core.Extensions;
using UnityEngine;

namespace RR.Core.EventLayer
{
	public class RREvent : RREventBase, ISerializedEvent
	{
		/// <summary>Subscribe w/o auto unsubscribe</summary>
		public SubscriberInfo SubscribeRaw(Action callback)
		{
			var unit = new SubscriberInfo(callback, this);
			subscribers.AddLast(unit);
			return unit;
		}

		/// <summary>Subscribes to the Event. Auto unsubscribe when Component is destroyed.</summary>
		public SubscriberInfo Subscribe(Component component, Action callback)
		{
			return SubscribeRaw(callback).JoinWith(component);
		}

		/// <summary>Subscribes to the Event. Is called if condition is True. Auto unsubscribe when Component is destroyed.</summary>
		public SubscriberInfo Subscribe(Component component, Func<bool> condition, Action callback)
		{
			return SubscribeRaw(callback).JoinWith(component).Condition(condition);
		}

		/// <summary>Unsubscribe the Callback</summary>
		public void Unsubscribe(Action callback)
		{
			base.Unsubscribe(callback);
		}

		/// <summary>Publish the Event</summary>
		public new void Publish()
		{
			base.Publish();
		}

		protected override bool TryInvoke(ISubscriberInfoInternal unit)
		{
			if (unit.Condition != null && !((Func<bool>)unit.Condition).Invoke())
				return false;

			((Action)unit.Action).Invoke();
			return true;
		}

		/// <summary>Assign bool Condition. If the condition is True, the Event will be published</summary>
		public static RREvent operator +(RREvent e, bool condition)
		{
			if (condition)
				e.Publish();
			return e;
		}

		#region ISerializedEvent

		void ISerializedEvent.SubscribeSerialized(Action<byte[]> callback)
		{
			SubscribeRaw(() => callback(null));
		}

		void ISerializedEvent.PublishSerialized(byte[] data)
		{
			base.Publish();
		}

		#endregion

		/// <summary>Stores info about subscriber</summary>
		public class SubscriberInfo : SubscriberInfoBase<SubscriberInfo>
		{
			public SubscriberInfo(Action callback, RREventBase parent) : base(callback, parent) { }

			/// <summary>Execute the Callback only if the condition is True</summary>
			public SubscriberInfo Condition(Func<bool> condition) => base.Condition(condition);
		}

		public static implicit operator Action(RREvent ev) => ev.Publish;
	}

	/// <summary>
	/// Event with one argument.
	/// Implements Publisher-Subscriber pattern.
	/// </summary>
	public class RREvent<T> : RREventBase, ISerializedEvent
	{
		protected T Data;

		#region Subscibe

		/// <summary>Subscribe w/o auto unsubscribe</summary>
		public SubscriberInfo SubscribeRaw(Action<T> callback)
		{
			var unit = new SubscriberInfo(callback, this);
			subscribers.AddLast(unit);
			return unit;
		}

		/// <summary>Subscribes to the Event. Auto unsubscribe when Component is destroyed.</summary>
		public SubscriberInfo Subscribe(Component component, Action<T> callback)
		{
			return SubscribeRaw(callback).JoinWith(component);
		}

		/// <summary>Subscribes to the Event. Auto unsubscribe when Component is destroyed.</summary>
		public SubscriberInfo Subscribe(Component component, Action callback)
		{
			return SubscribeRaw(x => callback()).JoinWith(component);
		}

		/// <summary>Subscribes to the Event. Is called if condition is True. Auto unsubscribe when Component is destroyed.</summary>
		public SubscriberInfo Subscribe(Component component, Func<T, bool> condition, Action<T> callback)
		{
			return SubscribeRaw(callback).JoinWith(component).Condition(condition);
		}

		/// <summary>Subscribes to the Event. Is called if condition is True. Auto unsubscribe when Component is destroyed.</summary>
		public SubscriberInfo Subscribe(Component component, Func<T, bool> condition, Action callback)
		{
			return SubscribeRaw((o) => callback()).JoinWith(component).Condition(condition);
		}

		/// <summary>Subscribe w/o auto unsubscribe. Firing order before others.</summary>
		public SubscriberInfo SubscribeRawFirst(Action<T> callback)
		{
			var unit = new SubscriberInfo(callback, this);
			subscribers.AddFirst(unit);
			return unit;
		}

		/// <summary>Subscribes to the Event. Firing order before others. Auto unsubscribe when Component is destroyed.</summary>
		public SubscriberInfo SubscribeFirst(Component component, Action callback)
		{
			return SubscribeRawFirst(x => callback()).JoinWith(component);
		}

		/// <summary>Subscribes to the Event. Firing order before others. Auto unsubscribe when Component is destroyed.</summary>
		public SubscriberInfo SubscribeFirst(Component component, Action<T> callback)
		{
			return SubscribeRawFirst(callback).JoinWith(component);
		}

		/// <summary>Subscribes to the Event. Firing order before others. Is called if condition is True. Auto unsubscribe when Component is destroyed.</summary>
		public SubscriberInfo SubscribeFirst(Component component, Func<T, bool> condition, Action callback)
		{
			return SubscribeRawFirst((o) => callback()).JoinWith(component).Condition(condition);
		}

		/// <summary>Subscribes to the Event. Firing order before others. Is called if condition is True. Auto unsubscribe when Component is destroyed.</summary>
		public SubscriberInfo SubscribeFirst(Component component, Func<T, bool> condition, Action<T> callback)
		{
			return SubscribeRawFirst(callback).JoinWith(component).Condition(condition);
		}
		
		#endregion
		

		/// <summary>Publish the Event</summary>
		public virtual void Publish(T data)
		{
			if (!CanPublish)
				return;

			Data = data;
			Publish();
			Data = default;
		}

		/// <summary>Join this Event with Other (OneWay)</summary>
		public void JoinWith<TM>(RREvent<TM> masterRREvent, Func<TM, T> convert)
		{
			masterRREvent.SubscribeRaw(m => Publish(convert(m)));
		}

		/// <summary>Join this Event with Other (TwoWay)</summary>
		public void JoinWith<TM>(RREvent<TM> otherRREvent, Func<TM, T> convert, Func<T, TM> backConvert)
		{
			otherRREvent.SubscribeRaw(m => Publish(convert(m)));
			SubscribeRaw(t => otherRREvent.Publish(backConvert(t)));
		}

		public virtual SubscriberInfo BindInternal(Component comp, BindDirection way, Action<T> callback)
		{
			return Subscribe(comp, way == BindDirection.OnlyPublish ? EmptyDelegate<T>.Action : callback)
				   .CallWhenInactive();
		}

		/// <summary>Publish the Event</summary>
		public static RREvent<T> operator +(RREvent<T> e, T data)
		{
			e.Publish(data);
			return e;
		}

		protected override bool TryInvoke(ISubscriberInfoInternal unit)
		{
			if (unit.Condition != null && !((Func<T, bool>)unit.Condition).Invoke(Data))
				return false;

			((Action<T>)unit.Action).Invoke(Data);
			return true;
		}

		#region ISerializedEvent

		void ISerializedEvent.SubscribeSerialized(Action<byte[]> callback)
		{
			SubscribeRaw((data) => callback(data.Serialize()));
		}

		void ISerializedEvent.PublishSerialized(byte[] bytes)
		{
			Publish(bytes.DeSerialize<T>());
		}

		#endregion

		public override string ToString()
		{
			var name = Name ?? $"Event<{typeof(T).Name}>";
			return $"{name}: {Data}";
		}

		protected override string ToLogString()
		{
			var name = Name ?? $"Event<{typeof(T).Name}>";
			return $"<color={LogLineColor}>{name}</color>: {Data}";
		}

		/// <summary>Unsubscribe the Callback</summary>
		public void Unsubscribe(Action<T> callback)
		{
			base.Unsubscribe(callback);
		}

		/// <summary>Stores info about subscriber</summary>
		public class SubscriberInfo : SubscriberInfoBase<SubscriberInfo>
		{
			public SubscriberInfo(Action<T> callback, RREventBase parent) : base(callback, parent) { }

			/// <summary>Execute the Callback only if the condition is True</summary>
			public SubscriberInfo Condition(Func<T, bool> condition) => base.Condition(condition);
		}
	}

	/// <summary>
	/// Event with two arguments.
	/// Implements Publisher-Subscriber pattern.
	/// </summary>
	public class RREvent<T1, T2> : RREventBase, ISerializedEvent
	{
		protected T1 Data1;
		protected T2 Data2;

		/// <summary>Subscribe w/o auto unsubscribe</summary>
		public SubscriberInfo SubscribeRaw(Action<T1, T2> callback)
		{
			var unit = new SubscriberInfo(callback, this);
			subscribers.AddLast(unit);
			return unit;
		}

		/// <summary>Subscribes to the Event. Auto unsubscribe when Component is destroyed.</summary>
		public SubscriberInfo Subscribe(Component component, Action<T1, T2> callback)
		{
			return SubscribeRaw(callback).JoinWith(component);
		}

		/// <summary>Subscribes to the Event. Auto unsubscribe when Component is destroyed.</summary>
		public SubscriberInfo Subscribe(Component component, Action callback)
		{
			return SubscribeRaw((_, __) => callback()).JoinWith(component);
		}

		/// <summary>Subscribes to the Event. Is called if condition is True. Auto unsubscribe when Component is destroyed.</summary>
		public SubscriberInfo Subscribe(Component component, Func<T1, T2, bool> condition, Action<T1, T2> callback)
		{
			return SubscribeRaw(callback).JoinWith(component).Condition(condition);
		}

		/// <summary>Subscribes to the Event. Is called if condition is True. Auto unsubscribe when Component is destroyed.</summary>
		public SubscriberInfo Subscribe(Component component, Func<T1, T2, bool> condition, Action callback)
		{
			return SubscribeRaw((o, O) => callback()).JoinWith(component).Condition(condition);
		}

		public virtual SubscriberInfo BindInternal(Component comp, BindDirection way, Action<T1, T2> callback)
		{
			return Subscribe(comp, way == BindDirection.OnlyPublish ? EmptyDelegate<T1, T2>.Action : callback)
				   .CallWhenInactive();
		}

		/// <summary>Unsubscribe the Callback</summary>
		public void Unsubscribe(Action<T1, T2> callback)
		{
			base.Unsubscribe(callback);
		}

		/// <summary>Publish the Event</summary>
		public virtual void Publish(T1 data1, T2 data2)
		{
			if (!CanPublish)
				return;

			Data1 = data1;
			Data2 = data2;
			Publish();
			Data1 = default;
			Data2 = default;
		}

		/// <summary>Publish the Event</summary>
		public static RREvent<T1, T2> operator +(RREvent<T1, T2> e, Tuple<T1, T2> data)
		{
			e.Publish(data.Item1, data.Item2);
			return e;
		}

		protected override bool TryInvoke(ISubscriberInfoInternal unit)
		{
			if (unit.Condition != null && !((Func<T1, T2, bool>)unit.Condition).Invoke(Data1, Data2))
				return false;

			((Action<T1, T2>)unit.Action).Invoke(Data1, Data2);
			return true;
		}

		#region ISerializedEvent

		void ISerializedEvent.SubscribeSerialized(Action<byte[]> callback)
		{
			SubscribeRaw(
				(data1, data2) =>
				{
					var arr1 = data1.Serialize();
					var arr2 = data2.Serialize();
					var arr0 = arr1.Length.Serialize();
					callback(BinarySerializationHelper.JoinArrays(arr0, arr1, arr2));
				});
		}

		void ISerializedEvent.PublishSerialized(byte[] bytes)
		{
			//unpack two arguments
			if (bytes.Length < 4)
				throw new Exception("Bad serialized array");
			var arr0 = new byte[4];
			Array.Copy(bytes, arr0, 4);
			var len1 = arr0.DeSerialize<int>();
			var arr1 = new byte[len1];
			Array.Copy(bytes, 4, arr1, 0, arr1.Length);
			var arr2 = new byte[bytes.Length - 4 - len1];
			Array.Copy(bytes, 4 + len1, arr2, 0, arr2.Length);
			var data1 = arr1.DeSerialize<T1>();
			var data2 = arr2.DeSerialize<T2>();

			//publish
			Publish(data1, data2);
		}

		#endregion

		public override string ToString()
		{
			var name = Name ?? $"Event<{typeof(T1).Name},{typeof(T2).Name}>";
			return $"{name}: {Data1}, {Data2}";
		}

		protected override string ToLogString()
		{
			var name = Name ?? $"Event<{typeof(T1).Name},{typeof(T2).Name}>";
			return $"<color={LogLineColor}>{name}</color>: {Data1}, {Data2}";
		}

		/// <summary>Stores info about subscriber</summary>
		public class SubscriberInfo : SubscriberInfoBase<SubscriberInfo>
		{
			public SubscriberInfo(Action<T1, T2> callback, RREventBase parent) : base(callback, parent) { }

			/// <summary>Execute the Callback only if the condition is True</summary>
			public SubscriberInfo Condition(Func<T1, T2, bool> condition) => base.Condition(condition);
		}
	}
}