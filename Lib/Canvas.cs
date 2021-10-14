using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace tkchJsonSerialize
{
	[Preserve]
	[Serializable]
	public class JsonCanvas : JsonComponentBase
	{
		private Canvas _targetComponent;
		public override Type ComponentType => typeof(Canvas);
		
		public override float Version => 1.0f;
		//public override Type ComponentType => _targetComponent.GetType();
		
		public float __canvas_json_dump_version__;
		
		public override void OnBeforeSerialize()
		{
			base.OnBeforeSerialize();
			__canvas_json_dump_version__ = Version;
		}
		
		public string name;
		public string tag;
		public HideFlags hideFlags;
		public bool enabled;

		public RenderMode renderMode;
		public JsonCameraReference worldCamera;
		public int sortingOrder;
		public bool overrideSorting;
		public int sortingLayerID;
		public string sortingLayerName;
		public AdditionalCanvasShaderChannels additionalShaderChannels;
		public float planeDistance;
		public float scaleFactor;
		public bool pixelPerfect;
		public bool overridePixelPerfect;
		public int targetDisplay;
		
		public JsonCanvas(Component component) : base(component)
		{
			if (typeof(Canvas) != component.GetType())
			{
				throw new ArgumentException();
			}

			var t = _targetComponent = (Canvas) component;
			
			name = t.name;
			tag = t.tag;
			hideFlags = t.hideFlags;
			enabled = t.enabled;

			renderMode = t.renderMode;
			worldCamera = new JsonCameraReference(t.worldCamera);
			sortingOrder = t.sortingOrder;
			overrideSorting = t.overrideSorting;
			sortingLayerID = t.sortingLayerID;
			sortingLayerName = t.sortingLayerName;
			additionalShaderChannels = t.additionalShaderChannels;
			planeDistance = t.planeDistance;
			scaleFactor = t.scaleFactor;
			pixelPerfect = t.pixelPerfect;
			overridePixelPerfect = t.overridePixelPerfect;
			targetDisplay = t.targetDisplay;
		}

		public override void JsonRestore(Component component)
		{
			if (typeof(Canvas) != component.GetType())
			{
				throw new ArgumentException();
			}
			var canvas = (Canvas) component;
			
			canvas.name = name;
			canvas.tag = tag;
			canvas.hideFlags = hideFlags;
			canvas.enabled = enabled;

			canvas.renderMode = renderMode;
			if (!ReferenceEquals(worldCamera, null)) { canvas.worldCamera = worldCamera.value; }
			canvas.sortingOrder = sortingOrder;
			canvas.overrideSorting = overrideSorting;
			canvas.sortingLayerID = sortingLayerID;
			canvas.sortingLayerName = sortingLayerName;
			canvas.additionalShaderChannels = additionalShaderChannels;
			canvas.planeDistance = planeDistance;
			canvas.scaleFactor = scaleFactor;
			canvas.pixelPerfect = pixelPerfect;
			canvas.overridePixelPerfect = overridePixelPerfect;
			canvas.targetDisplay = targetDisplay;
		}
	}
}