using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace tkchJsonSerialize
{
	[Preserve]
	[Serializable]
	public class JsonTransform : JsonComponentBase
	{
		private Transform _targetComponent;
		public override Type ComponentType => typeof(Transform);
		
		public override float Version => 1.0f;
		//public override Type ComponentType => _targetComponent.GetType();
		
		public float __transform_json_dump_version__;
		
		public override void OnBeforeSerialize()
		{
			base.OnBeforeSerialize();
			__transform_json_dump_version__ = Version;
		}
		
		public JsonVector3 forward;
		public JsonVector3 position;
		public JsonVector3 right;
		public JsonQuaternion rotation;
		public JsonVector3 up;

		//
		// Not necessary for restore
		//
		
		// public JsonVector3 eulerAngles;
		// public JsonVector3 localPosition;
		// public JsonQuaternion localRotation;
		// public JsonVector3 localScale;
		// public JsonVector3 lossyScale;
		// public JsonVector3 localEulerAngles;
	
		//
		// read only
		//
		
		//public JsonMatrix4x4 localToWorldMatrix;
		//public JsonMatrix4x4 worldToLocalMatrix;
	
		public JsonTransform(Component component) : base(component)
		{
			if (typeof(Transform) != component.GetType())
			{
				throw new ArgumentException();
			}

			var t = _targetComponent = (Transform) component;
			
			this.forward = new JsonVector3(t.forward);
			//t.parent
			this.position = new JsonVector3(t.position);
			this.right = new JsonVector3(t.right);
			this.rotation = new JsonQuaternion(t.rotation);
			this.up = new JsonVector3(t.up);

			//
			// Not necessary for restore
			//
			
			// this.eulerAngles = new JsonVector3(t.eulerAngles);
			// this.localPosition = new JsonVector3(t.localPosition);
			// this.localRotation = new JsonQuaternion(t.localRotation);
			// this.localScale = new JsonVector3(t.localScale);
			// this.lossyScale = new JsonVector3(t.lossyScale);
			// this.localEulerAngles = new JsonVector3(t.localEulerAngles);
		
			//
			// read only
			//
			
			//this.localToWorldMatrix = new JsonMatrix4x4(t.localToWorldMatrix);
			//this.worldToLocalMatrix = new JsonMatrix4x4(t.worldToLocalMatrix);
		}

		public override void JsonRestore(Component component)
		{
			if (typeof(Transform) != component.GetType())
			{
				throw new ArgumentException();
			}
			var transform = (Transform) component;

			if (!ReferenceEquals(forward, null))
			{
				transform.forward = forward.value;
			}
			if (!ReferenceEquals(position, null))
			{
				transform.position = position.value;
			}
			if (!ReferenceEquals(right, null))
			{
				transform.right = right.value;
			}
			if (!ReferenceEquals(rotation, null))
			{
				transform.rotation = rotation.value;
			}
			if (!ReferenceEquals(up, null))
			{
				transform.up = up.value;
			}
		}
	}
}