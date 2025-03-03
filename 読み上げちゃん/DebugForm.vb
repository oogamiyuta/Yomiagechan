Imports System.Net.Http
Imports System.Text
Imports Newtonsoft.Json.Linq

Public Class DebugForm

    Private ReadOnly httpClient As New HttpClient()
    Private jsonData As JArray

    Private Async Sub DebugForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' /list エンドポイントからデータを取得
        Dim response As HttpResponseMessage = Await httpClient.GetAsync("http://localhost:11451/list")
        response.EnsureSuccessStatusCode()

        Dim jsonResponse As String = Await response.Content.ReadAsStringAsync()
        jsonData = JArray.Parse(jsonResponse)

        ' ComboBoxTypeにTypeを格納
        Dim types = jsonData.Select(Function(j) j("Type").ToString()).Distinct()
        For Each type In types
            ComboBoxType.Items.Add(type)
        Next
    End Sub

    Private Sub ComboBoxType_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBoxType.SelectedIndexChanged
        ' ComboBoxNameをクリア
        ComboBoxName.Items.Clear()
        ComboBoxComponent.Items.Clear()

        ' 選択されたTypeに基づいてNameを絞り込み
        Dim selectedType As String = ComboBoxType.SelectedItem.ToString()
        Dim names = jsonData.Where(Function(j) j("Type").ToString() = selectedType).Select(Function(j) j("Name").ToString()).Distinct()
        For Each Name_ In names
            ComboBoxName.Items.Add(Name_)
        Next
    End Sub

    Private Sub ComboBoxName_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBoxName.SelectedIndexChanged
        ' ComboBoxComponentをクリア
        ComboBoxComponent.Items.Clear()

        ' 選択されたTypeとNameに基づいてComponentを絞り込み
        Dim selectedType As String = ComboBoxType.SelectedItem.ToString()
        Dim selectedName As String = ComboBoxName.SelectedItem.ToString()
        Dim components = jsonData.Where(Function(j) j("Type").ToString() = selectedType AndAlso j("Name").ToString() = selectedName).Select(Function(j) j("Component").ToString()).Distinct()
        For Each component In components
            ComboBoxComponent.Items.Add(component)
        Next
    End Sub

    Private Async Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        ' ComboBoxで選択したKeyとTextBoxの内容を送信
        Dim selectedType As String = ComboBoxType.SelectedItem.ToString()
        Dim selectedName As String = ComboBoxName.SelectedItem.ToString()
        Dim selectedComponent As String = ComboBoxComponent.SelectedItem.ToString()
        Dim textValue As String = TextBox1.Text

        Dim jsonObject As New JObject()
        jsonObject("Type") = selectedType
        jsonObject("Name") = selectedName
        jsonObject("Component") = selectedComponent
        jsonObject("Text") = textValue

        Dim content As New StringContent(jsonObject.ToString(), Encoding.UTF8, "application/json")
        Dim response As HttpResponseMessage = Await httpClient.PostAsync("http://localhost:11451/input", content)
        response.EnsureSuccessStatusCode()
    End Sub

End Class
