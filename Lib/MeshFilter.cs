using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace tkchJsonSerialize
{
	[Serializable]
	public class JsonMeshFilter : JsonComponentBase
	{
		private MeshFilter _targetComponent;
		public override Type ComponentType => typeof(MeshFilter);
		
		public override float Version => 1.0f;
		//public override Type ComponentType => _targetComponent.GetType();
		
		public float __meshfilter_json_dump_version__;
		
		public override void OnBeforeSerialize()
		{
			base.OnBeforeSerialize();
			__meshfilter_json_dump_version__ = Version;
		}
		
		//public JsonMesh mesh;
		public List<JsonMesh> sharedMesh;
		public string assetPath;
		
		public JsonMeshFilter(Component component) : base(component)
		{
			if (typeof(MeshFilter) != component.GetType())
			{
				throw new ArgumentException();
			}

			var t = _targetComponent = (MeshFilter) component;
			
			//this.mesh = new JsonMesh(t.mesh); // mesh で取得するとインスタンスが新しく生成されてしまう
			var m = t.sharedMesh;
			var meshInstanceId = m.GetInstanceID();
			var meshAssetPath = AssetDatabase.GetAssetPath(meshInstanceId);
			if (0 < meshAssetPath.Length)
			{
				assetPath = meshAssetPath;
			}
			else
			{
				sharedMesh = new List<JsonMesh>(1);
				var jsonMesh = new JsonMesh(m);
				sharedMesh.Add(jsonMesh);
			}
		}

		public override void JsonRestore(Component component)
		{
			if (typeof(MeshFilter) != component.GetType())
			{
				throw new ArgumentException();
			}
			var meshFilter = (MeshFilter) component;

			if (0 < assetPath.Length)
			{
				
			}
			else
			{
				if (!ReferenceEquals(sharedMesh, null) && 0 < sharedMesh.Count)
				{
					meshFilter.sharedMesh = sharedMesh[0].value;
				}
				
			}
		}
	}
}