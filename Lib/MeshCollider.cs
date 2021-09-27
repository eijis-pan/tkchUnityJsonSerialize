using System;
using System.Collections.Generic;
using UnityEngine;

namespace tkchJsonSerialize
{
    [Serializable]
    public class JsonMeshCollider : JsonComponentBase
    {
        private MeshCollider _targetComponent;
        public override Type ComponentType => typeof(MeshCollider);
		
        public override float Version => 1.0f;
        //public override Type ComponentType => _targetComponent.GetType();
		
        public float __meshcollider_json_dump_version__;
		
        public override void OnBeforeSerialize()
        {
            base.OnBeforeSerialize();
            __meshcollider_json_dump_version__ = Version;
        }
		
        public string name;
        public string tag;
        public HideFlags hideFlags;
        public bool enabled;
        
        public bool convex;
        public MeshColliderCookingOptions cookingOptions;
        public List<JsonMesh> sharedMesh;
        public List<JsonPhysicMaterial> sharedMaterial;
        public float contactOffset;
        public bool isTrigger;
		
        public JsonMeshCollider(Component component) : base(component)
        {
            if (typeof(MeshCollider) != component.GetType())
            {
                throw new ArgumentException();
            }

            var t = _targetComponent = (MeshCollider) component;
			
            name = t.name;
            tag = t.tag;
            hideFlags = t.hideFlags;
            enabled = t.enabled;

            convex = t.convex;
            cookingOptions = t.cookingOptions;

            sharedMesh = new List<JsonMesh>();
            var mesh = t.sharedMesh;
            if (!ReferenceEquals(mesh, null))
            {
                sharedMesh.Add(new JsonMesh(t.sharedMesh));
            }
            
            sharedMaterial = new List<JsonPhysicMaterial>();
            var material = t.sharedMaterial;
            if (!ReferenceEquals(material, null))
            {
                sharedMaterial.Add(new JsonPhysicMaterial(material));
            }
            
            contactOffset = t.contactOffset;
            isTrigger = t.isTrigger;
        }

        public override void JsonRestore(Component component)
        {
            if (typeof(MeshCollider) != component.GetType())
            {
                throw new ArgumentException();
            }
            var meshCollider = (MeshCollider) component;
            
            meshCollider.name = name;
            meshCollider.tag = tag;
            meshCollider.hideFlags = hideFlags;
            meshCollider.enabled = enabled;

            meshCollider.convex = convex;
            meshCollider.cookingOptions = cookingOptions;
            if (!ReferenceEquals(sharedMesh, null) && 0 < sharedMesh.Count)
            {
                meshCollider.sharedMesh = sharedMesh[0].value;
            }
            if (!ReferenceEquals(sharedMaterial, null) && 0 < sharedMaterial.Count)
            {
                meshCollider.sharedMaterial = sharedMaterial[0].value;
            }
            meshCollider.contactOffset = contactOffset;
            meshCollider.isTrigger = isTrigger;
        }
    }
}