using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace tkchJsonSerialize
{
	[Serializable]
	public class JsonMeshRenderer : JsonComponentBase
	{
		private MeshRenderer _targetComponent;
		public override Type ComponentType => typeof(MeshRenderer);
		
		public override float Version => 1.0f;
		//public override Type ComponentType => _targetComponent.GetType();
		
		public float __meshrenderer_json_dump_version__;
		
		public override void OnBeforeSerialize()
		{
			base.OnBeforeSerialize();
			__meshrenderer_json_dump_version__ = Version;
		}
		
		public string name;
		public string tag;
		public HideFlags hideFlags;
		
		public List<JsonMesh> additionalVertexStreams;
		public bool enabled;
		public List<JsonMaterial> materials;
		
		public JsonMeshRenderer(Component component) : base(component)
		{
			if (typeof(MeshRenderer) != component.GetType())
			{
				throw new ArgumentException();
			}

			var t = _targetComponent = (MeshRenderer) component;

			this.name = t.name;
			this.tag = t.tag;
			this.hideFlags = t.hideFlags;

			this.additionalVertexStreams = new List<JsonMesh>();
			if (!ReferenceEquals(t.additionalVertexStreams, null))
			{
				this.additionalVertexStreams.Add(new JsonMesh(t.additionalVertexStreams));
			}
			
			this.enabled = t.enabled;
			// todo 続き
			
			//this.material = new JsonMesh(t.material); // material で取得するとインスタンスが新しく生成されてしまう
			var sharedMaterials = t.sharedMaterials;
			if (!ReferenceEquals(sharedMaterials, null) && 0 < sharedMaterials.Length)
			{
				materials = new List<JsonMaterial>(sharedMaterials.Length);
				foreach (var m in sharedMaterials)
				{
					var jsonMaterial = new JsonMaterial(m);
					materials.Add(jsonMaterial);
				}
			}
			else
			{
				materials = new List<JsonMaterial>();
			}
			
			// todo 続き
		}

		public override void JsonRestore(Component component)
		{
			if (typeof(MeshRenderer) != component.GetType())
			{
				throw new ArgumentException();
			}
			var meshRenderer = (MeshRenderer) component;

			meshRenderer.name = name;
			meshRenderer.tag = tag;
			meshRenderer.hideFlags = hideFlags;

			if (!ReferenceEquals(additionalVertexStreams, null) && 0 < additionalVertexStreams.Count)
			{
				meshRenderer.additionalVertexStreams = additionalVertexStreams[0].value;
			}
			else
			{
				meshRenderer.additionalVertexStreams = null;
			}

			meshRenderer.enabled = enabled;
			
			if (!ReferenceEquals(materials, null) && 0 < materials.Count)
			{
				Material[] newMaterials = new Material[materials.Count];
				for (var i = 0; i < materials.Count; i++)
				{
					JsonMaterial jsonMaterial = materials[i];
					newMaterials[i] = jsonMaterial.value;
				}
				meshRenderer.sharedMaterials = newMaterials;
			}
			/*
			else
			{
				meshRenderer.sharedMaterial = new Material[0];
			}
			*/
			
			// todo 続き
		}
	}
}