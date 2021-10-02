using System.Collections.Generic;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;

namespace tkchJsonSerialize
{
	/// <summary>
	/// エディター拡張ウィンドウ（dump、resotreをそれぞれ１つだけ管理）
	/// Component 用
	/// </summary>
	public class PopupJsonWindow : EditorWindow
	{
		private static PopupJsonWindow _singleDumpWindow;	// dump ウィンドウ
		private static PopupJsonWindow _singleRestoreWindow;// restore ウィンドウ
		
		private const float LabelWidth = 100f;
		private const float ButtonHeight = 30f;
		private const int TextPadding = 5;
		
		private bool _writeable = false;
		//private Action<Component, JsonComponentBase> _restoreAction = null;
		private Component _component = null;
		private bool _errorOnCheck = false;
		private bool _componentMissingReference = false;
		private string _componentTypeName = string.Empty;
		private string _objectName = string.Empty;
		private Vector2 _scroll;
		private bool _firstFocused = false;
		private string _text = string.Empty;
		private string _defaultText = string.Empty;
		private string _readableText = null;
		private JsonComponentBase _jsonObject = null;
		private List<HelpBoxInfo> _helpBoxInfos = new List<HelpBoxInfo>(3);

		private bool _readableFormatting = true;
		
		private bool _needDelayUpdate = false;
		private const float UpdateDelay = 0.01f;
		private float _lastEditorUpdateTime = float.NaN;

		private Dictionary<CheckResult.ResultType, MessageType> CheckResultTypeToMessageType
			= new Dictionary<CheckResult.ResultType, MessageType>()
			{
				{CheckResult.ResultType.Info, MessageType.Info},
				{CheckResult.ResultType.Warning, MessageType.Warning},
				{CheckResult.ResultType.Error, MessageType.Error}
			};

		private static string GetJsonDump(Component component)
		{
			var jsonComponent = JsonComponentBase.CreateJsonComponent(component);
			return jsonComponent.JsonDump();
		}
		
		/// <summary>
		/// dump ウィンドウを表示
		/// </summary>
		/// <param name="component"></param>
		public static void ShowDumpWindow(Component component)
		{
			var window = GetWindow(false, new GUIContent("json dump"), component);
			window._text = window._defaultText = GetJsonDump(component);
			window.ShowUtility();
		}
		
		/// <summary>
		/// restore ウィンドウを表示
		/// </summary>
		/// <param name="component"></param>
		/// <param name="callback"></param>
		public static void ShowRestoreWindow(Component component /*, Action<Component, object> callback = null */ )
		{
			var window = GetWindow(true, new GUIContent("json restore"), component);
			/*
			if (ReferenceEquals(callback, null))
			{
				window._restoreAction = JsonComponentBase.DefaultRestoreAction;
			}
			else
			{
				window._restoreAction = callback;
			}
			*/
			window.ShowUtility();
		}
		
		private static PopupJsonWindow GetWindow(bool writable, GUIContent editorTitle, Component component)
		{
			var window = writable ? PopupJsonWindow._singleRestoreWindow : PopupJsonWindow._singleDumpWindow;
			if (null == window)
			{
				window = (PopupJsonWindow)ScriptableObject.CreateInstance<PopupJsonWindow>();
				if (writable)
				{
					PopupJsonWindow._singleRestoreWindow = window;
				}
				else
				{
					PopupJsonWindow._singleDumpWindow = window;
				}
			}

			window._writeable = writable;
			window.titleContent = editorTitle;
			window._component = component;
			window._componentTypeName = component.GetType().Name;
			window._objectName = component.name;
			window._firstFocused = false;
			window.Focus();
			return window;
		}
		
		private class HelpBoxInfo
		{
			public string message;
			public MessageType messageType;
			public bool wide;

			public HelpBoxInfo(string message, MessageType messageType, bool wide)
			{
				this.message = message;
				this.messageType = messageType;
				this.wide = wide;
			}
		}
		
		private void OnGUI()
		{
			/*
			if (ReferenceEquals(_component, null)) // null == _component よりも早いが Missing Reference を検出できないことがある
			{
				_componentMissingReference = true;
			}
			*/
			
			// コンポーネントの Missing Reference 検出用
			bool componentMissingReferencePrevValue = _componentMissingReference;
			try
			{
				if (0 == _component.name.Length)
				{
					_componentMissingReference = true;
				}
			}
			catch (Exception ex)
			{
				_componentMissingReference = true;
			}

			EditorGUILayout.Space();
			
			using (new EditorGUILayout.HorizontalScope())
			{
				GUILayout.Label ("Component : ", GUILayout.Width(LabelWidth));
				GUI.SetNextControlName("ComponentType");
				EditorGUILayout.SelectableLabel(_componentTypeName);
			}
			
			using (new EditorGUILayout.HorizontalScope())
			{
				GUILayout.Label ("GameObject : ", GUILayout.Width(LabelWidth));
				EditorGUILayout.SelectableLabel(_objectName);
			}
			
			// dump 画面の Reload ボタン、Copy ボタンと メッセージ領域
			if (!_writeable)
			{
				using (new EditorGUILayout.HorizontalScope())
				{
					if (GUILayout.Button("Reload",GUILayout.Height(ButtonHeight)))
					{
						if (_componentMissingReference)
						{
							_text = _defaultText = string.Empty;
						}
						else
						{
							_defaultText = GetJsonDump(_component);
							if (_readableFormatting)
							{
								_text = _readableText = Utility.ReadableFormattedString(_defaultText);
							}
							else
							{
								_text = _defaultText;
							}
							_helpBoxInfos.Clear();
						}
					}

					//using (new EditorGUI.DisabledScope(string.IsNullOrWhiteSpace(_text)))
					using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(_text)))
					{
						if (GUILayout.Button("Copy to clipboard",GUILayout.Height(ButtonHeight)))
						{
							GUIUtility.systemCopyBuffer = _text;
							_helpBoxInfos.Add(new HelpBoxInfo("copied to clipboard.", MessageType.Info, true));
						}
					}
				}
				if (!componentMissingReferencePrevValue && _componentMissingReference)
				{
					EditorGUILayout.HelpBox(string.Format("The object of type '{0}' has been destroyed.", _componentTypeName), MessageType.Error, true);
					_componentTypeName = _objectName = String.Empty;
				}
				else
				{
					foreach (var helpBoxInfo in _helpBoxInfos)
					{
						EditorGUILayout.HelpBox(helpBoxInfo.message, helpBoxInfo.messageType, helpBoxInfo.wide);
					}
				}

				using (var scope = new EditorGUI.ChangeCheckScope())
				{
					GUI.SetNextControlName("Toggle");
					bool checkOn = GUILayout.Toggle(_readableFormatting, "Readable formatting");
					if (scope.changed || !_firstFocused)
					{
						if (checkOn)
						{
							_readableFormatting = true;
							if (ReferenceEquals(_readableText, null))
							{
								_readableText = Utility.ReadableFormattedString(_defaultText);
								_helpBoxInfos.Clear();
							}
							_text = _readableText;
							GUI.FocusControl("Toggle");
						}
						else
						{
							_readableFormatting = false;
							if (!ReferenceEquals(_defaultText, _text))
							{
								_text = _defaultText;
								_helpBoxInfos.Clear();
							}
							GUI.FocusControl("Toggle");
						}
					}
				}
			}

			bool restored = false;
			Exception exception = null;

			using (new EditorGUILayout.VerticalScope(GUILayout.ExpandHeight(true)))
			{
				RectOffset textPaddingRect = new RectOffset(TextPadding, TextPadding, TextPadding, TextPadding);
				using (var scrollView = new EditorGUILayout.ScrollViewScope(_scroll))
				{
					_scroll = scrollView.scrollPosition;
					
					GUI.SetNextControlName("TextField");
					using (var scope = new EditorGUI.ChangeCheckScope())
					{
						var guiStyle = GUI.skin.textArea;
						guiStyle.padding = textPaddingRect;
						guiStyle.wordWrap = true;
						_text = EditorGUILayout.TextArea(_text, guiStyle,GUILayout.ExpandHeight(true));
						if (scope.changed)
						{
							if (_writeable)
							{
								//if (!string.IsNullOrWhiteSpace(_text) && 0 < _componentTypeName.Length)
								if (!string.IsNullOrEmpty(_text) && 0 < _componentTypeName.Length)
								{
									bool jsonError = false;
									_helpBoxInfos.Clear();
									_jsonObject = null;
									_errorOnCheck = false;
									try
									{
										_jsonObject = JsonComponentBase.FromJson(_component, _text);
									}
									catch (NotImplementedException ex)
									{
										jsonError = true;
										_helpBoxInfos.Add(new HelpBoxInfo("Not implement component type.", MessageType.Error, true));
										//Debug.Log(string.Format("Invalid json format.   {0}", ex));
									}
									catch (Exception ex)
									{
										jsonError = true;
										_helpBoxInfos.Add(new HelpBoxInfo("Invalid json format.", MessageType.Error, true));
										//Debug.Log(string.Format("Invalid json format.   {0}", ex));
									}
									
									if (!jsonError)
									{
										if (null == _jsonObject)
										{
											jsonError = true;
											_helpBoxInfos.Add(new HelpBoxInfo("Empty json object.", MessageType.Error, true));
										}
										else
										{
											// 参照の見つからないものがあれば警告を表示する
											if (_jsonObject.IsReferenceNotFound)
											{
												_helpBoxInfos.Add(new HelpBoxInfo("Reference not found.", MessageType.Warning, true));
											}
											
											// インスペクタのリロード（再表示が必要か）
											if (_jsonObject.NeedInspectorReload)
											{
												// 今のところ Cloth のエディタに更新が必要なケースのみ
												_needDelayUpdate = true;
											}
											else
											{
												_needDelayUpdate = false;
											}

											// その他の整合成チェック
											var checkResults = _jsonObject.AfterCheckFromJson(_component);
											foreach (var checkResult in checkResults)
											{
												_helpBoxInfos.Add(new HelpBoxInfo(
													string.Format("{0} Item=[ {1} ]", checkResult.Message, checkResult.ItemName), 
													CheckResultTypeToMessageType[checkResult.Type], true));

												if (CheckResult.ResultType.Error == checkResult.Type)
												{
													_errorOnCheck = true;
												}
											}
										}
									}
								}
							}
							else
							{
								_text = _defaultText;
								GUIUtility.keyboardControl = 0;
							}
						}
					}
				}

				// Restore 画面の メッセージ領域と Restore ボタン
				if (_writeable)
				{
					/*
					if (ReferenceEquals(_restoreAction, null))
					{
						EditorGUILayout.HelpBox("RESTORE ACTION MISSNG !!!", MessageType.Warning, true);
					}
					*/
					
					if (!componentMissingReferencePrevValue && _componentMissingReference)
					{
						EditorGUILayout.HelpBox(string.Format("The object of type '{0}' has been destroyed.", _componentTypeName), MessageType.Error, true);
						_componentTypeName = _objectName = String.Empty;
					}
					//if (string.IsNullOrWhiteSpace(_text))
					else if (string.IsNullOrEmpty(_text))
					{
						EditorGUILayout.HelpBox("please input json text.", MessageType.Info, true);
					}
					else
					{
						foreach (var helpBoxInfo in _helpBoxInfos)
						{
							EditorGUILayout.HelpBox(helpBoxInfo.message, helpBoxInfo.messageType, helpBoxInfo.wide);
						}
					}

					using (new EditorGUILayout.HorizontalScope())
					{
						using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(_text) || ReferenceEquals(_jsonObject, null) || 0 == _componentTypeName.Length || _componentMissingReference || _errorOnCheck))
						{
							if (GUILayout.Button("Restore",GUILayout.Height(ButtonHeight)))
							{
								_helpBoxInfos.Clear();
									
								int dlgRet = EditorUtility.DisplayDialogComplex (
									"json restore",
									"Component properties will override. are you sure ?",
									"Go", // 0 ok
									"Cancel", // 1 cancel
									null
								);
								if (0 == dlgRet)
								{
									restored = true;
									try
									{
										//_restoreAction(_component, _jsonObject);
										_jsonObject.JsonRestore(_component);
									}
									catch (Exception ex)
									{
										exception = ex;
									}
								}
							}
						}
						if (GUILayout.Button("Close",GUILayout.Height(ButtonHeight)))
						{
							Close();
						}
					}
				}
				else
				{
					if (GUILayout.Button("Close",GUILayout.Height(ButtonHeight)))
					{
						Close();
					}
				}
				
				EditorGUILayout.Space();
			}
			
			if (restored)
			{
				if (null == exception)
				{
					EditorUtility.SetDirty(_component);
					EditorUtility.DisplayDialog ("json restore", "restore complete.", "OK");
				}
				else if (0 < _helpBoxInfos.Count)
				{
					EditorUtility.SetDirty(_component);
					EditorUtility.DisplayDialog ("json restore", "restore finish with warning.", "OK");
				}
				else
				{
					EditorUtility.DisplayDialog ("json restore", "restore failed. \n \n" + exception.ToString(), "OK");
				}

				if (_needDelayUpdate)
				{
					// Inspector を更新
					Selection.activeObject = null; // 一旦選択解除させる
					_lastEditorUpdateTime = Time.time;
				}
			}
			
			if (!_firstFocused)
			{
				_firstFocused = true;
				//GUI.FocusControl("ComponentType");
				GUI.FocusControl("TextField");
			}

			if (_needDelayUpdate)
			{
				if (!float.IsNaN(_lastEditorUpdateTime))
				{
					if (_lastEditorUpdateTime + UpdateDelay < Time.time)
					{
						_lastEditorUpdateTime = float.NaN;
						Selection.activeObject = _component;
					}
				}
			}
		}
	}
	
	/// <summary>
	/// エディター拡張ウィンドウ（dumpのみ）
	/// World用
	/// </summary>
	public class DumpJsonWindow : EditorWindow
	{
		private static DumpJsonWindow _singleWindow;
			
		private const float ButtonHeight = 30f;
		private const int TextPadding = 5;
		
		private Vector2 _scroll;
		private bool _firstFocused = false;
		private string _text = string.Empty;
		private string _defaultText = string.Empty;
		private string _readableText = null;
		private object _jsonObject = null;
		private List<HelpBoxInfo> _helpBoxInfos = new List<HelpBoxInfo>(3);

		private bool _readableFormatting = false;
		
		/// <summary>
		/// dump ウィンドウを表示
		/// </summary>
		/// <param name="component"></param>
		public static void ShowDumpWindow(string jsonText)
		{
			var window = GetWindow(new GUIContent("json dump"));
			window._text = window._defaultText = jsonText;
			window.ShowUtility();
		}
		
		private static DumpJsonWindow GetWindow(GUIContent editorTitle)
		{
			var window = DumpJsonWindow._singleWindow;
			if (null == window)
			{
				window = (DumpJsonWindow)ScriptableObject.CreateInstance<DumpJsonWindow>();
				DumpJsonWindow._singleWindow = window;
			}

			window.titleContent = editorTitle;
			window._firstFocused = false;
			window.Focus();
			return window;
		}
		
		private class HelpBoxInfo
		{
			public string message;
			public MessageType messageType;
			public bool wide;

			public HelpBoxInfo(string message, MessageType messageType, bool wide)
			{
				this.message = message;
				this.messageType = messageType;
				this.wide = wide;
			}
		}
		
		private void OnGUI()
		{
			EditorGUILayout.Space();
			
			// dump 画面の Copy ボタン、Close ボタンと メッセージ領域
			using (new EditorGUILayout.HorizontalScope())
			{
				using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(_text)))
				{
					if (GUILayout.Button("Copy to clipboard",GUILayout.Height(ButtonHeight)))
					{
						GUIUtility.systemCopyBuffer = _text;
						_helpBoxInfos.Add(new HelpBoxInfo("copied to clipboard.", MessageType.Info, true));
					}
				}
				
				if (GUILayout.Button("Close",GUILayout.Height(ButtonHeight)))
				{
					Close();
				}
			}
			foreach (var helpBoxInfo in _helpBoxInfos)
			{
				EditorGUILayout.HelpBox(helpBoxInfo.message, helpBoxInfo.messageType, helpBoxInfo.wide);
			}

			if (GUILayout.Toggle(_readableFormatting, "Readable formatting"))
			{
				_readableFormatting = true;
				if (ReferenceEquals(_readableText, null))
				{
					_readableText = Utility.ReadableFormattedString(_defaultText);
					_helpBoxInfos.Clear();
				}
				_text = _readableText;
			}
			else
			{
				_readableFormatting = false;
				if (!ReferenceEquals(_defaultText, _text))
				{
					_text = _defaultText;
					_helpBoxInfos.Clear();
				}
			}
			
			using (new EditorGUILayout.VerticalScope(GUILayout.ExpandHeight(true)))
			{
				RectOffset textPaddingRect = new RectOffset(TextPadding, TextPadding, TextPadding, TextPadding);
				using (var scrollView = new EditorGUILayout.ScrollViewScope(_scroll))
				{
					_scroll = scrollView.scrollPosition;

					GUI.SetNextControlName("TextField");
					using (var scope = new EditorGUI.ChangeCheckScope())
					{
						var guiStyle = GUI.skin.textArea;
						guiStyle.padding = textPaddingRect;
						guiStyle.wordWrap = true;
						_text = EditorGUILayout.TextArea(_text, guiStyle, GUILayout.ExpandHeight(true));
					}
				}
			}

			if (!_firstFocused)
			{
				_firstFocused = true;
				//GUI.FocusControl("ComponentType");
				GUI.FocusControl("TextField");
			}
		}
	}
	
	/// <summary>
	/// エディター拡張ウィンドウ（resotreのみ）
	/// World用
	/// </summary>
	public class RestoreJsonWindow : EditorWindow
	{
		private static RestoreJsonWindow _singleWindow;
		
		private const float LabelWidth = 100f;
		private const float ButtonHeight = 30f;
		private const int TextPadding = 5;
		
		private Func<string, JsonRoot> _jsonAction = null;
		private Action<JsonRoot> _restoreAction = null;
		private bool _errorOnCheck = false;
		private Vector2 _scroll;
		private bool _firstFocused = false;
		private string _text = string.Empty;
		private string _defaultText = string.Empty;
		private JsonRoot _jsonObject = null;
		private List<HelpBoxInfo> _helpBoxInfos = new List<HelpBoxInfo>(3);
		
		private bool _defaultStateChange = false;
		//private bool _needDelayUpdate = false;
		private const float UpdateDelay = 0.01f;
		private float _lastEditorUpdateTime = float.NaN;

		private Dictionary<CheckResult.ResultType, MessageType> CheckResultTypeToMessageType
			= new Dictionary<CheckResult.ResultType, MessageType>()
			{
				{CheckResult.ResultType.Info, MessageType.Info},
				{CheckResult.ResultType.Warning, MessageType.Warning},
				{CheckResult.ResultType.Error, MessageType.Error}
			};

		private Dictionary<RestoreResult.ResultType, MessageType> RestoreResultTypeToMessageType
			= new Dictionary<RestoreResult.ResultType, MessageType>()
			{
				{RestoreResult.ResultType.Info, MessageType.Info},
				{RestoreResult.ResultType.Warning, MessageType.Warning},
				{RestoreResult.ResultType.Error, MessageType.Error}
			};
		
		/// <summary>
		/// restore ウィンドウを表示
		/// </summary>
		/// <param name="component"></param>
		/// <param name="callback"></param>
		public static void ShowRestoreWindow( 
			string defaultText,
			Func<string, JsonRoot> jsonCallback,
			Action<JsonRoot> restoreCallback 
			)
		{
			var window = GetWindow(new GUIContent("json restore"));
			window._text = window._defaultText = defaultText;
			window._defaultStateChange = !string.IsNullOrEmpty(defaultText);
			window._jsonAction = jsonCallback;
			window._restoreAction = restoreCallback;
			window.ShowUtility();
		}
		
		private static RestoreJsonWindow GetWindow(GUIContent editorTitle)
		{
			var window = RestoreJsonWindow._singleWindow;
			if (null == window)
			{
				window = (RestoreJsonWindow)ScriptableObject.CreateInstance<RestoreJsonWindow>();
				RestoreJsonWindow._singleWindow = window;
			}

			window.titleContent = editorTitle;
			window._firstFocused = false;
			window.Focus();
			return window;
		}
		
		private class HelpBoxInfo
		{
			public string message;
			public MessageType messageType;
			public bool wide;

			public HelpBoxInfo(string message, MessageType messageType, bool wide)
			{
				this.message = message;
				this.messageType = messageType;
				this.wide = wide;
			}
		}
		
		private void OnGUI()
		{
			EditorGUILayout.Space();
			
			bool restored = false;
			Exception exception = null;

			using (new EditorGUILayout.VerticalScope(GUILayout.ExpandHeight(true)))
			{
				RectOffset textPaddingRect = new RectOffset(TextPadding, TextPadding, TextPadding, TextPadding);
				using (var scrollView = new EditorGUILayout.ScrollViewScope(_scroll))
				{
					_scroll = scrollView.scrollPosition;
					
					GUI.SetNextControlName("TextField");
					using (var scope = new EditorGUI.ChangeCheckScope())
					{
						var guiStyle = GUI.skin.textArea;
						guiStyle.padding = textPaddingRect;
						guiStyle.wordWrap = true;
						_text = EditorGUILayout.TextArea(_text, guiStyle,GUILayout.ExpandHeight(true));
						if (scope.changed || _defaultStateChange)
						{
							_defaultStateChange = false;
							if (!string.IsNullOrEmpty(_text))
							{
								bool jsonError = false;
								_helpBoxInfos.Clear();
								_jsonObject = null;
								_errorOnCheck = false;
								try
								{
									_jsonObject = _jsonAction(_text);
								}
								catch (NotImplementedException ex)
								{
									jsonError = true;
									_helpBoxInfos.Add(new HelpBoxInfo("Not implement component type.", MessageType.Error, true));
									//Debug.Log(string.Format("Invalid json format.   {0}", ex));
								}
								catch (Exception ex)
								{
									jsonError = true;
									_helpBoxInfos.Add(new HelpBoxInfo("Invalid json format.", MessageType.Error, true));
									//Debug.Log(string.Format("Invalid json format.   {0}", ex));
								}
								
								if (!jsonError)
								{
									if (null == _jsonObject)
									{
										jsonError = true;
										_helpBoxInfos.Add(new HelpBoxInfo("Empty json object.", MessageType.Error, true));
									}
									else
									{
										/*
										// 参照の見つからないものがあれば警告を表示する
										if (_jsonObject.IsReferenceNotFound)
										{
											_helpBoxInfos.Add(new HelpBoxInfo("Reference not found.", MessageType.Warning, true));
										}
										
										// インスペクタのリロード（再表示が必要か）
										if (_jsonObject.NeedInspectorReload)
										{
											// 今のところ Cloth のエディタに更新が必要なケースのみ
											_needDelayUpdate = true;
										}
										else
										{
											_needDelayUpdate = false;
										}

										// その他の整合成チェック
										var checkResults = _jsonObject.AfterCheckFromJson(_component);
										foreach (var checkResult in checkResults)
										{
											_helpBoxInfos.Add(new HelpBoxInfo(
												string.Format("{0} Item=[ {1} ]", checkResult.Message, checkResult.ItemName), 
												CheckResultTypeToMessageType[checkResult.Type], true));

											if (CheckResult.ResultType.Error == checkResult.Type)
											{
												_errorOnCheck = true;
											}
										}
										*/
									}
								}
							}
						}
					}
				}

				if (string.IsNullOrEmpty(_text))
				{
					EditorGUILayout.HelpBox("please input json text.", MessageType.Info, true);
				}
				else
				{
					int count = 0;
					foreach (var helpBoxInfo in _helpBoxInfos)
					{
						EditorGUILayout.HelpBox(helpBoxInfo.message, helpBoxInfo.messageType, helpBoxInfo.wide);
						count++;
						if (5 <= count)
						{
							break;
						}
					}
					if (count < _helpBoxInfos.Count)
					{
						EditorGUILayout.HelpBox(string.Format("and more infomation. ({0})", _helpBoxInfos.Count - count), MessageType.None);
					}
				}

				using (new EditorGUILayout.HorizontalScope())
				{
					using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(_text) || ReferenceEquals(_jsonObject, null) || _errorOnCheck))
					{
						if (GUILayout.Button("Restore",GUILayout.Height(ButtonHeight)))
						{
							_helpBoxInfos.Clear();
								
							int dlgRet = EditorUtility.DisplayDialogComplex (
								"json restore",
								"Hierarchy objects will override. are you sure ?",
								"Go", // 0 ok
								"Cancel", // 1 cancel
								null
							);
							if (0 == dlgRet)
							{
								restored = true;
								exception = null;
								try
								{
									_restoreAction(_jsonObject);
								}
								catch (Exception ex)
								{
									exception = ex;
								}

								var restoreResults = _jsonObject.GetRestoreErrorList();
								foreach (var restoreResult in restoreResults)
								{
									_helpBoxInfos.Add(new HelpBoxInfo(
										string.Format("{0} Item=[ {1} ]", restoreResult.Message, restoreResult.ItemName), 
										RestoreResultTypeToMessageType[restoreResult.Type], true));
								}
							}
						}
					}
					if (GUILayout.Button("Close",GUILayout.Height(ButtonHeight)))
					{
						Close();
					}
				}
				
				EditorGUILayout.Space();
			}
			
			if (restored)
			{
				if (0 < _helpBoxInfos.Count)
				{
					EditorUtility.DisplayDialog ("json restore", "restore finish with warning.", "OK");
				}
				else if (ReferenceEquals(exception, null))
				{
					EditorUtility.DisplayDialog ("json restore", "restore complete.", "OK");
				}
				else
				{
					EditorUtility.DisplayDialog ("json restore", "restore failed. \n \n" + exception.ToString(), "OK");
				}

				/*
				if (_needDelayUpdate)
				{
					// Inspector を更新
					Selection.activeObject = null; // 一旦選択解除させる
					_lastEditorUpdateTime = Time.time;
				}
				*/
			}
			
			if (!_firstFocused)
			{
				_firstFocused = true;
				//GUI.FocusControl("ComponentType");
				GUI.FocusControl("TextField");
			}

			/*
			if (_needDelayUpdate)
			{
				if (!float.IsNaN(_lastEditorUpdateTime))
				{
					if (_lastEditorUpdateTime + UpdateDelay < Time.time)
					{
						_lastEditorUpdateTime = float.NaN;
						Selection.activeObject = _component;
					}
				}
			}
			*/
		}
	}
}