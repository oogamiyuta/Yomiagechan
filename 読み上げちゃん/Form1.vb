Imports System.Text.RegularExpressions
Imports AI.Talk.Editor.Api
Imports CeVIO.Talk.RemoteService2
Imports System.Media
Imports Newtonsoft.Json.Linq
Imports Microsoft.VisualBasic.Logging
Imports System.Net.Http
Imports System.Net
Imports System.Text
Imports System.IO
Imports System.Collections.Concurrent
Imports System.Net.Http.Headers

Public Class Form1

    Private Cevio_TtsControl As New Talker2
    Private AiVoce_TtsControl As New AI.Talk.Editor.Api.TtsControl

    Dim Cevio_State As SpeakingState2
    Dim Cevio_Talker_Name() As String            'PCにインストールされているCeVIO AI話者の名前一覧
    Dim Cevio_Lp As Integer                      'Longではダメ　必ずIntegerを使用する(相手のdllに合わせる)
    Dim Cast1 As String

    Dim VoiceVOX_httpClient As New System.Net.Http.HttpClient() 'ViceVoxのHTTP通信用クライアント

    ' 受信したデータを保存する変数
    Private Request_Type As String
    Private Request_Name As String
    Private Request_Component As String
    Private Request_Text As String


    ' VoiceVOXのスタイルIDを格納する辞書
    Private VoiceVOX_StyleDictionary As New Dictionary(Of String, Integer)

    ' リクエストを保存するキュー
    Private requestQueue As New ConcurrentQueue(Of JObject)

    Private Async Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        'エンジン別に非同期で読み込む
        Await AivoicePreset() 'A.I.Voice読込み

        Await CeVioPreset() 'CeVio読込み

        Await ViceVoxPreset() 'ViceVox読込み

        ' HTTPサーバーを開始
        StartHttpServer()


        ' リクエスト処理を開始
        ProcessRequestQueue()
        BufferRequestQueue()


        ' デバッグ用のフォームを表示
        'ShowDebugForm()
    End Sub


    ' デバッグ用のフォームを表示するメソッド
    Private Sub ShowDebugForm()
        Dim debugForm As New DebugForm()
        debugForm.ShowDialog()
    End Sub


    ' HTTPサーバーを開始するメソッド
    Private Sub StartHttpServer()
        Dim listener As New HttpListener()
        listener.Prefixes.Add("http://localhost:11451/list/")
        listener.Prefixes.Add("http://localhost:11451/input/")
        listener.Start()
        listener.BeginGetContext(AddressOf HandleRequest, listener)
    End Sub

    ' リクエストを処理するメソッド
    Private Async Sub HandleRequest(result As IAsyncResult)
        Dim listener As HttpListener = CType(result.AsyncState, HttpListener)
        Dim context As HttpListenerContext = listener.EndGetContext(result)
        Dim request As HttpListenerRequest = context.Request
        Dim response As HttpListenerResponse = context.Response

        If request.Url.AbsolutePath = "/list" Then
            ' DataGridViewの内容をJSON形式で取得
            Dim jsonResponse As String = GetJsonFromDataGridView()

            ' レスポンスのContentTypeとエンコーディングを設定
            response.ContentType = "application/json; charset=utf-8"
            response.ContentEncoding = Encoding.UTF8

            ' レスポンスを送信
            Dim buffer As Byte() = Encoding.UTF8.GetBytes(jsonResponse)
            response.ContentLength64 = buffer.Length
            response.OutputStream.Write(buffer, 0, buffer.Length)
            response.OutputStream.Close()
        ElseIf request.Url.AbsolutePath = "/input" Then
            ' リクエストの内容を読み取る
            Dim body As String
            Using reader As New StreamReader(request.InputStream, request.ContentEncoding)
                body = reader.ReadToEnd()
            End Using

            ' JSONデータを解析してキューに追加
            Dim jsonObject As JObject = JObject.Parse(body)
            requestQueue.Enqueue(jsonObject)

            ' レスポンスを送信
            response.StatusCode = 200
            response.StatusDescription = "OK"
            response.Close()
        End If

        ' 次のリクエストを待機
        listener.BeginGetContext(AddressOf HandleRequest, listener)
    End Sub

    ' リクエストキューを処理するメソッド
    Private Async Sub ProcessRequestQueue()
        While True
            Dim jsonObject As JObject
            If requestQueue.TryDequeue(jsonObject) Then
                Dim text As String = jsonObject("Text").ToString()
                Dim maxLength As Integer = 200
                Dim start As Integer = 0

                ' テキストが空ならスキップ
                If String.IsNullOrWhiteSpace(text) Then Continue While

                While start < text.Length
                    Dim endIndex As Integer = Math.Min(start + maxLength, text.Length)

                    ' 句読点やスペースの最適な切れ目を探す
                    Dim cutIndex As Integer = text.LastIndexOfAny(" 、。.,!?".ToCharArray(), endIndex - 1, endIndex - start)

                    ' 見つからなければスペースで区切る
                    If cutIndex < start Then cutIndex = text.LastIndexOf(" "c, endIndex - 1, endIndex - start)

                    ' それでもなければ最大長でカット
                    If cutIndex < start Then cutIndex = endIndex

                    ' DataGridViewに追加（UIスレッドで実行）
                    DataGridView2.Invoke(Sub()
                                             DataGridView2.Rows.Add(jsonObject("Type").ToString(),
                                                               jsonObject("Name").ToString(),
                                                               jsonObject("Component").ToString(),
                                                               text.Substring(start, cutIndex - start))
                                         End Sub)

                    ' 次の処理位置を設定（+1で無限ループ防止）
                    start = cutIndex + 1

                    ' 終端を超えたら抜ける
                    If start >= text.Length Then Exit While
                End While


            End If

            ' 少し待機してから次のリクエストを処理
            Await Task.Delay(100)
        End While
    End Sub


    ' DataGridViewの内容をJSON形式で返すメソッド
    Private Function GetJsonFromDataGridView() As String
        Dim jsonArray As New JArray()

        For Each row As DataGridViewRow In DataGridView1.Rows
            If Not row.IsNewRow Then
                Dim jsonObject As New JObject()
                jsonObject("Type") = row.Cells(0).Value.ToString()
                jsonObject("Name") = row.Cells(1).Value.ToString()
                jsonObject("Component") = row.Cells(2).Value.ToString()
                jsonArray.Add(jsonObject)
            End If
        Next

        Return jsonArray.ToString()
    End Function

    '-----------------------------------------------------

    'Preset読込み編

    'A.I.Voice編
    Private Async Function AivoicePreset() As Task
        AIvoiceStartCheck()
        '話者の名前を取得
        Dim AIvoice_Talker_Name() As String = AiVoce_TtsControl.VoicePresetNames
        '話者の人数分ループしてDataGridに話者の名前を追加
        For Each talkerName In AIvoice_Talker_Name
            DataGridView1.Rows.Add("A.I.Voice", Regex.Replace(talkerName, "\（.*?\）", ""), talkerName)
        Next

    End Function

    'Cevio編
    Private Async Function CeVioPreset() As Task
        CeVioStartCheck()
        '話者の名前を取得
        Cevio_Talker_Name = Cevio_TtsControl.AvailableCasts
        '話者の人数分ループして感情の名前を取得
        For Each talkerName In Cevio_Talker_Name
            '話者の感情の名前を取得

            Cevio_TtsControl.Cast = talkerName
            'dataGridに話者の名前と性格を追加
            For Each component In Cevio_TtsControl.Components
                DataGridView1.Rows.Add("Cevio AI", talkerName, component.Name)
            Next
        Next
    End Function

    'ViceVox編
    Private Async Function ViceVoxPreset() As Task
        Try
            Dim VoiceVOX_response As HttpResponseMessage = Await VoiceVOX_httpClient.GetAsync("http://localhost:50021/speakers")
            VoiceVOX_response.EnsureSuccessStatusCode()

            Dim VoiceVOX_jsonResponse As String = Await VoiceVOX_response.Content.ReadAsStringAsync()
            Dim VoiceVox_speakers As JArray = JArray.Parse(VoiceVOX_jsonResponse)

            For Each VoiceVOX_speaker As JObject In VoiceVox_speakers
                Dim VoiceVOX_name As String = VoiceVOX_speaker("name").ToString()
                Dim VoiceVOX_styles As JArray = VoiceVOX_speaker("styles")

                For Each VoiceVOX_style As JObject In VoiceVOX_styles
                    Dim VoiceVOX_styleName As String = VoiceVOX_style("name").ToString()
                    Dim VoiceVOX_styleId As Integer = VoiceVOX_style("id").ToObject(Of Integer)()
                    DataGridView1.Rows.Add("VoiceVox", VoiceVOX_name, VoiceVOX_styleName)

                    ' 辞書に追加（キーは名前+スタイル）
                    Dim key As String = VoiceVOX_name & "-" & VoiceVOX_styleName
                    If Not VoiceVOX_StyleDictionary.ContainsKey(key) Then
                        VoiceVOX_StyleDictionary.Add(key, VoiceVOX_styleId)
                    End If
                Next
            Next

        Catch ex As Exception
            MessageBox.Show("音声データの取得に失敗しました:" & ex.Message)
        End Try

    End Function

    '-----------------------------------------------------
    '起動確認編

    'Cevio編
    Private Sub CeVioStartCheck()
        'CeVIO AI Talk Editor を起動する
        'Falseを指定することで、起動していなければ起動し、起動していれば何もしない
        ServiceControl2.StartHost(False)

        While ServiceControl2.IsHostStarted = 0
            If ServiceControl2.IsHostStarted = -1 Then
                MsgBox("CeVIO AIのインストール状況が取得出来ません")
                Exit Sub
            ElseIf ServiceControl2.IsHostStarted = -2 Then
                MsgBox("CeVIO AIの実行ファイルが取得出来ません")
                Exit Sub
            ElseIf ServiceControl2.IsHostStarted = -3 Then
                MsgBox("CeVIO AIのプロセス起動が出来ませんでした")
                Exit Sub
            ElseIf ServiceControl2.IsHostStarted = -4 Then
                MsgBox("CeVIO AIの起動後にErrorが発生しました" & vbCrLf & "再起動しますか?", MsgBoxStyle.YesNo)
                If DialogResult = DialogResult.Yes Then
                    ServiceControl2.StartHost(True)
                Else
                    Exit Sub
                End If
            End If
            'CeVIO AI Talk Editorが起動しているか確認する
            '起動していればTrueを返す
            '起動していなければFalseを返す
            Task.Delay(1000)
        End While

    End Sub

    'A.I.Voice編
    Private Sub AIvoiceStartCheck()
        'A.I.VOICE起動
        Dim AIvoice_HostName() As String = AiVoce_TtsControl.GetAvailableHostNames()     'ホストネームをゲット

        If AIvoice_HostName.Length = 0 Then
            Exit Sub '1つも取得できなかった。アイボスがPCにインストールされてないんじゃね？エラー
        End If

        AiVoce_TtsControl.Initialize(AIvoice_HostName(0))

        Try
            ' A.I.VOICEの状態を確認
            Select Case AiVoce_TtsControl.Status
                Case HostStatus.Busy
                    Task.Delay(100) ' 100ミリ秒待機
                Case HostStatus.NotRunning
                    AiVoce_TtsControl.StartHost()
                Case HostStatus.NotConnected
                    AiVoce_TtsControl.Connect()
                Case HostStatus.Idle
                    ' 何もしない
            End Select

            While AiVoce_TtsControl.Status <> HostStatus.Idle
                Task.Delay(100) ' 100ミリ秒待機
            End While


        Catch ex As Exception
            MessageBox.Show("A.I.Voice接続エラー：" & ex.Message)
        End Try
    End Sub

    'ViceVox編



    '-----------------------------------------------------
    '接続切る処理群

    'Cevio編
    Private Sub CeVio_Disconnect()
        CeVIO.Talk.RemoteService2.ServiceControl2.CloseHost()

    End Sub


    'AI Voice編

    '-----------------------------------------------------
    ' アプリケーションのエントリポイント
    <STAThread()>
    Public Shared Sub Main()
        Application.EnableVisualStyles()
        Application.SetCompatibleTextRenderingDefault(False)
        Application.Run(New Form1())
    End Sub

    ' リクエストキューを処理するメソッド
    Private Async Sub BufferRequestQueue()
        While True
            ' DataGridView の先頭行が存在するかチェック
            If DataGridView2.Rows.Count > 0 Then
                ' 先頭行を取得
                Dim firstRow As DataGridViewRow = DataGridView2.Rows(0)
                Dim requestType As String = firstRow.Cells(0).Value.ToString()
                Dim requestName As String = firstRow.Cells(1).Value.ToString()
                Dim requestComponent As String = firstRow.Cells(2).Value.ToString()
                Dim requestText As String = firstRow.Cells(3).Value.ToString()

                ' ソフトウェアにリクエストを送信
                Await Software_Check(requestType, requestName, requestComponent, requestText)

                ' 送信完了後に先頭行を削除
                DataGridView2.Invoke(Sub()
                                         DataGridView2.Rows.RemoveAt(0)
                                     End Sub)
            End If

            ' 少し待機してから次のリクエストを処理
            Await Task.Delay(100)
        End While
    End Sub

    '-------------------------------------------------------------------------------------------------------------------------------------------------------------------
    ' 各ソフトにRequestを送る処理群
    '-------------------------------------------------------------------------------------------------------------------------------------------------------------------
    ' ソフト判定
    Private Async Function Software_Check(requestType As String, requestName As String, requestComponent As String, requestText As String) As Task
        If requestType = "Cevio AI" Then
            CeVio_Request(requestName, requestComponent, requestText)
        ElseIf requestType = "A.I.Voice" Then
            AIvoice_Request(requestComponent, requestText)
        ElseIf requestType = "VoiceVox" Then
            VoiceVox_RequestAsync(requestName, requestComponent, requestText)
        End If
    End Function

    ' A.I.Voice に Request を送る
    Private Sub AIvoice_Request(requestComponent As String, requestText As String)
        AIvoiceStartCheck()
        AiVoce_TtsControl.CurrentVoicePresetName = requestComponent
        AiVoce_TtsControl.Text = requestText
        AiVoce_TtsControl.Play()
    End Sub

    ' CeVio AI に Request を送る
    Private Sub CeVio_Request(requestName As String, requestComponent As String, requestText As String)
        CeVioStartCheck()

        With Cevio_TtsControl
            .Cast = requestName
            For i = 0 To .Components.Count - 1
                If .Components(i).Name = requestComponent Then
                    .Components(i).Value = 100
                Else
                    .Components(i).Value = 0
                End If
            Next
        End With

        If ServiceControl2.IsHostStarted = True Then
            Dim Cevio_State = Cevio_TtsControl.Speak(requestText)
            Cevio_State.Wait()
        End If
    End Sub

    ' VoiceVox に Request を送る
    Private Async Function VoiceVox_RequestAsync(requestName As String, requestComponent As String, requestText As String) As Task
        ' VoiceVox のリクエスト処理を実装（仮）
        ' ここに VoiceVox の API を呼び出す処理を追加する
        Dim styleId As Integer? = GetVoiceVOXStyleId(requestName, requestComponent)
        If styleId.HasValue Then

            Dim query As String = String.Empty
            Dim selectedSpeaker As String = requestName
            Dim speakerId As Integer = styleId
            Dim text As String = requestText
            Using request = New HttpRequestMessage(New HttpMethod("POST"), $"http://localhost:50021/audio_query?text={text}&speaker={speakerId}")
                request.Headers.TryAddWithoutValidation("accept", "application/json")
                request.Content = New StringContent("")
                request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded")

                Dim response = Await VoiceVOX_httpClient.SendAsync(request)
                query = Await response.Content.ReadAsStringAsync()
            End Using

            ' Step 2: クエリーからオーディオを合成する
            Dim RandomNum As New System.Random  '乱数生成器
            Dim RandomNumber As Integer = RandomNum.Next(1000000000)
            Dim tempFilePath As String = Path.Combine(Path.GetTempPath(), RandomNumber.ToString & "test.wav")
            Using request = New HttpRequestMessage(New HttpMethod("POST"), $"http://localhost:50021/synthesis?speaker={speakerId}&enable_interrogative_upspeak=true")
                request.Headers.TryAddWithoutValidation("accept", "audio/wav")
                request.Content = New StringContent(query)
                request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json")

                Dim response = Await VoiceVOX_httpClient.SendAsync(request)

                ' オーディオをファイルに保存する
                Using fileStream = File.Create(tempFilePath)
                    Using httpStream = Await response.Content.ReadAsStreamAsync()
                        httpStream.CopyTo(fileStream)
                        fileStream.Flush()
                    End Using
                End Using
            End Using

            ' オーディオを再生する
            Using player = New SoundPlayer(tempFilePath)
                player.PlaySync()
            End Using

            ' クリーンアップ
            File.Delete(tempFilePath)


        End If

    End Function
    Private Function GetVoiceVOXStyleId(name As String, style As String) As Integer?
        Dim key As String = name & "-" & style
        If VoiceVOX_StyleDictionary.ContainsKey(key) Then
            Return VoiceVOX_StyleDictionary(key)
        Else
            Return Nothing ' IDが見つからない場合
        End If
    End Function




End Class
