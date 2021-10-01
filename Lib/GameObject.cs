﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
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
	public class JsonRoot // : ISerializationCallbackReceiver //, IJsonInstantiation
	{
		public List<string> includedPaths = new List<string>();
		public List<string> excludedPaths = new List<string>();
		//public List<JsonGameObject> children = new List<JsonGameObject>();
		public List<int> instanceIdList;
		public List<JsonGameObject> gameObjectList;

		/*
		public JsonRoot(List<JsonGameObject> t)
		{
			this.children = t;
		}
		*/
		
		public JsonRoot(Dictionary<int, JsonGameObject> dict)
		{
			instanceIdList = new List<int>(dict.Keys);
			gameObjectList = new List<JsonGameObject>(dict.Values);
		}
		
		/*
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
		*/

		public void JsonRestoreToSecne()
		{
			var scene = SceneManager.GetActiveScene();

			bool changed = false;

			try
			{
				/*
				foreach (var child in children)
				{
					var go = child.JsonRestoreObject();
					SceneManager.MoveGameObjectToScene(go, scene);
					changed = true;
				}
				*/
				
				//foreach (var value in instanceIdToGameObject.Values)
				for (int i = 0; i < instanceIdList.Count && i < gameObjectList.Count; i++)
				{
					var jsonGameObject = gameObjectList[i];
					if (0 != jsonGameObject.parentInstanceId)
					{
						continue;
					}
					
					var go = jsonGameObject.JsonRestoreObject();
					SceneManager.MoveGameObjectToScene(go, scene);
					jsonGameObject.RestoreComponents(go);
					changed = true;

					foreach (var childInstanceId in jsonGameObject.childInstanceIds)
					{
						createGameObjectByInstanceId(childInstanceId, go.transform, scene, jsonGameObject.assetReference.assetPath);
					}
				}
				
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				throw;
			}
			finally
			{
				if (changed)
				{
					EditorSceneManager.MarkSceneDirty(scene);
				}
			}
		}

		private void createGameObjectByInstanceId(int instanceId, Transform parentTr, Scene sc, string parentAssetPath)
		{
			//var jsonGameObject = (JsonGameObject)instanceIdToGameObject[instanceId];

			var idx = instanceIdList.IndexOf(instanceId);
			if (idx < 0 || gameObjectList.Count <= idx)
			{
				return;
			}

			var jsonGameObject = gameObjectList[idx];
			var assetPath = jsonGameObject.assetReference.assetPath;
			GameObject go = null;
			if (parentAssetPath == assetPath)
			{
				for (int i = 0; i < parentTr.childCount; i++)
				{
					var child = parentTr.GetChild(i);
					if (child.name == jsonGameObject.name)
					{
						go = parentTr.gameObject;
						break;
					}
				}
			}
			if (ReferenceEquals(go, null))
			{
				go = jsonGameObject.JsonRestoreObject();
				
				//SceneManager.MoveGameObjectToScene(go, sc);
				go.transform.SetParent(parentTr);
			}
			
			jsonGameObject.RestoreComponents(go);
			
			foreach (var childInstanceId in jsonGameObject.childInstanceIds)
			{
				createGameObjectByInstanceId(childInstanceId, go.transform, sc, jsonGameObject.assetReference.assetPath);
			}
		}
	}

	
	[Serializable]
	public class JsonGameObject : ISerializationCallbackReceiver
	{
		//private GameObject _targetComponent;

		public int instanceId;
		public int parentInstanceId;
		public List<int> childInstanceIds = new List<int>();
		
		public string name;
		public string tag;
		public HideFlags hideFlags;
		
		public bool activeSelf;

		public string path;
		//public string assetPath;
		public JsonAssetReference assetReference;
		//public List<JsonGameObject> children = new List<JsonGameObject>();
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
			instanceId = gameObject.GetInstanceID();
			var tr = gameObject.transform;
			parentInstanceId = 0;
			if (!ReferenceEquals(tr.parent, null))
			{
				parentInstanceId = tr.parent.gameObject.GetInstanceID();
			}
			for (int i = 0; i < tr.childCount; i++)
			{
				childInstanceIds.Add(tr.GetChild(i).gameObject.GetInstanceID());
			}
			
			name = gameObject.name;
			tag = gameObject.tag;
			hideFlags = gameObject.hideFlags;

			activeSelf = gameObject.activeSelf;
			
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
			var gameObjectAssetPath = AssetDatabase.GetAssetPath(gameObjectId);
			assetReference = new JsonAssetReference(gameObjectAssetPath);
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
			GameObject go = null;
			if (JsonAssetReference.IsValid(assetReference))
			{
				go = (GameObject)assetReference.FindAsset(typeof(GameObject), null);
			}
			else
			{
				go = new GameObject();
			}
			
			go.name = name;
			go.tag = tag;
			go.hideFlags = hideFlags;
			
			go.SetActive(activeSelf);

			/*
			if (!ReferenceEquals(children, null))
			{
				foreach (var child in children)
				{
					var childGo = child.JsonRestoreObject();
					childGo.transform.SetParent(go.transform);
				}
			}
			*/

			// ここで行うと、子オブジェクトのスケールで問題が出る
			//RestoreComponents(go);
			
			return go;
		}

		public void RestoreComponents(GameObject go)
		{
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
						
						/*
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
						*/

						Component c = go.GetComponent(componentType);
						
						if (!ReferenceEquals(c, null))
						{
							try
							{
								var checkName = c.name;
							}
							catch
							{
								c = null;
							}
						}
						
						if (ReferenceEquals(c, null))
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
		public List<JsonMeshRenderer> meshRenderer;
		public List<JsonSkinnedMeshRenderer> skinnedMeshRenderer;
		public List<JsonMeshCollider> meshCollider;
		public List<JsonBoxCollider> boxCollider;
		public List<JsonSphereCollider> sphereCollider;
		public List<JsonAnimator> animator;
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
			meshRenderer = new List<JsonMeshRenderer>();
			skinnedMeshRenderer = new List<JsonSkinnedMeshRenderer>();
			meshCollider = new List<JsonMeshCollider>();
			boxCollider = new List<JsonBoxCollider>();
			sphereCollider = new List<JsonSphereCollider>();
			animator = new List<JsonAnimator>();
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
			_componentDict[componentTypes[2]] = meshRenderer;
			_componentDict[componentTypes[3]] = skinnedMeshRenderer;
			_componentDict[componentTypes[4]] = meshCollider;
			_componentDict[componentTypes[5]] = boxCollider;
			_componentDict[componentTypes[6]] = sphereCollider;
			_componentDict[componentTypes[7]] = animator;
			_componentDict[componentTypes[8]] = cloth;
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