# tkchUnityJsonSerialize

UnityのComponent（今のところ Transform と Cloth）プロパティを json 形式で表示するエディタ拡張です。  

既知の不具合  
Clothへのリストアで不整合が発生する場合があります。  
・頂点数の違う別のオブジェクトへリストアした場合  
・編集したjsonデータをリストアした場合（配列の長さや頂点へのインデックスが不正な場合）  
