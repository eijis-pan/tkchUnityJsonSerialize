# tkchUnityJsonSerialize
====

UnityのComponent（今のところ Transform と Cloth）プロパティを json 形式で表示するエディタ拡張です。  

## Description

既知の不具合  
Clothへのリストアで不整合が発生する場合があります。  
・頂点数の違う別のオブジェクトへリストアした場合  
・編集したjsonデータをリストアした場合（配列の長さや頂点へのインデックスが不正な場合）  
（Clothへリストアする場合はClothコンポーネントを作り直した直後に行うのが無難です。）

その他  
_ _ json_dump_timestamp_ _ と _ _ cloth(transform)_json_dump_version _ _ というプロパティはこのエディタ拡張が付加したものです。  
コンポーネントに存在しないプロパティなのでリストア時は無視されます。

開発環境 & 動作確認環境  
Unity 2018 4.20f1 (macOS 10.14.x)  

## Install

Assets/Editor フォルダの下に配置する。  

## Usage

使い方  
Inspector 画面の Transform または Cloth コンポーネントの右上にある歯車メニューから json_dump を選ぶ。  
リストアする場合は json_restore を選び、テキストエリアに jsonデータ をペーストし、restore ボタンを押す。

## Author

github:[eijis](https://github.com/eijis-pan)  または twitter: @ eijis_pan

## Disclaimer

利用は自己責任でお願いします。<br>
本プログラムは、なんの欠陥もないという無制限の保証を行うものではありません。<br>
本プログラムに関する不具合修正や質問についてのお問い合わせもお受けできない場合があります。<br>
本プログラムの利用によって生じたあらゆる損害に対して、一切の責任を負いません。<br>
本プログラムの利用によって生じるいかなる問題についても、その責を負いません。
