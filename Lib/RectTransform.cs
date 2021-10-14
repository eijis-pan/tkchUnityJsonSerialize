using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace tkchJsonSerialize
{
	[Preserve]
	[Serializable]
	public class JsonRectTransform : JsonComponentBase
	{
		private RectTransform _targetComponent;
		public override Type ComponentType => typeof(RectTransform);
		
		public override float Version => 1.0f;
		//public override Type ComponentType => _targetComponent.GetType();
		
		public float __rectTransform_json_dump_version__;
		
		public override void OnBeforeSerialize()
		{
			base.OnBeforeSerialize();
			__rectTransform_json_dump_version__ = Version;
		}
		
		public string name;
		public string tag;
		public HideFlags hideFlags;
		//public bool enabled;

		public JsonVector2 anchorMax;
		public JsonVector2 anchorMin;
		public JsonVector2 anchoredPosition;
		public JsonVector3 anchoredPosition3D;
		public int hierarchyCapacity;
		public JsonVector2 offsetMax;
		public JsonVector2 offsetMin;
		public JsonVector2 pivot;
		public JsonVector2 sizeDelta;
		
		// inherit transform
		public JsonVector3 forward;
		public JsonVector3 right;
		public JsonVector3 up;
		public JsonVector3 localPosition;
		public JsonQuaternion localRotation;
		public JsonVector3 localEulerAngles;
		public JsonVector3 localScale;

		public JsonRectTransform(Component component) : base(component)
		{
			if (typeof(RectTransform) != component.GetType())
			{
				throw new ArgumentException();
			}

			var t = _targetComponent = (RectTransform) component;
			
			name = t.name;
			tag = t.tag;
			hideFlags = t.hideFlags;
			//enabled = t.enabled;

			anchorMax = new JsonVector2(t.anchorMax);
			anchorMin = new JsonVector2(t.anchorMin);
			anchoredPosition = new JsonVector2(t.anchoredPosition);
			anchoredPosition3D = new JsonVector3(t.anchoredPosition3D);
			hierarchyCapacity = t.hierarchyCapacity;
			offsetMax = new JsonVector2(t.offsetMax);
			offsetMin = new JsonVector2(t.offsetMin);
			pivot = new JsonVector2(t.pivot);
			sizeDelta = new JsonVector2(t.sizeDelta);

			forward = new JsonVector3(t.forward);
			right = new JsonVector3(t.right);
			up = new JsonVector3(t.up);
			localPosition = new JsonVector3(t.localPosition);
			localRotation = new JsonQuaternion(t.localRotation);
			localEulerAngles = new JsonVector3(t.localEulerAngles);
			localScale = new JsonVector3(t.localScale);
		}

		public override void JsonRestore(Component component)
		{
			if (typeof(RectTransform) != component.GetType())
			{
				throw new ArgumentException();
			}
			var rectTransform = (RectTransform) component;
			
			rectTransform.name = name;
			rectTransform.tag = tag;
			rectTransform.hideFlags = hideFlags;
			//rectTransform.enabled = enabled;

			if (!ReferenceEquals(anchorMax, null)) { rectTransform.anchorMax = anchorMax.value; }
			if (!ReferenceEquals(anchorMin, null)) { rectTransform.anchorMin = anchorMin.value; }
			if (!ReferenceEquals(anchoredPosition, null)) { rectTransform.anchoredPosition = anchoredPosition.value; }
			if (!ReferenceEquals(anchoredPosition3D, null)) { rectTransform.anchoredPosition3D = anchoredPosition3D.value; }
			rectTransform.hierarchyCapacity = hierarchyCapacity;
			if (!ReferenceEquals(offsetMax, null)) { rectTransform.offsetMax = offsetMax.value; }
			if (!ReferenceEquals(offsetMin, null)) { rectTransform.offsetMin = offsetMin.value; }
			if (!ReferenceEquals(pivot, null)) { rectTransform.pivot = pivot.value; }
			if (!ReferenceEquals(sizeDelta, null)) { rectTransform.sizeDelta = sizeDelta.value; }
			
			// inherit transform
			if (!ReferenceEquals(forward, null)) { rectTransform.forward = forward.value; }
			if (!ReferenceEquals(right, null)) { rectTransform.right = right.value; }
			if (!ReferenceEquals(up, null)) { rectTransform.up = up.value; }
			if (!ReferenceEquals(localPosition, null)) { rectTransform.localPosition = localPosition.value; }
			if (!ReferenceEquals(localRotation, null)) { rectTransform.localRotation = localRotation.value; }
			if (!ReferenceEquals(localEulerAngles, null)) { rectTransform.localEulerAngles = localEulerAngles.value; }
			if (!ReferenceEquals(localScale, null)) { rectTransform.localScale = localScale.value; }
		}
	}
}