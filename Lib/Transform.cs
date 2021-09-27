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
		public JsonVector3 right;
		public JsonVector3 up;
		public JsonVector3 localPosition;
		public JsonQuaternion localRotation;
		public JsonVector3 localEulerAngles;
		public JsonVector3 localScale;

		//
		// Not necessary for restore
		//
		
		// public JsonVector3 position;
		// public JsonQuaternion rotation;
		// public JsonVector3 eulerAngles;
		// public JsonVector3 lossyScale;
	
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
			this.right = new JsonVector3(t.right);
			this.up = new JsonVector3(t.up);
			this.localPosition = new JsonVector3(t.localPosition);
			this.localRotation = new JsonQuaternion(t.localRotation);
			this.localEulerAngles = new JsonVector3(t.localEulerAngles);
			this.localScale = new JsonVector3(t.localScale);

			//
			// Not necessary for restore
			//
			
			// this.position = new JsonVector3(t.position);
			// this.rotation = new JsonQuaternion(t.rotation);
			// this.eulerAngles = new JsonVector3(t.eulerAngles);
			// this.lossyScale = new JsonVector3(t.lossyScale);
		
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

			if (!ReferenceEquals(forward, null)) { transform.forward = forward.value; }
			if (!ReferenceEquals(right, null)) { transform.right = right.value; }
			if (!ReferenceEquals(up, null)) { transform.up = up.value; }
			if (!ReferenceEquals(localPosition, null)) { transform.localPosition = localPosition.value; }
			if (!ReferenceEquals(localRotation, null)) { transform.localRotation = localRotation.value; }
			if (!ReferenceEquals(localEulerAngles, null)) { transform.localEulerAngles = localEulerAngles.value; }
			if (!ReferenceEquals(localScale, null)) { transform.localScale = localScale.value; }
			
			// Not necessary for restore
			
			// if (!ReferenceEquals(position, null)) { transform.position = position.value; }
			// if (!ReferenceEquals(rotation, null)) { transform.rotation = rotation.value; }
			// if (!ReferenceEquals(eulerAngles, null)) { transform.eulerAngles = eulerAngles.value; }
			// if (!ReferenceEquals(lossyScale, null)) { transform.lossyScale = lossyScale.value; }
		}
	}
}