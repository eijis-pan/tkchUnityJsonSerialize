using System;
using System.Collections.Generic;
using UnityEngine;

namespace tkchJsonSerialize
{
	/*
	public interface IJsonSerializable<T>
	{
		string JsonDump();
		void JsonRestore(T target);
		bool IsReferenceNotFound { get; }
	}
	*/

	public class CheckResult
	{
		public enum ResultType
		{
			Info,
			Warning,
			Error
		}

		private string _message;
		private string _itemName;
		private ResultType _type;

		public virtual string Message => _message;
		public virtual string ItemName => _itemName;
		public virtual ResultType Type => _type;
		
		public CheckResult(string message, string itemName = null, ResultType type = ResultType.Info)
		{
			_message = message;
			_itemName = itemName;
			_type = type;
		}
	}

	/*
	[Serializable]
	public class JsonWrapper : ISerializationCallbackReceiver
	{
		public static readonly float Version = 1.0f;
		public float _version;
		public string _timestamp;
		public string _component;
		public JsonComponentBase Component;
		public JsonCloth Cloth;
		public Transform Transform;

		public JsonWrapper(JsonComponentBase jsonComponent)
		{
			Component = jsonComponent;
			//Cloth = (JsonCloth) jsonComponent;
			_component = jsonComponent.ComponentType.Name;
			_timestamp = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
			_version = JsonWrapper.Version;
		}
		
		public void OnBeforeSerialize()
		{
			Debug.Log("OnBeforeSerialize()");
		}

		public void OnAfterDeserialize()
		{
			Debug.Log("OnAfterDeserialize()");
		}
	}
	*/
	
	[Serializable]
	public class JsonComponentBase : ISerializationCallbackReceiver // IJsonSerializable<Component>
	{
		[NonSerialized]
		static public readonly Dictionary<Type, Type> ImplementedTypePairsFromJson =
			new Dictionary<Type, Type>()
		{
			{ typeof(JsonTransform), typeof(Transform) },
			{ typeof(JsonMeshFilter), typeof(MeshFilter) },
			{ typeof(JsonMeshRenderer), typeof(MeshRenderer) },
			{ typeof(JsonCloth), typeof(Cloth) }
		};
		
		public virtual float Version => throw new NotImplementedException();
		//public virtual Type ComponentType => throw new NotImplementedException();
		//public float __component_json_dump_version__;
		public string __json_dump_timestamp__;

		public virtual Type ComponentType => typeof(Component);
		
		public virtual void OnBeforeSerialize()
		{
			__json_dump_timestamp__ = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
		}

		public virtual void OnAfterDeserialize()
		{
			// nop
		}
		
		public static JsonComponentBase CreateJsonComponent(Component component)
		{
			foreach (var typePair in JsonComponentBase.ImplementedTypePairsFromJson)
			{
				var jsonType = typePair.Key;
				var componentType = typePair.Value;
				if (componentType == component.GetType())
				{
					object result = jsonType.InvokeMember(null, System.Reflection.BindingFlags.CreateInstance, null, null, new object[] { component });
					var jsonComponentBase = (JsonComponentBase) result;
					return jsonComponentBase;
				}
			}
			
			throw new NotImplementedException();
		}

		public static JsonComponentBase FromJson(Component component, string jsonString)
		{
			foreach (var typePair in JsonComponentBase.ImplementedTypePairsFromJson)
			{
				Type jsonType = typePair.Key;
				var componentType = typePair.Value;
				if (componentType == component.GetType())
				{
					return JsonUtility.FromJson(jsonString, jsonType) as JsonComponentBase;
				}
			}

			throw new NotImplementedException();
		}

		/*
		public static Action<Component, JsonComponentBase> DefaultRestoreAction = (component, jsonObject) =>
		{
			jsonObject.JsonRestore(component);
		};
		*/
		
		protected JsonComponentBase(Component component)
		{
			// nop
		}

		public virtual string JsonDump()
		{
			//var json = JsonUtility.ToJson(new JsonWrapper(this));
			/*
			var json = string.Format(
				"{{\"{0}\":{1},\"__{2}_dump_version__\":{3},\"__timestamp__\":\"{4}\"}}",
				this.ComponentType.Name,
				JsonUtility.ToJson(this),
				this.ComponentType.Name.ToLower(),
				this.Version.ToString("F02"),
				DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")
				);
				*/
			var json = JsonUtility.ToJson(this);
			//Debug.Log(json);
			return json;
		}
		
		public virtual void JsonRestore(Component component)
		{
			throw new NotImplementedException();
		}
		
		public virtual bool IsReferenceNotFound => false;

		public virtual bool NeedInspectorReload => false;
		
		public virtual CheckResult[] AfterCheckFromJson(Component component = null)
		{
			return new CheckResult[0];
		}
	}
}