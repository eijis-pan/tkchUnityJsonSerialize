using System;
using UnityEditor;
using UnityEngine;

namespace tkchJsonSerialize
{
    [Serializable]
    public class JsonAnimator : JsonComponentBase
    {
        private Animator _targetComponent;
        public override Type ComponentType => typeof(Animator);
		
        public override float Version => 1.0f;
        //public override Type ComponentType => _targetComponent.GetType();
		
        public float __animator_json_dump_version__;
		
        public override void OnBeforeSerialize()
        {
            base.OnBeforeSerialize();
            __animator_json_dump_version__ = Version;
        }
		
        public string name;
        public string tag;
        public HideFlags hideFlags;
        public bool enabled;

        public JsonAssetReference avatar;
        public float speed;
        public JsonVector3 bodyPosition;
        public JsonQuaternion bodyRotation;
        public string cullingMode;
        public bool fireEvents;
        public bool logWarnings;
        public float playbackTime;
        public JsonVector3 rootPosition;
        public JsonQuaternion rootRotation;
        public bool stabilizeFeet;
        public string updateMode;
        public bool applyRootMotion;
        public float feetPivotActive;
        public float recorderStartTime;
        public float recorderStopTime;
        public JsonAssetReference runtimeAnimatorController;
        public bool layersAffectMassCenter;
        public bool keepAnimatorControllerStateOnDisable;
        
        public JsonAnimator(Component component) : base(component)
        {
            if (typeof(Animator) != component.GetType())
            {
                throw new ArgumentException();
            }

            var t = _targetComponent = (Animator) component;
			
            name = t.name;
            tag = t.tag;
            hideFlags = t.hideFlags;
            enabled = t.enabled;

            var ava = t.avatar;
            if (!ReferenceEquals(ava, null))
            {
                var instanceId = ava.GetInstanceID();
                var assetPath = AssetDatabase.GetAssetPath(instanceId);
                avatar = new JsonAssetReference(assetPath);
            }
            
            speed = t.speed;
            bodyPosition = new JsonVector3(t.bodyPosition);
            bodyRotation = new JsonQuaternion(t.bodyRotation);
            cullingMode = t.cullingMode.ToString();
            fireEvents = t.fireEvents;
            logWarnings = t.logWarnings;
            playbackTime = t.playbackTime;
            rootPosition = new JsonVector3(t.rootPosition);
            rootRotation = new JsonQuaternion(t.rootRotation);
            stabilizeFeet = t.stabilizeFeet;
            updateMode = t.updateMode.ToString();
            applyRootMotion = t.applyRootMotion;
            feetPivotActive = t.feetPivotActive;
            recorderStartTime = t.recorderStartTime;
            recorderStopTime = t.recorderStopTime;
            
            var rac = t.runtimeAnimatorController;
            if (!ReferenceEquals(rac, null))
            {
                var instanceId = rac.GetInstanceID();
                var assetPath = AssetDatabase.GetAssetPath(instanceId);
                runtimeAnimatorController = new JsonAssetReference(assetPath);
            }

            layersAffectMassCenter = t.layersAffectMassCenter;
            keepAnimatorControllerStateOnDisable = t.keepAnimatorControllerStateOnDisable;
        }

        public override void JsonRestore(Component component)
        {
            if (typeof(Animator) != component.GetType())
            {
                throw new ArgumentException();
            }
            var animator = (Animator) component;
            
            animator.name = name;
            animator.tag = tag;
            animator.hideFlags = hideFlags;
            animator.enabled = enabled;

            if (!ReferenceEquals(avatar, null) && !ReferenceEquals(avatar.assetPath, null) && 0 < avatar.assetPath.Length)
            {
                var avatarAsset = (Avatar)avatar.FindAsset(typeof(Avatar), null);
                animator.avatar = avatarAsset;
            }
            
            animator.speed = speed;
            animator.bodyPosition = bodyPosition.value;
            animator.bodyRotation = bodyRotation.value;
            if (!ReferenceEquals(cullingMode, null) && 0 < cullingMode.Length)
            {
                animator.cullingMode = (AnimatorCullingMode)Enum.Parse(typeof(AnimatorCullingMode), cullingMode);
            }
            animator.fireEvents = fireEvents;
            animator.logWarnings = logWarnings;
            animator.playbackTime = playbackTime;
            animator.rootPosition = rootPosition.value;
            animator.rootRotation = rootRotation.value;
            animator.stabilizeFeet = stabilizeFeet;
            if (!ReferenceEquals(updateMode, null) && 0 < updateMode.Length)
            {
                animator.updateMode = (AnimatorUpdateMode)Enum.Parse(typeof(AnimatorUpdateMode), updateMode);
            }
            animator.applyRootMotion = applyRootMotion;
            animator.feetPivotActive = feetPivotActive;
            animator.recorderStartTime = recorderStartTime;
            animator.recorderStopTime = recorderStopTime;
            
            if (!ReferenceEquals(runtimeAnimatorController, null) && !ReferenceEquals(runtimeAnimatorController.assetPath, null) && 0 < runtimeAnimatorController.assetPath.Length)
            {
                var runtimeAnimatorControllerAsset = (RuntimeAnimatorController)runtimeAnimatorController.FindAsset(typeof(RuntimeAnimatorController), null);
                animator.runtimeAnimatorController = runtimeAnimatorControllerAsset;
            }

            animator.layersAffectMassCenter = layersAffectMassCenter;
            animator.keepAnimatorControllerStateOnDisable = keepAnimatorControllerStateOnDisable;
        }
    }
}