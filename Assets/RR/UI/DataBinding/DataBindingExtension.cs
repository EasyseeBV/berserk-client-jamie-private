using System;
using RR.Core.EventLayer;
using RR.Core.Extensions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RR.UI.DataBinding
{
	/// <summary>Implements data binding of UI components and Bus Events</summary>
	public static class DataBindingExtension
	{
		#region TwoWay string

		public static RREvent<string>.SubscriberInfo Bind(this RREvent<string> state, InputField text, BindDirection way = BindDirection.TwoWay)
		{
			var res = state.BindInternal(text, way, v => text.SetTextWithoutNotify(v));
			if (way != BindDirection.OnlySubscribe)
			{
				var a = new UnityAction<string>(v => state.Publish(v));
				text.onValueChanged.AddListener(a);
				res.OnUnsubscribed(() => text.onValueChanged.RemoveListener(a));
			}
			return res;
		}

		#endregion

		#region TwoWay float

		public static RREvent<float>.SubscriberInfo Bind(this RREvent<float> state, InputField text, string format = "0.00", BindDirection way = BindDirection.TwoWay)
		{
			var res = state.BindInternal(text, way, v => text.SetTextWithoutNotify(v.ToString(format)));
			if (way != BindDirection.OnlySubscribe)
			{
				var a = new UnityAction<string>(v =>
				{
					if (float.TryParse(v, out float val))
						state.Publish(val);
				});
				text.onValueChanged.AddListener(a);
				res.OnUnsubscribed(() => text.onValueChanged.RemoveListener(a));
			}
			return res;
		}

		public static RREvent<float>.SubscriberInfo Bind(this RREvent<float> state, Slider slider, BindDirection way = BindDirection.TwoWay)
		{
			var res = state.BindInternal(slider, way, v => slider.SetValueWithoutNotify(v));
			if (way != BindDirection.OnlySubscribe)
			{
				var a = new UnityAction<float>(v => state.Publish(v));
				slider.onValueChanged.AddListener(a);
				res.OnUnsubscribed(() => slider.onValueChanged.RemoveListener(a));
			}
			return res;
		}

		#endregion

		#region TwoWay int

		public static RREvent<int>.SubscriberInfo Bind(this RREvent<int> state, InputField text, string format = "0", BindDirection way = BindDirection.TwoWay)
		{
			var res = state.BindInternal(text, way, v => text.SetTextWithoutNotify(v.ToString(format)));
			if (way != BindDirection.OnlySubscribe)
			{
				var a = new UnityAction<string>(v =>
				{
					if (int.TryParse(v, out int val))
						state.Publish(val);
				});
				text.onValueChanged.AddListener(a);
				res.OnUnsubscribed(() => text.onValueChanged.RemoveListener(a));
			}
			return res;
		}

		public static RREvent<int>.SubscriberInfo Bind(this RREvent<int> state, Slider slider, BindDirection way = BindDirection.TwoWay)
		{
			var res = state.BindInternal(slider, way, v => slider.SetValueWithoutNotify(v));
			if (way != BindDirection.OnlySubscribe)
			{
				var a = new UnityAction<float>(v => state.Publish((int)v));
				slider.onValueChanged.AddListener(a);
				res.OnUnsubscribed(() => slider.onValueChanged.RemoveListener(a));
			}
			return res;
		}

		public static RREvent<int>.SubscriberInfo Bind(this RREvent<int> state, Dropdown dropdown, BindDirection way = BindDirection.TwoWay)
		{
			var res = state.BindInternal(dropdown, way, v => dropdown.SetValueWithoutNotify(v));
			if (way != BindDirection.OnlySubscribe)
			{
				var a = new UnityAction<int>(v => state.Publish(v));
				dropdown.onValueChanged.AddListener(a);
				res.OnUnsubscribed(() => dropdown.onValueChanged.RemoveListener(a));
			}
			return res;
		}

		public static RREvent<int>.SubscriberInfo Bind(this RREvent<int> state, Toggle toggle, BindDirection way = BindDirection.TwoWay)
		{
			var res = state.BindInternal(toggle, way, v => toggle.SetIsOnWithoutNotify(v != 0));

			if (way == BindDirection.OnlySubscribe)
				return res;

			var a = new UnityAction<bool>(v => state.Publish(v ? 1 : 0));
			toggle.onValueChanged.AddListener(a);
			res.OnUnsubscribed(() => toggle.onValueChanged.RemoveListener(a));

			return res;
		}

		#endregion

		#region TwoWay Enum

		/// <summary>Binding to Enum</summary>
		public static RREvent<T>.SubscriberInfo Bind<T>(this RREvent<T> state, Dropdown dropdown, BindDirection way = BindDirection.TwoWay) where T : struct, IConvertible
		{
			if (!typeof(T).IsEnum)
			{
				throw new ArgumentException("T must be an enumerated type");
			}

			var res = state.BindInternal(dropdown, way, v => dropdown.SetValueWithoutNotify(Convert.ToInt32(v)));
			if (way != BindDirection.OnlySubscribe)
			{
				var a = new UnityAction<int>(v => state.Publish((T)(object)v));
				dropdown.onValueChanged.AddListener(a);
				res.OnUnsubscribed(() => dropdown.onValueChanged.RemoveListener(a));
			}
			return res;
		}

		#endregion

		#region TwoWay bool

		public static RREvent<bool>.SubscriberInfo Bind(this RREvent<bool> state, Toggle toggle, BindDirection way = BindDirection.TwoWay)
		{
			var res = state.BindInternal(toggle, way, v => toggle.SetIsOnWithoutNotify(v));

			if (way == BindDirection.OnlySubscribe)
				return res;

			var a = new UnityAction<bool>(state.Publish);
			toggle.onValueChanged.AddListener(a);
			res.OnUnsubscribed(() => toggle.onValueChanged.RemoveListener(a));

			return res;
		}

		public static RREvent<bool, T>.SubscriberInfo Bind<T>(this RREvent<bool, T> state, Toggle toggle, T data, BindDirection way = BindDirection.TwoWay)
		{
			var res = state.BindInternal(toggle, way, (v, t) => toggle.SetIsOnWithoutNotify(v));

			if (way == BindDirection.OnlySubscribe)
				return res;

			var a = new UnityAction<bool>(x => state.Publish(x, data));
			toggle.onValueChanged.AddListener(a);
			res.OnUnsubscribed(() => toggle.onValueChanged.RemoveListener(a));

			return res;
		}

		#endregion

		#region OnClick

		public static RREvent.SubscriberInfo Bind(this RREvent state, Button button)
		{
			var a = new UnityAction(state.Publish);
			button.onClick.AddListener(a);
			return state.Subscribe(button, EmptyDelegate.Action).OnUnsubscribed(() => button.onClick.RemoveListener(a));
		}

		/// <summary>Click on button will publish Value</summary>
		public static RREvent<T>.SubscriberInfo Bind<T>(this RREvent<T> state, Button button, T value)
		{
			var a = new UnityAction(() => state.Publish(value));
			button.onClick.AddListener(a);
			return state.Subscribe(button, EmptyDelegate<T>.Action).OnUnsubscribed(() => button.onClick.RemoveListener(a));
		}

		/// <summary>Click on button will publish Value. If value published, selected color will be assigned to button.</summary>
		public static RREvent<T>.SubscriberInfo Bind<T>(this RREvent<T> state, Button button, T value, Color selectedColor)
		{
			Image image = button.GetComponent<Image>();
			if (!image)
				throw new Exception("Button must contain Image");

			var unselectedColor = image.color;
			state.BindInternal(image, BindDirection.OnlySubscribe, v => image.color = Equals(v, value) ? selectedColor : unselectedColor);

			return Bind(state, button, value);
		}

		/// <summary>Click on button will publish Value. If value published, selected sprite will be assigned to button.</summary>
		public static RREvent<T>.SubscriberInfo Bind<T>(this RREvent<T> state, Button button, T value, Sprite selectedSprite)
		{
			Image image = button.GetComponent<Image>();
			if (!image)
				throw new Exception("Button must contain Image");

			var unselectedSprite = image.sprite;
			state.BindInternal(image, BindDirection.OnlySubscribe, v => image.sprite = Equals(v, value) ? selectedSprite : unselectedSprite);

			return Bind(state, button, value);
		}

		#endregion

		#region OneWay

		public static RREvent<string>.SubscriberInfo Bind(this RREvent<string> state, Text text)
		{
			return state.BindInternal(text, BindDirection.OnlySubscribe, v => text.text = v);
		}

		public static RREvent<int>.SubscriberInfo Bind(this RREvent<int> state, Text text, string format = "0")
		{
			return state.BindInternal(text, BindDirection.OnlySubscribe, v => text.text = v.ToString(format));
		}

		public static RREvent<float>.SubscriberInfo Bind(this RREvent<float> state, Text text, string format = "0.00")
		{
			return state.BindInternal(text, BindDirection.OnlySubscribe, v => text.text = v.ToString(format));
		}

        public static RREvent<T>.SubscriberInfo Bind<T>(this RREvent<T> state, Text text, Func<T, string> converter)
        {
            return state.BindInternal(text, BindDirection.OnlySubscribe, v => text.text = converter(v));
        }

        #endregion

        #region OneWay Interactable, Active

        public static RREvent<T>.SubscriberInfo BindInteractable<T>(this RREvent<T> state, Selectable selectable, Func<T, bool> converter)
		{
			return state.BindInternal(selectable, BindDirection.OnlySubscribe, v => selectable.interactable = converter(v));
		}

		public static RREvent<bool>.SubscriberInfo BindInteractable(this RREvent<bool> state, Selectable selectable)
		{
			return state.BindInternal(selectable, BindDirection.OnlySubscribe, v => selectable.interactable = v);
		}

		public static RREvent<T>.SubscriberInfo BindActive<T>(this RREvent<T> state, GameObject gameObject, Func<T, bool> converter)
		{
			return state.BindInternal(gameObject.transform, BindDirection.OnlySubscribe, v => gameObject.SetActive(converter(v)));
		}

		public static RREvent<bool>.SubscriberInfo BindActive(this RREvent<bool> state, GameObject gameObject)
		{
			return state.BindInternal(gameObject.transform, BindDirection.OnlySubscribe, v => gameObject.SetActive(v));
		}

		public static RREvent<T>.SubscriberInfo BindActive<T>(this RREvent<T> state, Component component, Func<T, bool> converter)
		{
			return state.BindInternal(component, BindDirection.OnlySubscribe, v => component.gameObject.SetActive(converter(v)));
		}

		public static RREvent<bool>.SubscriberInfo BindActive(this RREvent<bool> state, Component component)
		{
			return state.BindInternal(component, BindDirection.OnlySubscribe, v => component.gameObject.SetActive(v));
		}

		public static RREvent<T>.SubscriberInfo BindSelected<T>(this RREvent<T> state, Graphic image, T selectedValue, Color selectedColor)
		{
			var unselectedColor = image.color;
			return state.BindInternal(image, BindDirection.OnlySubscribe, v => image.color = Equals(v, selectedValue) ? selectedColor : unselectedColor);
		}

		public static RREvent<T>.SubscriberInfo BindSelected<T>(this RREvent<T> state, Component component, T selectedValue, Color selectedColor)
		{
			var image = component.GetComponent<Graphic>();
			if (!image)
				throw new Exception("GameObject must contain Image");

			var unselectedColor = image.color;
			return state.BindInternal(image, BindDirection.OnlySubscribe, v => image.color = Equals(v, selectedValue) ? selectedColor : unselectedColor);
		}

		public static RREvent<T>.SubscriberInfo BindSelected<T>(this RREvent<T> state, Image image, T selectedValue, Sprite selectedSprite)
		{
			var unselectedSprite = image.sprite;
			return state.BindInternal(image, BindDirection.OnlySubscribe, v => image.sprite = Equals(v, selectedValue) ? selectedSprite : unselectedSprite);
		}

		public static RREvent<T>.SubscriberInfo BindSelected<T>(this RREvent<T> state, Component component, T selectedValue, Sprite selectedSprite)
		{
			Image image = component.GetComponent<Image>();
			if (!image)
				throw new Exception("GameObject must contain Image");

			var unselectedSprite = image.sprite;
			return state.BindInternal(image, BindDirection.OnlySubscribe, v => image.sprite = Equals(v, selectedValue) ? selectedSprite : unselectedSprite);
		}

		#endregion
	}

#if !NO_TEXTMESHPRO

	/// <summary>Implements data binding of TextMeshPro components and Bus Events</summary>
	public static class DataBindingExtensionForTextMeshPro
	{
		#region TwoWay string

		public static RREvent<string>.SubscriberInfo Bind(this RREvent<string> state, TMPro.TMP_InputField text, BindDirection way = BindDirection.TwoWay)
		{
			var res = state.BindInternal(text, way, v => text.SetTextWithoutNotify(v));
			if (way != BindDirection.OnlySubscribe)
			{
				var a = new UnityAction<string>(v => state.Publish(v));
				text.onValueChanged.AddListener(a);
				res.OnUnsubscribed(() => text.onValueChanged.RemoveListener(a));
			}
			return res;
		}

		#endregion

		#region TwoWay float

		public static RREvent<float>.SubscriberInfo Bind(this RREvent<float> state, TMPro.TMP_InputField text, string format = "0.00", BindDirection way = BindDirection.TwoWay)
		{
			var res = state.BindInternal(text, way, v => text.SetTextWithoutNotify(v.ToString(format)));
			if (way != BindDirection.OnlySubscribe)
			{
				var a = new UnityAction<string>(v =>
				{
					if (float.TryParse(v, out float val))
						state.Publish(val);
				});
				text.onValueChanged.AddListener(a);
				res.OnUnsubscribed(() => text.onValueChanged.RemoveListener(a));
			}
			return res;
		}

		#endregion

		#region TwoWay int

		public static RREvent<int>.SubscriberInfo Bind(this RREvent<int> state, TMPro.TMP_InputField text, string format = "0", BindDirection way = BindDirection.TwoWay)
		{
			var res = state.BindInternal(text, way, v => text.SetTextWithoutNotify(v.ToString(format)));
			if (way != BindDirection.OnlySubscribe)
			{
				var a = new UnityAction<string>(v =>
				{
					if (int.TryParse(v, out int val))
						state.Publish(val);
				});
				text.onValueChanged.AddListener(a);
				res.OnUnsubscribed(() => text.onValueChanged.RemoveListener(a));
			}
			return res;
		}

		public static RREvent<int>.SubscriberInfo Bind(this RREvent<int> state, TMPro.TMP_Dropdown dropdown, BindDirection way = BindDirection.TwoWay)
		{
			var res = state.BindInternal(dropdown, way, v => dropdown.SetValueWithoutNotify(v));
			if (way != BindDirection.OnlySubscribe)
			{
				var a = new UnityAction<int>(v => state.Publish(v));
				dropdown.onValueChanged.AddListener(a);
				res.OnUnsubscribed(() => dropdown.onValueChanged.RemoveListener(a));
			}
			return res;
		}

		#endregion

		#region TwoWay Enum

		/// <summary>Binding to Enum</summary>
		public static RREvent<T>.SubscriberInfo Bind<T>(this RREvent<T> state, TMPro.TMP_Dropdown dropdown, BindDirection way = BindDirection.TwoWay) where T : struct, IConvertible
		{
			if (!typeof(T).IsEnum)
			{
				throw new ArgumentException("T must be an enumerated type");
			}

			var res = state.BindInternal(dropdown, way, v => dropdown.SetValueWithoutNotify(Convert.ToInt32(v)));
			if (way != BindDirection.OnlySubscribe)
			{
				var a = new UnityAction<int>(v => state.Publish((T)(object)v));
				dropdown.onValueChanged.AddListener(a);
				res.OnUnsubscribed(() => dropdown.onValueChanged.RemoveListener(a));
			}
			return res;
		}

		#endregion

		#region OneWay

		public static RREvent<string>.SubscriberInfo Bind(this RREvent<string> state, TMPro.TextMeshProUGUI text)
		{
			return state.BindInternal(text, BindDirection.OnlySubscribe, v => text.SetText(v));
		}

		public static RREvent<int>.SubscriberInfo Bind(this RREvent<int> state, TMPro.TextMeshProUGUI text, string format = "0")
		{
			return state.BindInternal(text, BindDirection.OnlySubscribe, v => text.SetText(v.ToString(format)));
		}

		public static RREvent<float>.SubscriberInfo Bind(this RREvent<float> state, TMPro.TextMeshProUGUI text, string format = "0")
		{
			return state.BindInternal(text, BindDirection.OnlySubscribe, v => text.SetText(v.ToString(format)));
		}

        public static RREvent<T>.SubscriberInfo Bind<T>(this RREvent<T> state, TMPro.TextMeshProUGUI text, Func<T, string> converter)
        {
            return state.BindInternal(text, BindDirection.OnlySubscribe, v => text.SetText(converter(v)));
        }

        #endregion
    }

#endif
}
