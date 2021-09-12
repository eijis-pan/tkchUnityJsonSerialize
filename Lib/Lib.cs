using System.Collections.Generic;
using System;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace tkchJsonSerialize
{
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
			Unnecessary,
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