using System.Collections.Generic;
using System;
using System.CodeDom;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace tkchJsonSerialize
{
	/// <summary>
	/// Cloth コンポーネントのプロパティを json 形式で出力するためのクラス
	/// </summary>
	[Serializable]
	public class JsonCloth : JsonComponentBase
	{
		private Cloth _targetComponent;
		public override Type ComponentType => typeof(Cloth);
		
		public override float Version => 1.0f;
		//public override Type ComponentType => _targetComponent.GetType();
		public float __cloth_json_dump_version__;
		
		public override void OnBeforeSerialize()
		{
			base.OnBeforeSerialize();
			__cloth_json_dump_version__ = Version;
		}
		
		public override void OnAfterDeserialize()
		{
			base.OnAfterDeserialize();
			NotFoundReferences = new List<JsonReference>();
		}
		
		public float selfCollisionDistance;
		public float selfCollisionStiffness;
		public float stretchingStiffness;
		public float bendingStiffness;
		public bool useTethers;
		public bool useGravity;
		public float damping;
		public JsonVector3 externalAcceleration;
		public JsonVector3 randomAcceleration;
		public float worldVelocityScale;
		public float worldAccelerationScale;
		public float friction;
		public float collisionMassScale;
		public bool enableContinuousCollision;
		public float clothSolverFrequency;
		public float sleepThreshold;

		public JsonCapsuleColliderReference[] capsuleColliders;
		public JsonClothSphereColliderPair[] sphereColliders;

		public float useVirtualParticles;
		public JsonClothSkinningCoefficient[] coefficients;
		public uint[] virtualParticleIndices;
		public JsonVector3[] virtualParticleWeights;
		public uint[] selfAndInterCollisionIndices;

		[NonSerialized] public List<JsonReference> NotFoundReferences = null;
		
		public JsonCloth(Component component) : base(component)
		{
			if (typeof(Cloth) != component.GetType())
			{
				throw new ArgumentException();
			}

			var c = _targetComponent = (Cloth) component;
			
			this.coefficients = new JsonClothSkinningCoefficient[c.coefficients.Length];
			for (int i = 0; i < c.coefficients.Length; i++)
			{
				this.coefficients[i] = new JsonClothSkinningCoefficient(c.coefficients[i]);
			}

			{
				var list = new List<uint>();
				c.GetSelfAndInterCollisionIndices(list);
				this.selfAndInterCollisionIndices = list.ToArray();
			}

			this.selfCollisionDistance = c.selfCollisionDistance;
			this.selfCollisionStiffness = c.selfCollisionStiffness;
			this.stretchingStiffness = c.stretchingStiffness;
			this.bendingStiffness = c.bendingStiffness;
			this.useTethers = c.useTethers;
			this.useGravity = c.useGravity;
			this.damping = c.damping;
			this.externalAcceleration = new JsonVector3(c.externalAcceleration);
			this.randomAcceleration = new JsonVector3(c.randomAcceleration);
			this.worldVelocityScale = c.worldVelocityScale;
			this.worldAccelerationScale = c.worldAccelerationScale;
			this.friction = c.friction;
			this.collisionMassScale = c.collisionMassScale;
			this.enableContinuousCollision = c.enableContinuousCollision;
			this.useVirtualParticles = c.useVirtualParticles;
			this.clothSolverFrequency = c.clothSolverFrequency;
			this.sleepThreshold = c.sleepThreshold;

			capsuleColliders = new JsonCapsuleColliderReference[c.capsuleColliders.Length];
			for (int i = 0; i < c.capsuleColliders.Length; i++)
			{
				capsuleColliders[i] = new JsonCapsuleColliderReference(c.capsuleColliders[i]);
			}

			sphereColliders = new JsonClothSphereColliderPair[c.sphereColliders.Length];
			for (int i = 0; i < c.sphereColliders.Length; i++)
			{
				sphereColliders[i] = new JsonClothSphereColliderPair(c.sphereColliders[i]);
			}

			this.useVirtualParticles = c.useVirtualParticles;

			List<Vector3> virtualParticleWeights = new List<Vector3>();
			c.GetVirtualParticleWeights(virtualParticleWeights);

			// なぜか Count が 0 の List になっているので　_items の Length を _size に設定して読み出せるようにする
			{
				var type = virtualParticleWeights.GetType();
				var items = type.GetField("_items",
					BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
				var size = type.GetField("_size",
					BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
				size.SetValue(virtualParticleWeights, ((Vector3[]) items.GetValue(virtualParticleWeights)).Length);
			}

			this.virtualParticleWeights = new JsonVector3[virtualParticleWeights.Count];
			for (int i = 0; i < virtualParticleWeights.Count; i++)
			{
				this.virtualParticleWeights[i] = new JsonVector3(virtualParticleWeights[i]);
			}

			{
				var list = new List<uint>();
				c.GetVirtualParticleIndices(list);
				this.virtualParticleIndices = list.ToArray();
			}
		}

		public override void JsonRestore(Component component)
		{
			if (typeof(Cloth) != component.GetType())
			{
				throw new ArgumentException();
			}
			var cloth = (Cloth) component;

			if (null != this.coefficients)
			{
				var array = new ClothSkinningCoefficient[this.coefficients.Length];
				for (int i = 0; i < this.coefficients.Length; i++)
				{
					array[i] = this.coefficients[i].value;
				}

				cloth.coefficients = array;
			}

			if (null != this.selfAndInterCollisionIndices)
			{
				var list = new List<uint>(this.selfAndInterCollisionIndices);
				cloth.SetSelfAndInterCollisionIndices(list);
			}

			cloth.selfCollisionDistance = this.selfCollisionDistance;
			cloth.selfCollisionStiffness = this.selfCollisionStiffness;
			cloth.stretchingStiffness = this.stretchingStiffness;
			cloth.bendingStiffness = this.bendingStiffness;
			cloth.useTethers = this.useTethers;
			cloth.useGravity = this.useGravity;
			cloth.damping = this.damping;
			cloth.externalAcceleration = this.externalAcceleration.value;
			cloth.randomAcceleration = this.randomAcceleration.value;
			cloth.worldVelocityScale = this.worldVelocityScale;
			cloth.worldAccelerationScale = this.worldAccelerationScale;
			cloth.friction = this.friction;
			cloth.collisionMassScale = this.collisionMassScale;
			cloth.enableContinuousCollision = this.enableContinuousCollision;
			cloth.useVirtualParticles = this.useVirtualParticles;
			cloth.clothSolverFrequency = this.clothSolverFrequency;
			cloth.sleepThreshold = this.sleepThreshold;

			if (null != this.capsuleColliders)
			{
				var array = new CapsuleCollider[this.capsuleColliders.Length];
				for (int i = 0; i < this.capsuleColliders.Length; i++)
				{
					array[i] = this.capsuleColliders[i].value;
				}

				cloth.capsuleColliders = array;
			}

			if (null != this.sphereColliders)
			{
				var array = new ClothSphereColliderPair[this.sphereColliders.Length];
				for (int i = 0; i < this.sphereColliders.Length; i++)
				{
					array[i] = this.sphereColliders[i].value;
				}

				cloth.sphereColliders = array;
			}

			if (null != this.virtualParticleWeights)
			{
				var list = new List<Vector3>(this.virtualParticleWeights.Length);
				for (int i = 0; i < this.virtualParticleWeights.Length; i++)
				{
					list.Add(this.virtualParticleWeights[i].value);
				}

				cloth.SetVirtualParticleWeights(list);
			}

			if (null != this.virtualParticleIndices)
			{
				var list = new List<uint>(this.virtualParticleIndices);
				cloth.SetVirtualParticleIndices(list);
			}
		}
		
		public override bool IsReferenceNotFound
		{
			get
			{
				if (null != capsuleColliders)
				{
					foreach (var t in capsuleColliders)
					{
						var jsonReference = t.reference;
						if (null == jsonReference)
						{
							continue;
						}

						if (JsonReference.ReferenceStateEnum.UnCheck == jsonReference.ReferenceState)
						{
							jsonReference.FindComponent();
						}

						if (JsonReference.ReferenceStateEnum.NotFound == jsonReference.ReferenceState)
						{
							NotFoundReferences.Add(jsonReference);
						}
					}
				}

				if (null != sphereColliders)
				{
					for (var i = 0; i < this.sphereColliders.Length; i++)
					{
						foreach (var jsonReference in new JsonReference[]
							{sphereColliders[i].first.reference, sphereColliders[i].second.reference})
						{
							if (null == jsonReference)
							{
								continue;
							}

							if (JsonReference.ReferenceStateEnum.UnCheck == jsonReference.ReferenceState)
							{
								jsonReference.FindComponent();
							}

							if (JsonReference.ReferenceStateEnum.NotFound == jsonReference.ReferenceState)
							{
								NotFoundReferences.Add(jsonReference);
							}
						}
					}
				}

				return (0 < NotFoundReferences.Count) ? true : false;
			}
		}

		public override bool NeedInspectorReload
		{
			get
			{
				return !ReferenceEquals(this.selfAndInterCollisionIndices, null);
			}
		}
		
		public override CheckResult[] AfterCheckFromJson(Component component = null)
		{
			var c = _targetComponent;
			if (!ReferenceEquals(component, null))
			{
				if (typeof(Cloth) != component.GetType())
				{
					throw new ArgumentException();
				}
				c = _targetComponent = (Cloth) component;
			}

			var checkResults = new List<CheckResult>();
			
			if (ReferenceEquals(this.selfAndInterCollisionIndices, null))
			{
				/*
				checkResults.Add(new CheckResult(
					"Item missing.", 
					"selfAndInterCollisionIndices", 
					CheckResult.ResultType.Warning));
					*/
			}
			else
			{
				// Set〜系メソッドで配列を減らす処理は期待通りの動作をしないので警告
				// SetSelfAndInterCollisionIndices
				// SetVirtualParticleWeights
				// SetVirtualParticleIndices

				var list = new List<uint>();
				c.GetSelfAndInterCollisionIndices(list);
				if (this.selfAndInterCollisionIndices.Length < list.Count)
				{
					checkResults.Add(new CheckResult(
						"Array reduction will fail.", 
						"selfAndInterCollisionIndices", 
						CheckResult.ResultType.Warning));
				}

				var v3list = new List<Vector3>();
				c.GetVirtualParticleWeights(v3list);
				if (this.virtualParticleWeights.Length < v3list.Count)
				{
					checkResults.Add(new CheckResult(
						"Array reduction will fail.", 
						"virtualParticleWeights", 
						CheckResult.ResultType.Warning));
				}

				list = new List<uint>();
				c.GetVirtualParticleIndices(list);
				if (this.virtualParticleIndices.Length < list.Count)
				{
					checkResults.Add(new CheckResult(
						"Array reduction will fail.", 
						"virtualParticleIndices", 
						CheckResult.ResultType.Warning));
				}
				
				/*
				if (!this.selfAndInterCollisionIndices.SequenceEqual(list.ToArray()))
				{
					checkResults.Add(new CheckResult(
						"Vertex sequence mismatch.", 
						"selfAndInterCollisionIndices", 
						CheckResult.ResultType.Warning));
				}
				*/

				// 頂点数の不一致は警告にする
				
				if (c.coefficients.Length != this.coefficients.Length)
				{
					checkResults.Add(new CheckResult(
						"Number of coefficients must match number of vertices!", 
						"coefficients", 
						CheckResult.ResultType.Warning));
				}
				
				// 頂点インデックスが範囲外だとエディタがおかしくなるのでエラーにする
				// SetSelfAndInterCollisionIndices

				foreach (var vertIndex in this.selfAndInterCollisionIndices)
				{
					if (coefficients.Length <= vertIndex)
					{
						checkResults.Add(new CheckResult(
							"Coefficients index is out of range.", 
							"selfAndInterCollisionIndices", 
							CheckResult.ResultType.Error));
						break;
					}
				}

				// 設定しても無視されるので警告しておく
				// SetVirtualParticleIndices
				
				if (0 < this.virtualParticleIndices.Length)
				{
					checkResults.Add(new CheckResult(
						"Array update will fail.", 
						"virtualParticleIndices", 
						CheckResult.ResultType.Warning));
				}
				
				/*
				verticesCount = -1;
				var skinnedMeshRenderer = c.transform.GetComponent<SkinnedMeshRenderer>();
				if (!ReferenceEquals(skinnedMeshRenderer, null))
				{
					var mesh = skinnedMeshRenderer.sharedMesh;
					if (!ReferenceEquals(mesh, null))
					{
						verticesCount = mesh.vertices.Length;
					}
				}

				if (0 <= verticesCount)
				{
					foreach (var vertIndex in this.virtualParticleIndices)
					{
						if (verticesCount <= vertIndex)
						{
							checkResults.Add(new CheckResult(
							 	"Vertices index is out of range.", 
							 	"virtualParticleIndices", 
							 	CheckResult.ResultType.Error));
							break;
						}
					}
				}
				*/
			}
			
			return checkResults.ToArray();
		}
	}
}