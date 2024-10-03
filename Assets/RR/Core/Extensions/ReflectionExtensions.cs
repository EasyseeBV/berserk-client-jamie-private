using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using RR.Core.DebugSystem;

namespace RR.Core.Extensions
{
	public static class ReflectionExtensions
	{
		public delegate bool CompareType(MethodInfo methodInfo);

		public static IEnumerable<MethodInfo> GetGenericMethods(this Type targetObject, string methodName, CompareType selectDelegate)
		{
			return targetObject?
				.GetMethods()
				.Where(info => info.Name == methodName)
				.Select(info => selectDelegate.Invoke(info) ? info : null)
				.Where(info => info != null);
		}

		public static T GetPropertyByPath<T>(object instance, string path)
		{
			var props = path.Split('.');
			var t = instance.GetType();
			foreach (var prop in props)
			{
				var propInfo = t.GetProperty(prop, (BindingFlags)54);
				if (propInfo != null)
				{
					instance = propInfo.GetValue(instance, null);
					t = propInfo.PropertyType;
				}
				else throw new ArgumentException("Properties path is not correct");
			}

			return (T)instance;
		}

		public static T GetFieldByPath<T>(object instance, string path)
		{
			var fields = path.Split('.');
			var t = instance.GetType();
			foreach (var field in fields)
			{
				var fieldInfo = t.GetField(field, (BindingFlags)54);
				if (fieldInfo != null)
				{
					instance = fieldInfo.GetValue(instance);
					t = fieldInfo.FieldType;
				}
				else throw new ArgumentException("Fields path is not correct");
			}

			return (T)instance;
		}

		public static bool IsSubclassOfRawGeneric(this Type toCheck, Type generic)
		{
			while (toCheck != null && toCheck != typeof(object))
			{
				var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
				if (generic == cur)
				{
					return true;
				}
				toCheck = toCheck.BaseType;
			}
			return false;
		}

		public static string HumanReadableName(this Type type)
		{
			if (!type.IsGenericType)
				return type.Name;

			return new StringBuilder()
				.Append(type.Name.Substring(0, type.Name.LastIndexOf("`", StringComparison.Ordinal)))
				.Append(type.GetGenericArguments()
					.Aggregate(
						"<",
						(aggregate, t) => aggregate + (aggregate == "<" ? "" : ",") + HumanReadableName(t)
					)
				)
				.Append(">")
				.ToString();
		}

		/// <summary>
		/// Copy property from one type to another.
		/// Slow.
		/// <returns>Returns <see cref="T2"/> second param.</returns>
		/// </summary>
		public static T2 CopyPropertiesTo<T1, T2>(this T1 from, T2 to)
		{
			if (from == null || to == null)
				return to;

			foreach (var fromProperty in from.GetType().GetProperties())
			{
				var toProperty = to.GetType().GetProperty(fromProperty.Name);

				if (toProperty.CantPassFromProperty(fromProperty))
					continue;

				toProperty.SetValue(to, fromProperty.GetValue(from, null), null);
			}

			return to;
		}

		/// <summary>
		/// Copy values from one type to another.
		/// Slow.
		/// <returns>Returns <see cref="T2"/> second param.</returns>
		/// </summary>
		public static T2 CopyFieldsTo<T1, T2>(this T1 from, T2 to)
		{
			if (from == null || to == null)
				return to;

			foreach (var fromField in from.GetType().GetFields())
			{
				var toField = to.GetType().GetField(fromField.Name);

				if (toField == null)
					continue;

				toField.SetValue(to, fromField.GetValue(from));
			}

			return to;
		}

		public static T DeepCloneFields<T>(this T from) where T : new()
			=> DeepCopyFieldsTo(from, new T());

		/// <summary>
		/// Copy values from one type to another.
		/// Slow and more HEAVY than CopyFields. Will create new instances in collections and reference types.
		/// WARNING - will throw if some of the nested types has no empty constructor.
		/// <returns>Returns <see cref="T2"/> second param.</returns>
		/// </summary>
		public static T2 DeepCopyFieldsTo<T1, T2>(this T1 from, T2 to)
		{
			if (from == null || to == null)
				return to;

			foreach (var fromField in from.GetType().GetFields())
			{
				var toField = to.GetType().GetField(fromField.Name);

				if (toField == null)
					continue;

				var fromValue = fromField.GetValue(from);
				toField.SetValue(to, fromValue);

				if (toField.FieldType.IsGenericType && typeof(IList).IsAssignableFrom(toField.FieldType))
				{
					var genericArgs = toField.FieldType.GetGenericArguments();
					if (genericArgs.Length > 1)
					{
						RRLogger.Log("Multiple generic types in IList are not supported for deepcopy atm.");
						continue;
					}

					if (!genericArgs[0].IsClass || IsPrimitive(genericArgs[0]))
						continue;

					var toValue = toField.GetValue(to);
					if (toValue == null)
						continue;

					var copyList = (IList)Activator.CreateInstance(toField.FieldType);

					foreach (var item in (IList)toValue)
					{
						var itemType = item.GetType();
						var constr = itemType.GetConstructor(new[] { itemType });

						if (constr != null)
						{
							copyList.Add(DeepCopyFieldsTo(item, Activator.CreateInstance(itemType, item)));
							continue;
						}

						copyList.Add(DeepCopyFieldsTo(item, Activator.CreateInstance(itemType)));
					}

					toField.SetValue(to, copyList);
				}

				if (toField.FieldType.IsClass && !IsPrimitive(toField.FieldType))
				{
					var toVal = toField.GetValue(to);
					if (toVal == null)
						continue;

					var constr = toField.FieldType.GetConstructor(new[] { toField.FieldType });
					if (constr != null)
					{
						toField.SetValue(to, toVal.DeepCopyFieldsTo(Activator.CreateInstance(toVal.GetType(), toVal)));
						continue;
					}

					if (toField.FieldType.GetConstructor(Type.EmptyTypes) == null)
					{
						RRLogger.Log("Parameterized constructors unsupported, skipping field.");
						continue;
					}

					toField.SetValue(to, toVal.DeepCopyFieldsTo(Activator.CreateInstance(toVal.GetType())));
				}
			}

			return to;
		}

		public static IEnumerable<PropertyInfo> GetNestedProperties(this Type initType, int depth = 2)
		{
			if (depth < 0)
				return Enumerable.Empty<PropertyInfo>();

			var accumulatedInfos = new List<PropertyInfo>();
			var properties = initType.GetProperties();
			var nestedProps = properties.SelectMany(x => x.PropertyType.GetNestedProperties(depth - 1));

			accumulatedInfos.AddRange(properties);
			accumulatedInfos.AddRange(nestedProps);

			return accumulatedInfos;
		}

		public static bool IsPrimitive(this Type obj)
		{
			return new[]
			{
				typeof (Enum),
				typeof (String),
				typeof (Char),
				typeof (Guid),
				typeof (Boolean),
				typeof (Byte),
				typeof (Int16),
				typeof (Int32),
				typeof (Int64),
				typeof (Single),
				typeof (Double),
				typeof (Decimal),
				typeof (SByte),
				typeof (UInt16),
				typeof (UInt32),
				typeof (UInt64),
				typeof (DateTime),
				typeof (DateTimeOffset),
				typeof (TimeSpan),
			}.Any(oo => obj == oo);
		}

		public static (object result, bool isFound) FindNestedValue(this Type initType, PropertyInfo searchInfo, object actualValue, int depth = 2)
		{
			if (depth < 0 || initType == null || actualValue == null)
				return (null, false);

			var properties = initType.GetProperties();
			var typeMatch = properties.FirstOrDefault(x => x.PropertyType == searchInfo.PropertyType && searchInfo.Name == x.Name);
			var isMatch = typeMatch != null;

			if (!isMatch)
				return properties
					.Where(IsAllowedType)
					.Select(property => property.PropertyType.FindNestedValue(searchInfo, property.GetValue(actualValue), depth - 1))
					.FirstOrDefault(prop => prop.result != null);

			actualValue = typeMatch.GetValue(actualValue);
			return (actualValue, true);
		}

		public static IEnumerable<Type> GetInheritedTypes(this Type type)
		{
			return Assembly.GetAssembly(type)
				.GetTypes()
				.Where(x => x.IsClass && !x.IsAbstract && x.IsSubclassOf(type))
				.ToList();
		}

		public static IEnumerable<Tuple<T, Type>> GetAllTypesByAttribute<T>(this Assembly assembly) where T : Attribute =>
			assembly
				.GetTypes()
				.Select(type => new Tuple<T, Type>(type.GetCustomAttribute<T>(), type))
				.Where(x => x.Item1 != null);

		private static bool IsAllowedType(PropertyInfo p) =>
			!p.PropertyType.IsArray
			&& !p.PropertyType.IsAbstract
			&& !p.PropertyType.IsInterface
			&& !p.PropertyType.IsPrimitive
			&& !p.PropertyType.IsValueType;

		private static bool CantPassFromProperty(this PropertyInfo toProperty, PropertyInfo fromProperty) =>
			toProperty == null
			|| !toProperty.CanWrite
			|| (toProperty.GetSetMethod(true) != null && toProperty.GetSetMethod(true).IsPrivate)
			|| (toProperty.GetSetMethod().Attributes & MethodAttributes.Static) != 0
			|| !toProperty.PropertyType.IsAssignableFrom(fromProperty.PropertyType);
	}
}