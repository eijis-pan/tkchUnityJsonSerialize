using System.Collections.Generic;
using System;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace tkchJsonSerialize
{
	[Serializable]
	public class JsonVector2 : ISerializationCallbackReceiver
	{
		private Vector2 v;

		public float x;
		public float y;

		public JsonVector2(Vector2 v)
		{
			this.v = v;
			this.x = v.x;
			this.y = v.y;
		}

		public Vector2 value
		{
			get { return v; }
		}

		public void OnBeforeSerialize()
		{
			// nop
		}

		public void OnAfterDeserialize()
		{
			v = new Vector2(x, y);
		}
	}
	
	[Serializable]
	public class JsonVector3 : ISerializationCallbackReceiver
	{
		private Vector3 v;

		public float x;
		public float y;
		public float z;

		public JsonVector3(Vector3 v)
		{
			this.v = v;
			this.x = v.x;
			this.y = v.y;
			this.z = v.z;
		}

		public Vector3 value
		{
			get { return v; }
		}

		public void OnBeforeSerialize()
		{
			// nop
		}

		public void OnAfterDeserialize()
		{
			v = new Vector3(x, y, z);
		}
	}

	[Serializable]
	public class JsonVector4 : ISerializationCallbackReceiver
	{
		private Vector4 v;
	
		public float x;
		public float y;
		public float z;
		public float w;
	
		public JsonVector4(Vector4 v)
		{
			this.v = v;
			this.x = v.x;
			this.y = v.y;
			this.z = v.z;
			this.w = v.w;
		}

		public Vector4 value
		{
			get { return v; }
		}
	
		public void OnBeforeSerialize ()
		{
			// nop
		}
	
		public void OnAfterDeserialize ()
		{
			v = new Vector4(x, y, z, w);
		}
	}

	[Serializable]
	public class JsonQuaternion : ISerializationCallbackReceiver
	{
		private Quaternion q;
	
		public float x;
		public float y;
		public float z;
		public float w;
		
		//
		// Not necessary for restore
		//
		
		//public JsonVector3 eulerAngles;
	
		public JsonQuaternion(Quaternion q)
		{
			this.q = q;
			this.x = q.x;
			this.y = q.y;
			this.z = q.z;
			this.w = q.w;
			
			//
			// Not necessary for restore
			//
			
			//this.eulerAngles = new JsonVector3(q.eulerAngles);
		}

		public Quaternion value
		{
			get { return q; }
		}
	
		public void OnBeforeSerialize ()
		{
			// nop
		}
	
		public void OnAfterDeserialize ()
		{
			q = new Quaternion(x, y, z, w);
		}
	}

	[Serializable]
	public class JsonMatrix4x4 : ISerializationCallbackReceiver
	{
		private Matrix4x4 m44;
	
		public JsonVector4[] v4 = new JsonVector4[4];
	
		public JsonMatrix4x4(Matrix4x4 m44)
		{
			this.m44 = m44;
			for (var col = 0; col < 4; col++)
			{
				v4[col] = new JsonVector4(m44.GetColumn(col));
			}
		}

		public Matrix4x4 value
		{
			get { return m44; }
		}
	
		public void OnBeforeSerialize ()
		{
			// nop
		}
	
		public void OnAfterDeserialize ()
		{
			int col = 0;
			m44 = new Matrix4x4(v4[col++].value, v4[col++].value, v4[col++].value, v4[col++].value);
		}
	}

	[Serializable]
	public class JsonReference
	{
		// 別の場所に実体がある場合の情報
		public string name;
		public string HierarchyPath;
		public string ShortHierarchyPath;
		public string ComponentTypeName;
		public string ComponentTypeFullName;

		public enum ReferenceStateEnum
		{
			UnCheck,
			Unnecessary, // 不要
			Found,
			NotFound
		}
		[NonSerialized] public ReferenceStateEnum ReferenceState = ReferenceStateEnum.UnCheck;
		
		public JsonReference(Component c)
		{
			if (null == c)
			{
				throw new ArgumentNullException();
			}

			this.name = c.name;
			this.HierarchyPath = c.transform.GetHierarchyPath();
			this.ShortHierarchyPath = c.transform.GetShortHierarchyPath();
			this.ComponentTypeName = c.GetType().Name;
			this.ComponentTypeFullName = c.GetType().FullName;
		}

		public Component FindComponent()
		{
			Component c = null;
			if (string.IsNullOrEmpty(HierarchyPath) || string.IsNullOrEmpty(ComponentTypeFullName))
			{
				ReferenceState = ReferenceStateEnum.Unnecessary;
			}
			else
			{
				ReferenceState = ReferenceStateEnum.NotFound;
				var gameObject = GameObject.Find(HierarchyPath);
				if (gameObject)
				{
					foreach (var component in gameObject.GetComponents<Component>())
					{
						if (component.GetType().FullName == ComponentTypeFullName)
						{
							c = component;
							ReferenceState = ReferenceStateEnum.Found;
							break;
						}
					}
				}
			}

			return c;
		}
	}

	[Serializable]
	public class JsonCameraReference : ISerializationCallbackReceiver
	{
		// 別の場所に実体があるので参照先の情報のみ
		public JsonReference reference;

		// 参照先
		private Camera _camera = null;
		
		public JsonCameraReference(Camera camera)
		{
			if (!ReferenceEquals(camera, null))
			{
				reference = new JsonReference(camera);
				_camera = camera;
			}
		}
		
		public JsonReference.ReferenceStateEnum ReferenceState
		{
			get
			{
				if (null != reference)
				{
					return reference.ReferenceState;
				}
				
				return JsonReference.ReferenceStateEnum.Unnecessary;
			}
		}
		
		public Camera value
		{
			get
			{
				return _camera;
			}
		}

		public void OnBeforeSerialize()
		{
			// nop
		}

		public void OnAfterDeserialize()
		{
			// 参照情報なので実体は復元しない
			if (null != reference)
			{
				_camera = (Camera)reference.FindComponent();
			}
		}
	}
	
	[Serializable]
	public class JsonCapsuleColliderReference : ISerializationCallbackReceiver
	{
		// 別の場所に実体があるので参照先の情報のみ
		public JsonReference reference;

		// 参照先
		private CapsuleCollider _cc = null;
		
		public JsonCapsuleColliderReference(CapsuleCollider cc)
		{
			if (!ReferenceEquals(cc, null))
			{
				reference = new JsonReference(cc);
				_cc = cc;
			}
		}
		
		public JsonReference.ReferenceStateEnum ReferenceState
		{
			get
			{
				if (null != reference)
				{
					return reference.ReferenceState;
				}
				
				return JsonReference.ReferenceStateEnum.Unnecessary;
			}
		}
		
		public CapsuleCollider value
		{
			get
			{
				return _cc;
			}
		}

		public void OnBeforeSerialize()
		{
			// nop
		}

		public void OnAfterDeserialize()
		{
			// 参照情報なので実体は復元しない
			if (null != reference)
			{
				_cc = (CapsuleCollider)reference.FindComponent();
			}
		}
	}

	[Serializable]
	public class JsonSphereColliderReference : ISerializationCallbackReceiver
	{
		// 別の場所に実体があるので参照先の情報のみ
		public JsonReference reference;

		// 参照先
		private SphereCollider _sc = null;
		
		public JsonSphereColliderReference(SphereCollider sc)
		{
			if (!ReferenceEquals(sc, null))
			{
				reference = new JsonReference(sc);
				_sc = sc;
			}
		}
		
		public JsonReference.ReferenceStateEnum ReferenceState
		{
			get
			{
				if (null != reference)
				{
					return reference.ReferenceState;
				}
				
				return JsonReference.ReferenceStateEnum.Unnecessary;
			}
		}

		public SphereCollider value
		{
			get
			{
				return _sc;
			}
		}

		public void OnBeforeSerialize()
		{
			// nop
		}

		public void OnAfterDeserialize()
		{
			// 参照情報なので実体は復元しない
			if (null != reference)
			{
				_sc = (SphereCollider)reference.FindComponent();
			}
		}
	}
	
	[Serializable]
	public class JsonClothSphereColliderPair : ISerializationCallbackReceiver
	{
		private ClothSphereColliderPair cscp;

		public JsonSphereColliderReference first;
		public JsonSphereColliderReference second;

		public JsonClothSphereColliderPair(ClothSphereColliderPair cscp)
		{
			this.cscp = cscp;
			this.first = new JsonSphereColliderReference(cscp.first);
			this.second = new JsonSphereColliderReference(cscp.second);
		}
		
		public ClothSphereColliderPair value
		{
			get { return cscp; }
		}

		public JsonReference.ReferenceStateEnum ReferenceState
		{
			get
			{
				if (JsonReference.ReferenceStateEnum.NotFound == first.ReferenceState ||
				    JsonReference.ReferenceStateEnum.NotFound == second.ReferenceState)
				{
					return JsonReference.ReferenceStateEnum.NotFound;
				}
				else if (JsonReference.ReferenceStateEnum.UnCheck == first.ReferenceState ||
				         JsonReference.ReferenceStateEnum.UnCheck == second.ReferenceState)
				{
					return JsonReference.ReferenceStateEnum.UnCheck;
				}
				else if (JsonReference.ReferenceStateEnum.Found == first.ReferenceState ||
				         JsonReference.ReferenceStateEnum.Found == second.ReferenceState)
				{
					return JsonReference.ReferenceStateEnum.Found;
				}

				return JsonReference.ReferenceStateEnum.Unnecessary;
			}
		}

		public void OnBeforeSerialize()
		{
			// nop
		}

		public void OnAfterDeserialize()
		{
			cscp = new ClothSphereColliderPair();
			cscp.first = first.value;
			cscp.second = second.value;
		}
	}

	[Serializable]
	public class JsonClothSkinningCoefficient : ISerializationCallbackReceiver
	{
		private ClothSkinningCoefficient csc;

		public float maxDistance;
		public float collisionSphereDistance;

		public JsonClothSkinningCoefficient(ClothSkinningCoefficient csc)
		{
			this.csc = csc;
			this.maxDistance = csc.maxDistance;
			this.collisionSphereDistance = csc.collisionSphereDistance;
		}

		public ClothSkinningCoefficient value
		{
			get { return csc; }
		}

		public void OnBeforeSerialize()
		{
			// nop
		}

		public void OnAfterDeserialize()
		{
			csc = new ClothSkinningCoefficient();
			csc.maxDistance = maxDistance;
			csc.collisionSphereDistance = collisionSphereDistance;
		}
	}

	[Serializable]
	public class JsonBoneWeight : ISerializationCallbackReceiver
	{
		private BoneWeight b;

		public float weight0;
		public float weight1;
		public float weight2;
		public float weight3;
		public int boneIndex0;
		public int boneIndex1;
		public int boneIndex2;
		public int boneIndex3;
		
		public JsonBoneWeight(BoneWeight b)
		{
			this.b = b;

			this.weight0 = b.weight0;
			this.weight1 = b.weight1;
			this.weight2 = b.weight2;
			this.weight3 = b.weight3;
			this.boneIndex0 = b.boneIndex0;
			this.boneIndex1 = b.boneIndex1;
			this.boneIndex2 = b.boneIndex2;
			this.boneIndex3 = b.boneIndex3;
		}

		public BoneWeight value
		{
			get { return b;  }
		}
		
		public void OnBeforeSerialize ()
		{
			// nop
		}
	
		public void OnAfterDeserialize ()
		{
			b = new BoneWeight();
			b.weight0 = this.weight0;
			b.weight1 = this.weight1;
			b.weight2 = this.weight2;
			b.weight3 = this.weight3;
			b.boneIndex0 = this.boneIndex0;
			b.boneIndex1 = this.boneIndex1;
			b.boneIndex2 = this.boneIndex2;
			b.boneIndex3 = this.boneIndex3;
		}
	}

	[Serializable]
	public class JsonBounds : ISerializationCallbackReceiver
	{
		private Bounds b;

		public JsonVector3 center;
		public JsonVector3 extents;
		public JsonVector3 max;
		public JsonVector3 min;
		public JsonVector3 size;
		
		public JsonBounds(Bounds b)
		{
			this.b = b;

			this.center = new JsonVector3(b.center);
			this.extents = new JsonVector3(b.extents);
			this.max = new JsonVector3(b.max);
			this.min = new JsonVector3(b.min);
			this.size = new JsonVector3(b.size);
		}
		
		public Bounds value
		{
			get { return b;  }
		}
		
		public void OnBeforeSerialize ()
		{
			// nop
		}
	
		public void OnAfterDeserialize ()
		{
			b = new Bounds();
			b.center = center.value;
			b.extents = extents.value;
			b.max = max.value;
			b.min = min.value;
			b.size = size.value;
		}
	}

	[Serializable]
	public class JsonAssetReference
	{
		public string assetPath;

		public enum ReferenceStateEnum
		{
			UnCheck,
			Found,
			NotFound
		}
		[NonSerialized] public ReferenceStateEnum ReferenceState = ReferenceStateEnum.UnCheck;

		public static bool IsValid(JsonAssetReference jsonAssetReference)
		{
			return (
				!ReferenceEquals(jsonAssetReference, null) && 
				!ReferenceEquals(jsonAssetReference.assetPath, null) &&
			       0 < jsonAssetReference.assetPath.Length
				);
		}
		
		public JsonAssetReference(string assetPath)
		{
			this.assetPath = assetPath;
		}
		
		public object FindAsset (Type type, string param)
		{
			ReferenceState = ReferenceStateEnum.NotFound;
			object result = null;
			if (assetPath.EndsWith(".prefab"))
			{
				if (typeof(GameObject) == type)
				{
					var loadedAsset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
					var go = PrefabUtility.InstantiatePrefab(loadedAsset);
					result = go;
				}
				else
				{
					throw new NotImplementedException(string.Format("{0} の FindAsset() 未対応のType {1}", assetPath, type.Name));
				}

				if (!ReferenceEquals(result, null))
				{
					ReferenceState = ReferenceStateEnum.Found;
				}
			}
			else if (assetPath.EndsWith(".fbx") || assetPath.EndsWith(".dae"))
			{
				// fbx の場合
				if (typeof(GameObject) == type)
				{
					var loadedAsset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
					var go = PrefabUtility.InstantiatePrefab(loadedAsset);
					result = go;
				}
				else if (typeof(Mesh) == type)
				{
					// mesh が複数の場合がある
					var assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
					foreach (var asset in assets)
					{
						if (asset.GetType() == type && asset.name == param)
						{
							result = asset;
						}
					}
				}
				else if (typeof(Avatar) == type)
				{
					result = AssetDatabase.LoadAssetAtPath<Avatar>(assetPath);
				}
				else if (typeof(Material) == type)
				{
					// fbx の場合、複数のMaterialがあるので名前で一致する物を探す
					var meshRenderer = AssetDatabase.LoadAssetAtPath<MeshRenderer>(assetPath);
					if (!ReferenceEquals(meshRenderer, null))
					{
						foreach (var material in meshRenderer.sharedMaterials)
						{
							if (material.name == param)
							{
								result = material;
								break;
							}
						}
					}
				}
				else
				{
					throw new NotImplementedException(string.Format("{0} の FindAsset() 未対応のType {1}", assetPath, type.Name));
				}

				if (!ReferenceEquals(result, null))
				{
					ReferenceState = ReferenceStateEnum.Found;
				}
			}
			else if(assetPath.EndsWith(".mat"))
			{
				if (typeof(Material) == type)
				{
					var material = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
					if (!ReferenceEquals(material, null))
					{
						result = material;
						ReferenceState = ReferenceStateEnum.Found;
					}
				}
				else
				{
					throw new NotImplementedException(string.Format("{0} の FindAsset() 未対応のType {1}", assetPath, type.Name));
				}
			}
			else if(assetPath.EndsWith(".controller"))
			{
				if (typeof(RuntimeAnimatorController) == type)
				{
					var rac = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(assetPath);
					if (!ReferenceEquals(rac, null))
					{
						result = rac;
						ReferenceState = ReferenceStateEnum.Found;
					}
				}
				else
				{
					throw new NotImplementedException(string.Format("{0} の FindAsset() 未対応のType {1}", assetPath, type.Name));
				}
			}
			else if (assetPath == "Library/unity default resources")
			{
				// PrimitiveType （Cube、Sphere など）の Mesh
				var primitiveType = (PrimitiveType)Enum.Parse(typeof(PrimitiveType), param);
				var primitive = GameObject.CreatePrimitive(primitiveType);
				if (!ReferenceEquals(primitive, null))
				{
					var meshFilter = primitive.GetComponent<MeshFilter>();
					if (!ReferenceEquals(meshFilter, null))
					{
						result = meshFilter.sharedMesh;
						//result = meshFilter.mesh;
						ReferenceState = ReferenceStateEnum.Found;
					}
					GameObject.DestroyImmediate(primitive);
				}
			}
			else
			{
				throw new NotImplementedException(string.Format("{0} の FindAsset() 未対応の拡張子", assetPath));
			}
			
			return result;

			/*
			if (!assetFound)
			{
				throw new Exception(string.Format("Asset not found. : {0} ({1}) : [ {2} ]", name, this.GetType(), assetPath));
			}
			*/
		}
	}

	[Serializable]
	public class JsonMesh : ISerializationCallbackReceiver
	{
		private Mesh m;

		public string name;
		public HideFlags hideFlags;

		public JsonAssetReference assetReference;
		
		public JsonMatrix4x4[] bindposes;
		public JsonBounds bounds;
		public Color[] colors;
		public Color32[] colors32;
		public JsonVector3[] normals;
		public JsonVector4[] tangents;
		public int[] triangles;
		public JsonVector2[] uv;
		public JsonVector2[] uv2;
		public JsonVector2[] uv3;
		public JsonVector2[] uv4;
		public JsonVector2[] uv5;
		public JsonVector2[] uv6;
		public JsonVector2[] uv7;
		public JsonVector2[] uv8;
		public JsonVector3[] vertices;
		public JsonBoneWeight[] boneWeights;
		public string indexFormat;
		public int subMeshCount;
		
		public JsonMesh(Mesh m)
		{
			this.m = m;

			this.name = m.name;
			this.hideFlags = m.hideFlags;
			
			var meshInstanceId = m.GetInstanceID();
			var meshAssetPath = AssetDatabase.GetAssetPath(meshInstanceId);
			if (0 < meshAssetPath.Length)
			{
				assetReference = new JsonAssetReference(meshAssetPath);
				return;
			}
			
			// fbx の AseetPathが取得できた場合はここを呼ばなくて済む

			this.bindposes = new JsonMatrix4x4[m.bindposes.Length];
			for (var i = 0; i < this.bindposes.Length; i++)
			{
				this.bindposes[i] = new JsonMatrix4x4(m.bindposes[i]);
			}
			
			this.bounds = new JsonBounds(m.bounds);
			this.colors = m.colors;
			this.colors32 = m.colors32;
			
			this.normals = new JsonVector3[m.normals.Length];
			for (var i = 0; i < this.normals.Length; i++)
			{
				this.normals[i] = new JsonVector3(m.normals[i]);
			}
			
			this.tangents = new JsonVector4[m.tangents.Length];
			for (var i = 0; i < this.tangents.Length; i++)
			{
				this.tangents[i] = new JsonVector4(m.tangents[i]);
			}
			
			this.triangles = new int[m.triangles.Length];
			Array.Copy(m.triangles, this.triangles, m.triangles.Length);
			
			this.uv = new JsonVector2[m.uv.Length];
			for (var i = 0; i < this.uv.Length; i++)
			{
				this.uv[i] = new JsonVector2(m.uv[i]);
			}
			this.uv2 = new JsonVector2[m.uv2.Length];
			for (var i = 0; i < this.uv2.Length; i++)
			{
				this.uv2[i] = new JsonVector2(m.uv2[i]);
			}
			this.uv3 = new JsonVector2[m.uv3.Length];
			for (var i = 0; i < this.uv3.Length; i++)
			{
				this.uv3[i] = new JsonVector2(m.uv3[i]);
			}
			this.uv4 = new JsonVector2[m.uv4.Length];
			for (var i = 0; i < this.uv4.Length; i++)
			{
				this.uv4[i] = new JsonVector2(m.uv4[i]);
			}
			this.uv5 = new JsonVector2[m.uv5.Length];
			for (var i = 0; i < this.uv5.Length; i++)
			{
				this.uv5[i] = new JsonVector2(m.uv5[i]);
			}
			this.uv6 = new JsonVector2[m.uv6.Length];
			for (var i = 0; i < this.uv6.Length; i++)
			{
				this.uv6[i] = new JsonVector2(m.uv6[i]);
			}
			this.uv7 = new JsonVector2[m.uv7.Length];
			for (var i = 0; i < this.uv7.Length; i++)
			{
				this.uv7[i] = new JsonVector2(m.uv7[i]);
			}
			this.uv8 = new JsonVector2[m.uv8.Length];
			for (var i = 0; i < this.uv8.Length; i++)
			{
				this.uv8[i] = new JsonVector2(m.uv8[i]);
			}
			
			this.vertices = new JsonVector3[m.vertices.Length];
			for (var i = 0; i < this.vertices.Length; i++)
			{
				this.vertices[i] = new JsonVector3(m.vertices[i]);
			}

			this.boneWeights = new JsonBoneWeight[m.boneWeights.Length];
			for (var i = 0; i < this.boneWeights.Length; i++)
			{
				this.boneWeights[i] = new JsonBoneWeight(m.boneWeights[i]);
			}

			this.indexFormat = m.indexFormat.ToString();
			this.subMeshCount = m.subMeshCount;
		}
		
		public Mesh value
		{
			get { return m;  }
		}
		
		public void OnBeforeSerialize ()
		{
			// nop
		}
	
		public void OnAfterDeserialize ()
		{
			if (ReferenceEquals(assetReference, null) || ReferenceEquals(assetReference.assetPath, null) || assetReference.assetPath.Length <= 0)
			{
				// fbx の AseetPathが取得できた場合はここを呼ばなくて済む
				// todo: 動的生成されたものであった場合、ここで復元が必要
				m = new Mesh();
			
				// todo: 続き				

				throw new NotImplementedException(string.Format("{0} の OnAfterDeserialize()", this.GetType()));
			}
			else
			{
				m = (Mesh) assetReference.FindAsset(typeof(Mesh), name);
			}
		}
	}

	[Serializable]
	public class JsonMaterial : ISerializationCallbackReceiver
	{
		private Material m;
		
		public string name;
		public HideFlags hideFlags;
		
		public JsonAssetReference assetReference;
		
		public Color color;
		public string shaderName;
		public bool enableInstancing;
		public string mainTextureName;
		
		public JsonMaterial(Material m)
		{
			this.m = m;

			this.name = m.name;
			this.hideFlags = m.hideFlags;

			var meshInstanceId = m.GetInstanceID();
			var meshAssetPath = AssetDatabase.GetAssetPath(meshInstanceId);
			if (0 < meshAssetPath.Length)
			{
				assetReference = new JsonAssetReference(meshAssetPath);
				return;
			}
			
			// fbx の AseetPathが取得できた場合はここを呼ばなくて済む
			
			this.color = m.color;
			
			if (!ReferenceEquals(m.shader, null))
			{
				this.shaderName = m.shader.name;
			}
			else
			{
				this.shaderName = null;
			}
			
			this.enableInstancing = m.enableInstancing;

			if (!ReferenceEquals(m.mainTexture, null))
			{
				this.mainTextureName = m.mainTexture.name;
			}
			else
			{
				this.mainTextureName = null;
			}
			
			// todo: 続き				

			throw new NotImplementedException(string.Format("{0} の コンストラクタ()", this.GetType()));
		}
		
		public Material value
		{
			get { return m;  }
		}
		
		public void OnBeforeSerialize ()
		{
			// nop
		}
	
		public void OnAfterDeserialize ()
		{
			if (ReferenceEquals(assetReference, null) || ReferenceEquals(assetReference.assetPath, null) || assetReference.assetPath.Length <= 0)
			{
				m = new Material(Shader.Find(this.shaderName));
				m.name = name;
				m.hideFlags = hideFlags;
				
				// todo: 続き				
				throw new NotImplementedException(string.Format("{0} の OnAfterDeserialize()", this.GetType()));
			}
			else
			{
				m = (Material) assetReference.FindAsset(typeof(Material), name);
			}
		}
	}
	
	[Serializable]
	public class JsonPhysicMaterial : ISerializationCallbackReceiver
	{
		private PhysicMaterial pm;

		public string assetPath;
		
		public string name;
		//public string tag;
		public HideFlags hideFlags;
		//public bool enabled;
		
		public float bounciness;
		public string bounceCombine;
		public float dynamicFriction;
		public string frictionCombine;
		public float staticFriction;
		
		public JsonPhysicMaterial(PhysicMaterial pm)
		{
			this.pm = pm;

			name = pm.name;
			//tag = pm.tag;
			hideFlags = pm.hideFlags;
			//enabled = pm.enabled;

			bounciness = pm.bounciness;
			bounceCombine = pm.bounceCombine.ToString();
			dynamicFriction = pm.dynamicFriction;
			frictionCombine = pm.frictionCombine.ToString();
			staticFriction = pm.staticFriction;
		}
		
		public PhysicMaterial value
		{
			get { return pm;  }
		}
		
		public void OnBeforeSerialize ()
		{
			// nop
		}
	
		public void OnAfterDeserialize ()
		{
			pm = new PhysicMaterial
			{
				hideFlags = hideFlags,
				name = name,
				bounceCombine = PhysicMaterialCombine.Average,
				bounciness = bounciness,
				dynamicFriction = dynamicFriction,
				frictionCombine = PhysicMaterialCombine.Average,
				staticFriction = staticFriction,
			};
			if (!ReferenceEquals(bounceCombine, null) && 0 < bounceCombine.Length)
			{
				pm.bounceCombine = (PhysicMaterialCombine)Enum.Parse(typeof(PhysicMaterialCombine), bounceCombine);
			}
			if (!ReferenceEquals(frictionCombine, null) && 0 < frictionCombine.Length)
			{
				pm.frictionCombine = (PhysicMaterialCombine)Enum.Parse(typeof(PhysicMaterialCombine), frictionCombine);
			}
		}
	}
	
	public class Utility
	{
		public static string ReadableFormattedString(string jsonString)
		{
			var readableFormattedSb = new StringBuilder();
			using (var cahrEnum = jsonString.GetEnumerator())
			{
				var tabUnit = "    "; // "\t"
				var insertDict = new Dictionary<int, string>();
				
				var indentLevel = 0;
				var needNewLine = false;
				var needIndent = false;
				
				var sb = new StringBuilder();
				
				cahrEnum.Reset();
				var position = -1;
				while (cahrEnum.MoveNext())
				{
					position++;

					var current = cahrEnum.Current;
					
					var beforeNewLine = (
						current == '}' ||
						current == ']'
					);
				
					var afterNewLine = (
						current == '{' ||
						current == '[' ||
						current == ','
					);

					if (needNewLine)
					{
						needNewLine = false;
						sb.AppendLine();
					}
					
					if (needIndent)
					{
						needIndent = false;
						for (var i = 0; i < indentLevel; i++)
						{
							sb.Append(tabUnit);
						}
					}

					if (
						current == '{' ||
						current == '['
					)
					{
						indentLevel++;
					}
					
					if (
						current == '}' ||
						current == ']'
					)
					{
						indentLevel--;
						if (indentLevel < 0)
						{
							indentLevel = 0;
						}
					}
					
					if (beforeNewLine)
					{
						sb.AppendLine();
						for (var i = 0; i < indentLevel; i++)
						{
							sb.Append(tabUnit);
						}
						//needIndent = true;
					}
					
					if (0 < sb.Length)
					{
						insertDict[position] = sb.ToString();
						sb.Clear();
					}
					
					if (afterNewLine)
					{
						//sb.AppendLine();
						needNewLine = true;
						needIndent = true;
					}
				}

								
				cahrEnum.Reset();
				position = -1;
				char before = char.MinValue;
				while (cahrEnum.MoveNext())
				{
					position++;
					var current = cahrEnum.Current;
					if (before == '[' && current == ']')
					{
						insertDict.Remove(position);
					}
					else if (before == '{' && current == '}')
					{
						insertDict.Remove(position);
					}
					before = current;
				}
				
				cahrEnum.Reset();
				position = -1;
				while (cahrEnum.MoveNext())
				{
					position++;

					if (insertDict.ContainsKey(position))
					{
						readableFormattedSb.Append(insertDict[position]);
					}

					readableFormattedSb.Append(cahrEnum.Current);
				}
			}

			return readableFormattedSb.ToString();
		}
	}
}