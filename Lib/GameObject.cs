using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace tkchJsonSerialize
{
	public interface IJsonInstantiation // <T>
	{
		void Instantiation();
		//void Instantiation(Scene sc);
		//void Instantiation(GameObject parent);
	}
	
	[Serializable]
	public class JsonRoot : ISerializationCallbackReceiver, IJsonInstantiation
	{
		public List<string> includedPaths = new List<string>();
		public List<string> excludedPaths = new List<string>();
		public List<JsonGameObject> children = new List<JsonGameObject>();

		public JsonRoot(List<JsonGameObject> t)
		{
			this.children = t;
		}
		
		public void OnBeforeSerialize()
		{
			// foreach (var child in children)
			// {
			// 	child.depth = 1;
			// }
		}

		public void OnAfterDeserialize()
		{
			// nop
		}

		public void Instantiation()
		{
			var scene = SceneManager.GetActiveScene();

			foreach (var child in children)
			{
				var go = child.Instantiation();
				SceneManager.MoveGameObjectToScene(go, scene);
			}
		}
	}

	
	[Serializable]
	public class JsonGameObject : ISerializationCallbackReceiver
	{
		//private GameObject _targetComponent;
		
		public string name;
		public string path;
		public List<JsonGameObject> children = new List<JsonGameObject>();
		public List<JsonComponentType> componentTypes = new List<JsonComponentType>();
		
		public JsonGameObject(GameObject gameObject)
		{
			this.name = gameObject.name;
			AddComponentTypes(gameObject);
		}
		
		public JsonGameObject(GameObject gameObject, string path)
		{
			this.name = gameObject.name;
			this.path = path;
			AddComponentTypes(gameObject);
		}

		protected void AddComponentTypes(GameObject gameObject)
		{
			var components = gameObject.GetComponents(typeof(Component));
			
			foreach (var component in gameObject.GetComponents<Component>())
			{
				var jsonComponent = new JsonComponentType(component);
				componentTypes.Add(jsonComponent);
			}
		}
		
		public void OnBeforeSerialize()
		{
			// if (7 <= this.depth)
			// {
			// 	Debug.LogFormat("{0} : {1} \n", depth, name);
			// }
			//
			// foreach (var child in children)
			// {
			// 	child.depth = this.depth + 1;
			// }
		}

		public void OnAfterDeserialize()
		{
			// nop
		}

		public GameObject Instantiation()
		{
			var go = new GameObject();
			go.name = this.name;

			if (!ReferenceEquals(children, null))
			{
				foreach (var child in children)
				{
					var childGo = child.Instantiation();
					childGo.transform.SetParent(go.transform);
				}
			}
			
			if (!ReferenceEquals(componentTypes, null))
			{
				foreach (var child in componentTypes)
				{
					foreach (var t in JsonComponentBase.ImplementedComponentTypes)
					{
						if (t.Name != child.name)
						{
							continue;
						}
						
						Component c = null;
						
						// Transform は最初からある
						if (t == typeof(Transform))
						{
							c = go.GetComponent<Transform>();
						}
						else
						{
							c = go.AddComponent(t);							
						}

						JsonComponentBase jc = child.component;
						if (ReferenceEquals(jc, null))
						{
							continue;
						}
						jc.JsonRestore(c);
					}
				}
			}
			
			return go;
		}
	}
	
	[Serializable]
	public class JsonComponentType : ISerializationCallbackReceiver
	{
		public string name;
		
		// todo: 継承先のクラスとしてシリアライズされない
		[NonSerialized]
		public JsonComponentBase component;
		
		// 全種類を網羅してゴリ押し
		// 配列にして、中身が（1つ）あれば該当。空だと対象外（そのクラスではない）
		
		public List<JsonTransform> transform;
		public List<JsonCloth> cloth;

		private Dictionary<Type, object> _componentDict;
		
		public JsonComponentType(Component t)
		{
			this.name = t.GetType().Name;
			try
			{
				component = JsonComponentBase.CreateJsonComponent(t);
			}
			catch
			{
				
			}

			transform = new List<JsonTransform>(1);
			cloth = new List<JsonCloth>(1);
		}

		private void initDict()
		{
			_componentDict = new Dictionary<Type, object>(
				JsonComponentBase.ImplementedJsonObjectTypes.Length
			);
			_componentDict[JsonComponentBase.ImplementedJsonObjectTypes[0]] = transform;
			_componentDict[JsonComponentBase.ImplementedJsonObjectTypes[1]] = cloth;
		}
		
		public void OnBeforeSerialize()
		{
			// 全種類を網羅してゴリ押し
			// 配列にして、中身が（1つ）あれば該当。空だと対象外（そのクラスではない）

			if (!ReferenceEquals(component, null))
			{
				foreach (var t in JsonComponentBase.ImplementedJsonObjectTypes)
				{
					if (t == component.GetType())
					{
						object o = _componentDict[t];
						var list = ListTypeCheck(o, t);
						if (!ReferenceEquals(list, null))
						{
							list.Add(component);
						}
					}
				}
			}
		}

		public void OnAfterDeserialize()
		{
			// 全種類を網羅してゴリ押し
			// 配列にして、中身が（1つ）あれば該当。空だと対象外（そのクラスではない）

			initDict();
			
			foreach(var t in JsonComponentBase.ImplementedJsonObjectTypes)
			{
				object o = _componentDict[t];
				var list = ListTypeCheck(o, t);
				if (ReferenceEquals(list, null))
				{
					continue;
				}
				foreach (object item in list)
				{
					if (item is JsonComponentBase)
					{
						component = (JsonComponentBase)item;
					}
					break;
				}
			}
		}

		private IList ListTypeCheck(object o, Type argType)
		{
			if (ReferenceEquals(o, null))
			{
				return null;
			}

			var oArgTypes = o.GetType().GetGenericArguments();
			if (oArgTypes.Length <= 0)
			{
				return null;
			}
				
			var oArgType = oArgTypes[0];

			if (argType.Name != oArgType.Name)
			{
				return null;
			}
				
			if( o.GetType().GetGenericTypeDefinition() != typeof(List<>))
			{
				return null;
			}

			return (IList) o;
		}
	}
}