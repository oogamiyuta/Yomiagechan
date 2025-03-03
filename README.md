## VoiceVOX・A.I.VOICE・Cevio AI中間ソフト
発信方法はDebugForm参考に  

APIは  
http://localhost:11451/list  
に利用可能なVoiceプリセットをjson形式で発信  
http://localhost:11451/input  
にjsonリクエストを送信  
VoiceVOX以外は各ソフトで読み上げ  
VoiceVOXは乱数+test.wavでtempフォルダーに作成後、再生し、wavを削除  

## 開発環境
- Windows11 PRO 24H2
- VS 2022
- .NET Framework 4.8.1
