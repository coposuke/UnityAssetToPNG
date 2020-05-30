# UnityAssetToPNG

UnityAsset を PNG で出力するツールです。
UnityAsset は下記タイプに対応しています。
 - SpriteAtlas
 - Sprite (整列)
 - Texture2D

![Sample1](https://github.com/coposuke/UnityAssetToPNG/blob/image/Sample01.SpriteAtlas.png)  
![Sample2](https://github.com/coposuke/UnityAssetToPNG/blob/image/Sample02.Sprites.png)  
![Sample3](https://github.com/coposuke/UnityAssetToPNG/blob/image/Sample03.Sprites.gif, "Spritesを整列させてParticleにしたもの")  

## ■導入方法
### 方法１ 手動
`Clone or Download` から `Download ZIP` を選択してダウンロードしてください。  
解凍したら、Scriptsフォルダ を Assetsフォルダ の中に配置してください。

### 方法２ PackageManager
Packages/manifest.json に下記のように追記してください。
```
{
  "dependencies": {
    ...
    ...
    "com.copocopo.generater.pngfromunityasset": "https://github.com/coposuke/UnityAssetToPNG.git"
  }
}
```
追記後に Unity を起動すると自動的にインポートされます。

## ■使い方
Pngにしたいアセットを右クリックし、「Genarate PNG」を選択します。  
![使い方](https://github.com/coposuke/UnityAssetToPNG/blob/image/HowToUse.png)

## ■License
[CC0 v1.0](https://github.com/coposuke/UnityAssetToPNG/blob/master/LICENSE)

## ■License (Sample/Asset)
サンプルで使用しているアセットは、それぞれライセンスが異なることにご留意ください。

Sample/Assets/CopoCopo 以下は Unity Companion License に準じます  
"Sample/Assets/CopoCopo" licensed under the Unity Companion License  
[Unity Companion License](https://unity3d.com/jp/legal/licenses/Unity_Companion_License)

Sample/Assets/GoogleFonts 以下は SIL Open Font License に準じます  
"Sample/Assets/GoogleFonts" licensed under the SIL Open Font License  
[SIL Open Font License](https://scripts.sil.org/cms/scripts/page.php?site_id=nrsi&id=OFL)

Sample/Assets/NotoEmoji 以下は Apache License Version 2.0 に準じます  
"Sample/Assets/NotoEmoji" licensed under the Apache License Version 2.0  
[Apache License Version 2.0](https://github.com/coposuke/UnityAssetToPNG/blob/master/Sample%7E/Assets/NotoEmoji/LICENSE)
