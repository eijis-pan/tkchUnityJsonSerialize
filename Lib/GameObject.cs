using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace tkchJsonSerialize
{
	/*
	public interface IJsonInstantiation // <T>
	{
		void Instantiation();
		//void Instantiation(Scene sc);
		//void Instantiation(GameObject parent);
	}
	*/
	
	[Serializable]
	public class JsonRoot : ISerializationCallbackReceiver //, IJsonInstantiation
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

		public void JsonRestoreToSecne()
		{
			var scene = SceneManager.GetActiveScene();

			foreach (var child in children)
			{
				var go = child.JsonRestoreObject();
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
		public string assetPath;
		public List<JsonGameObject> children = new List<JsonGameObject>();
		public List<JsonComponentType> componentTypes = new List<JsonComponentType>();
		
		public JsonGameObject(GameObject gameObject)
		{
			init(gameObject);
		}
		
		public JsonGameObject(GameObject gameObject, string path)
		{
			this.path = path;
			init(gameObject);
		}

		protected void init(GameObject gameObject)
		{
			this.name = gameObject.name;
			AddComponentTypes(gameObject);
			SearchAssetPath(gameObject);
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
		
		protected void SearchAssetPath(GameObject gameObject)
		{
			var gameObjectId = PrefabUtility.GetPrefabParent(gameObject);
			//var gameObjectId = gameObject.GetInstanceID();
			var gameObjectAssetPath = AssetDatabase.GetAssetPath(gameObjectId);
			this.assetPath = gameObjectAssetPath;
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

		public GameObject JsonRestoreObject()
		{
			if (!ReferenceEquals(assetPath, null) && 0 < assetPath.Length)
			{
				if (assetPath.EndsWith(".prefab") || assetPath.EndsWith(".fbx"))
				{
					var loadedAsset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
					PrefabUtility.InstantiatePrefab(loadedAsset);
					return loadedAsset;
				}
			}

			var go = new GameObject();
			go.name = this.name;

			if (!ReferenceEquals(children, null))
			{
				foreach (var child in children)
				{
					var childGo = child.JsonRestoreObject();
					childGo.transform.SetParent(go.transform);
				}
			}
			
			if (!ReferenceEquals(componentTypes, null))
			{
				foreach (var child in componentTypes)
				{
					foreach (var componentType in JsonComponentBase.ImplementedTypePairsFromJson.Values)
					{
						if (componentType.Name != child.name)
						{
							continue;
						}
						
						Component c = null;
						
						// Transform は最初からある
						if (componentType == typeof(Transform))
						{
							c = go.GetComponent<Transform>();
						}
						else
						{
							c = go.AddComponent(componentType);							
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
		public List<JsonMeshFilter> meshFilter;
		public List<JsonMeshRenderer> meshRendere;
		public List<JsonCloth> cloth;

		private Dictionary<Type, object> _componentDict;
		
		public JsonComponentType(Component t)
		{
			this.name = t.GetType().Name;
			try
			{
				component = JsonComponentBase.CreateJsonComponent(t);
			}
			catch (Exception ex)
			{
				Debug.LogFormat("Custom Script Exception : {0} : {1}", this.name, ex.ToString());
			}
			
			transform = new List<JsonTransform>();
			meshFilter = new List<JsonMeshFilter>();
			meshRendere = new List<JsonMeshRenderer>();
			cloth = new List<JsonCloth>();
		}

		private void initDict()
		{
			_componentDict = new Dictionary<Type, object>(
				JsonComponentBase.ImplementedTypePairsFromJson.Count
			);
			var componentTypes = new Type[JsonComponentBase.ImplementedTypePairsFromJson.Count];
			JsonComponentBase.ImplementedTypePairsFromJson.Values.CopyTo(componentTypes, 0);
			_componentDict[componentTypes[0]] = transform;
			_componentDict[componentTypes[1]] = meshFilter;
			_componentDict[componentTypes[2]] = meshRendere;
			_componentDict[componentTypes[3]] = cloth;
		}
		
		public void OnBeforeSerialize()
		{
			// 全種類を網羅してゴリ押し
			// 配列にして、中身が（1つ）あれば該当。空だと対象外（そのクラスではない）

			initDict();
			
			if (!ReferenceEquals(component, null))
			{
				foreach (var typePair in JsonComponentBase.ImplementedTypePairsFromJson)
				{
					var jsonType = typePair.Key;
					var componentType = typePair.Value;
					if (jsonType == component.GetType())
					{
						object o = _componentDict[componentType];
						var list = ListTypeCheck(o, jsonType);
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
			
			foreach (var typePair in JsonComponentBase.ImplementedTypePairsFromJson)
			{
				var jsonType = typePair.Key;
				var componentType = typePair.Value;
				object o = _componentDict[componentType];
				var list = ListTypeCheck(o, jsonType);
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