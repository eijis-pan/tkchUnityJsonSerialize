using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace tkchJsonSerialize
{
	[Serializable]
	public class JsonSkinnedMeshRenderer : JsonComponentBase
	{
		private SkinnedMeshRenderer _targetComponent;
		public override Type ComponentType => typeof(MeshRenderer);
		
		public override float Version => 1.0f;
		//public override Type ComponentType => _targetComponent.GetType();
		
		public float __skinnedmeshrenderer_json_dump_version__;
		
		public override void OnBeforeSerialize()
		{
			base.OnBeforeSerialize();
			__skinnedmeshrenderer_json_dump_version__ = Version;
		}
		
		public string name;
		public string tag;
		public HideFlags hideFlags;
		
		public List<JsonMesh> sharedMesh;
		public bool enabled;
		public List<JsonMaterial> materials;
		
		public JsonSkinnedMeshRenderer(Component component) : base(component)
		{
			if (typeof(SkinnedMeshRenderer) != component.GetType())
			{
				throw new ArgumentException();
			}

			var t = _targetComponent = (SkinnedMeshRenderer) component;

			name = t.name;
			tag = t.tag;
			hideFlags = t.hideFlags;

			//this.mesh = new JsonMesh(t.mesh); // mesh で取得するとインスタンスが新しく生成されてしまう
			sharedMesh = new List<JsonMesh>() {new JsonMesh(t.sharedMesh)};
			
			enabled = t.enabled;
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
			if (typeof(SkinnedMeshRenderer) != component.GetType())
			{
				throw new ArgumentException();
			}
			var skinnedMeshRenderer = (SkinnedMeshRenderer) component;

			skinnedMeshRenderer.name = name;
			skinnedMeshRenderer.tag = tag;
			skinnedMeshRenderer.hideFlags = hideFlags;

			if (!ReferenceEquals(sharedMesh, null) && 0 < sharedMesh.Count)
			{
				skinnedMeshRenderer.sharedMesh = sharedMesh[0].value;
			}

			skinnedMeshRenderer.enabled = enabled;
			
			if (!ReferenceEquals(materials, null) && 0 < materials.Count)
			{
				Material[] newMaterials = new Material[materials.Count];
				for (var i = 0; i < materials.Count; i++)
				{
					JsonMaterial jsonMaterial = materials[i];
					newMaterials[i] = jsonMaterial.value;
				}
				skinnedMeshRenderer.sharedMaterials = newMaterials;
			}
			/*
			else
			{
				skinnedMeshRenderer.sharedMaterial = new Material[0];
			}
			*/
			
			// todo 続き
		}
	}
}