using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace tkchJsonSerialize
{
    [Serializable]
    public class JsonBoxCollider : JsonComponentBase
    {
        private BoxCollider _targetComponent;
        public override Type ComponentType => typeof(BoxCollider);
		
        public override float Version => 1.0f;
        //public override Type ComponentType => _targetComponent.GetType();
		
        public float __boxcollider_json_dump_version__;
		
        public override void OnBeforeSerialize()
        {
            base.OnBeforeSerialize();
            __boxcollider_json_dump_version__ = Version;
        }
		
        public string name;
        public string tag;
        public HideFlags hideFlags;
        public bool enabled;

        public JsonVector3 center;
        public JsonVector3 size;
        public List<JsonPhysicMaterial> sharedMaterial;
        public float contactOffset;
        public bool isTrigger;
        
        public JsonBoxCollider(Component component) : base(component)
        {
            if (typeof(BoxCollider) != component.GetType())
            {
                throw new ArgumentException();
            }

            var t = _targetComponent = (BoxCollider) component;
			
            name = t.name;
            tag = t.tag;
            hideFlags = t.hideFlags;
            enabled = t.enabled;
            
            center = new JsonVector3(t.center);
            size = new JsonVector3(t.size);
            
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
            if (typeof(BoxCollider) != component.GetType())
            {
                throw new ArgumentException();
            }
            var boxCollider = (BoxCollider) component;
            
            boxCollider.name = name;
            boxCollider.tag = tag;
            boxCollider.hideFlags = hideFlags;
            boxCollider.enabled = enabled;
            
            boxCollider.center = center.value;
            boxCollider.size = size.value;
            if (!ReferenceEquals(sharedMaterial, null) && 0 < sharedMaterial.Count)
            {
                boxCollider.sharedMaterial = sharedMaterial[0].value;
            }
            boxCollider.contactOffset = contactOffset;
            boxCollider.isTrigger = isTrigger;
        }
    }
}