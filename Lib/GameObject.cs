using System;
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
		//public JsonGameObject(Component component) : base(component)
		{
			/*
			if (typeof(GameObject) != component.GetType())
			{
				throw new ArgumentException();
			}
			
			var gameObject = _targetComponent = (GameObject) component;
			*/
			
			this.name = gameObject.name;
		}
		
		public JsonGameObject(GameObject gameObject, string path)
		{
			this.name = gameObject.name;
			this.path = path;
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
			
			return go;
		}
	}
	
	[Serializable]
	public class JsonComponentType : ISerializationCallbackReceiver
	{
		public string name;
		
		public JsonComponentType(Component t)
		{
			this.name = t.GetType().Name;
		}
		
		public void OnBeforeSerialize()
		{
			// nop
		}

		public void OnAfterDeserialize()
		{
			// nop
		}
	}
}