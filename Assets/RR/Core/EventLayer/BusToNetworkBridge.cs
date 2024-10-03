using System;
using System.Collections.Generic;
using System.Linq;
using RR.Core.EventLayer.Attributes;
using RR.Core.Extensions;
using Debug = RR.Core.DebugSystem.RRLogger;

namespace RR.Core.EventLayer
{
	/// <summary>
	/// Network service should implement this interface to enable networked Events.
	/// </summary>
	public interface INetworkBusService
	{
		void RaiseEvent(int eventId, byte[] data);
		event Action<int, byte[]> OnEventCallback;
	}

	/// <summary>
	/// Enables sending Events via network.
	/// Needed events must be marked as [NetworkedEvent].
	/// [NetworkedEvent] Event will be called for all network players.
	/// 
	/// This class provides just bridge between Bus and network service. 
	/// To make it working you need create network service and implement INetworkBusService there.
	/// Then call method Init<Bus>(service) to enable bridge.
	/// </summary>
	public class BusToNetworkBridge
	{
		static readonly List<EventInfo> eventIdToEventInfo = new List<EventInfo>();
		static readonly HashSet<INetworkBusService> services = new HashSet<INetworkBusService>();

		/// <summary>
		/// Call this method to add Bus to networking service.
		/// After this event marked as [NetworkedEvent] will be shared via network.
		/// </summary>
		public static void Init<TBus>(INetworkBusService service)
		{
			if (!services.Contains(service))
			{
				service.OnEventCallback += Service_OnEventCallback;
				services.Add(service);
			}

			var rrEventBaseType = typeof(RREventBase);
			var networkedEventAttributeType = typeof(NetworkedEventAttribute);
			var iSerializedEventType = typeof(ISerializedEvent);

			//get all fields of type Event
			typeof(TBus)
				.GetFields()
				.Where(fi =>
					rrEventBaseType.IsAssignableFrom(fi.FieldType)
					&& iSerializedEventType.IsAssignableFrom(fi.FieldType)
					&& fi.GetCustomAttributes(networkedEventAttributeType, true).Any())
				.ForEach(fi =>
				{
					var e = (ISerializedEvent)fi.GetValue(null);
					Subscribe(e, service);
				});
		}

		private static void Service_OnEventCallback(int eventId, byte[] bytes)
		{
			if (eventId < 0 || eventId >= eventIdToEventInfo.Count)
				return;//is not my eventId

			var info = eventIdToEventInfo[eventId];
			info.IsFired = true;

            if (RREventBase.LogEvents)
            {
                if (info.Event is RREventBase e && !e.HideInLog)
                    Debug.Log($"[Received from Network] <color=blue>{e.Name}</color>");
            }

			try
			{
				info.Event.PublishSerialized(bytes);
			}
			finally
			{
				info.IsFired = false;
			}
		}

		private static void Subscribe(ISerializedEvent eBase, INetworkBusService service)
		{
			var eventId = eventIdToEventInfo.Count();
			var info = new EventInfo(eBase);

			eBase.SubscribeSerialized((bytes) =>
			{
				if (!info.IsFired)
					service.RaiseEvent(eventId, bytes);
			});

			eventIdToEventInfo.Add(info);
		}

		private class EventInfo
		{
			public ISerializedEvent Event;
			public bool IsFired;

			public EventInfo(ISerializedEvent @event)
			{
				Event = @event;
			}
		}
	}
}