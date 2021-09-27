using System;
using System.Collections.Generic;
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
		
		public string name;
		public string tag;
		public HideFlags hideFlags;
		//public bool enabled;
		
		//public JsonMesh mesh;
		public List<JsonMesh> sharedMesh;
		
		public JsonMeshFilter(Component component) : base(component)
		{
			if (typeof(MeshFilter) != component.GetType())
			{
				throw new ArgumentException();
			}

			var t = _targetComponent = (MeshFilter) component;
			
			name = t.name;
			tag = t.tag;
			hideFlags = t.hideFlags;
			//enabled = t.enabled;
			
			//this.mesh = new JsonMesh(t.mesh); // mesh で取得するとインスタンスが新しく生成されてしまう
			sharedMesh = new List<JsonMesh>() {new JsonMesh(t.sharedMesh)};
		}

		public override void JsonRestore(Component component)
		{
			if (typeof(MeshFilter) != component.GetType())
			{
				throw new ArgumentException();
			}
			var meshFilter = (MeshFilter) component;

			meshFilter.name = name;
			meshFilter.tag = tag;
			meshFilter.hideFlags = hideFlags;
			
			if (!ReferenceEquals(sharedMesh, null) && 0 < sharedMesh.Count)
			{
				meshFilter.sharedMesh = sharedMesh[0].value;
			}
		}
	}
}