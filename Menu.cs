using System;
using UnityEngine;
using UnityEditor;

namespace tkchJsonSerialize
{
	/// <summary>
	/// コンポーネントのコンテキストメニュー
	/// </summary>
	public class MenuEntry
	{
		/// <summary>
		/// Transform の情報を json 形式で出力
		/// （RectTransformも兼ねる）
		/// </summary>
		/// <param name="command">Command.</param>
		[MenuItem("CONTEXT/Transform/json_dump")]
		private static void Transform_Dump_Menu(MenuCommand command)
		{
			//Debug.Log("Transform_json_dump（Transform の情報を json 形式で出力）");
			try
			{
				Transform transform = (Transform)command.context;
				PopupJsonWindow.ShowDumpWindow(transform);
			}
			catch (Exception ex)
			{
				EditorUtility.DisplayDialog ("Custom Script Exception", ex.ToString(), "OK");
			}
		}
		
		/// <summary>
		/// json データから Transform の情報をリストア
		/// （RectTransformも兼ねる）
		/// </summary>
		/// <param name="command">Command.</param>
		[MenuItem("CONTEXT/Transform/json_restore")]
		private static void Transform_Restore_Menu(MenuCommand command)
		{
			//Debug.Log("jsonデータから Transform の情報をリストア");
			try
			{
				Transform transform = (Transform)command.context;
				/*
				PopupJsonWindow.ShowRestoreWindow(transform, (component, jsonObject) => {
					if (component.GetType() == typeof(Transform))
					{
						((JsonTransform) jsonObject).JsonRestore((Transform)component);
					}
				});
				*/
				PopupJsonWindow.ShowRestoreWindow(transform);

			}
			catch (Exception ex)
			{
				EditorUtility.DisplayDialog ("Custom Script Exception", ex.ToString(), "OK");
			}
		}
		
		/// <summary>
		/// Cloth の情報を json 形式で出力
		/// </summary>
		/// <param name="command">Command.</param>
		[MenuItem("CONTEXT/Cloth/json_dump")]
		private static void Cloth_Dump_Menu(MenuCommand command)
		{
			//Debug.Log("Cloth_json_dump（Cloth の情報を json 形式で出力）");
			try
			{
				Cloth cloth = (Cloth) command.context;
				PopupJsonWindow.ShowDumpWindow(cloth);
			}
			catch (Exception ex)
			{
				EditorUtility.DisplayDialog("Custom Script Exception", ex.ToString(), "OK");
			}
		}

		/// <summary>
		/// json データからCloth の情報をリストア
		/// </summary>
		/// <param name="command">Command.</param>
		[MenuItem("CONTEXT/Cloth/json_restore")]
		private static void Cloth_Restore_Menu(MenuCommand command)
		{
			//Debug.Log("jsonデータから Cloth の情報をリストア");
			try
			{
				Cloth cloth = (Cloth) command.context;
				PopupJsonWindow.ShowRestoreWindow(cloth);
			}
			catch (Exception ex)
			{
				EditorUtility.DisplayDialog("Custom Script Exception", ex.ToString(), "OK");
			}
		}
	}
}