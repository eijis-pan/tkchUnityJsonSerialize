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
		public string hideFlags;
		
		public List<JsonMesh> additionalVertexStreams;
		public bool enabled;
		public List<JsonMaterial> materials;
		public List<string> assetPaths;
		
		public JsonMeshRenderer(Component component) : base(component)
		{
			if (typeof(MeshRenderer) != component.GetType())
			{
				throw new ArgumentException();
			}

			var t = _targetComponent = (MeshRenderer) component;

			this.name = t.name;
			this.tag = t.tag;
			this.hideFlags = t.hideFlags.ToString();

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
				assetPaths = new List<string>(sharedMaterials.Length);
				materials = new List<JsonMaterial>(sharedMaterials.Length);
				foreach (var m in sharedMaterials)
				{
					var instanceId = m.GetInstanceID();
					var assetPath = AssetDatabase.GetAssetPath(instanceId);
					assetPaths.Add(assetPath);
					var jsonMaterial = new JsonMaterial(m);
					materials.Add(jsonMaterial);
				}
			}
			else
			{
				assetPaths = new List<string>();
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
			meshRenderer.hideFlags = (HideFlags)Enum.Parse(typeof(HideFlags), hideFlags);

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
				int count = materials.Count;
				if (!ReferenceEquals(assetPaths, null) && 0 < assetPaths.Count)
				{
					if (assetPaths.Count < count)
					{
						count = assetPaths.Count;
					}
				}
				else
				{
					count = 0;
				}

				meshRenderer.materials = new Material[count - 1];
				for (var i = 0; i < count; i++)
				{
					string assetPath = assetPaths[i];
					JsonMaterial jsonMaterial = materials[i];
					meshRenderer.materials[i] = jsonMaterial.value;
				}
			}
			else
			{
				meshRenderer.materials = new Material[0];
			}
			
			// todo 続き
		}
	}
}