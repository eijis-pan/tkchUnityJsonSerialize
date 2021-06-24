# tkchUnityJsonSerialize

UnityのComponent（今のところ Transform と Cloth）プロパティを json 形式で表示するエディタ拡張です。  

既知の不具合  
Clothへのリストアで不整合が発生する場合があります。  
・頂点数の違う別のオブジェクトへリストアした場合  
・編集したjsonデータをリストアした場合（配列の長さや頂点へのインデックスが不正な場合）  
（Clothへリストアする場合はClothコンポーネントを作り直した直後に行うのが無難です。）

開発環境 & 動作確認環境  
Unity 2018 4.20f1 (macOS 10.14.x)  

使い方  
Assets/Editor フォルダの下に配置する。  
Inspector 画面の Transform または Cloth コンポーネントの右上にある歯車メニューから json_dump を選ぶ。  
リストアする場合は json_restore を選び、テキストエリアに jsonデータ をペーストし、restore ボタンを押す。

その他  
_ _ json_dump_timestamp_ _ と _ _ cloth(transform)_json_dump_version _ _ というプロパティはこのエディタ拡張が付加したものです。  
コンポーネントに存在しないプロパティなのでリストア時は無視されます。
