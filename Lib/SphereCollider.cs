using System;
using System.Collections.Generic;
using UnityEngine;

namespace tkchJsonSerialize
{
    [Serializable]
    public class JsonSphereCollider : JsonComponentBase
    {
        private SphereCollider _targetComponent;
        public override Type ComponentType => typeof(SphereCollider);
		
        public override float Version => 1.0f;
        //public override Type ComponentType => _targetComponent.GetType();
		
        public float __spherecollider_json_dump_version__;
		
        public override void OnBeforeSerialize()
        {
            base.OnBeforeSerialize();
            __spherecollider_json_dump_version__ = Version;
        }
		
        public string name;
        public string tag;
        public HideFlags hideFlags;
        public bool enabled;
        
        public JsonVector3 center;
        public float radius;
        public List<JsonPhysicMaterial> sharedMaterial;
        public float contactOffset;
        public bool isTrigger;
        
        public JsonSphereCollider(Component component) : base(component)
        {
            if (typeof(SphereCollider) != component.GetType())
            {
                throw new ArgumentException();
            }

            var t = _targetComponent = (SphereCollider) component;
			
            name = t.name;
            tag = t.tag;
            hideFlags = t.hideFlags;
            enabled = t.enabled;

            center = new JsonVector3(t.center);
            radius = t.radius;
            
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
            if (typeof(SphereCollider) != component.GetType())
            {
                throw new ArgumentException();
            }
            var sphereCollider = (SphereCollider) component;
            
            sphereCollider.name = name;
            sphereCollider.tag = tag;
            sphereCollider.hideFlags = hideFlags;
            sphereCollider.enabled = enabled;
            
            sphereCollider.center = center.value;
            sphereCollider.radius = radius;
            if (!ReferenceEquals(sharedMaterial, null) && 0 < sharedMaterial.Count)
            {
                sphereCollider.sharedMaterial = sharedMaterial[0].value;
            }
            sphereCollider.contactOffset = contactOffset;
            sphereCollider.isTrigger = isTrigger;
        }
    }
}