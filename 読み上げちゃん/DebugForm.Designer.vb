<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class DebugForm
    Inherits System.Windows.Forms.Form

    'フォームがコンポーネントの一覧をクリーンアップするために dispose をオーバーライドします。
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Windows フォーム デザイナーで必要です。
    Private components As System.ComponentModel.IContainer

    'メモ: 以下のプロシージャは Windows フォーム デザイナーで必要です。
    'Windows フォーム デザイナーを使用して変更できます。  
    'コード エディターを使って変更しないでください。
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.ComboBoxType = New System.Windows.Forms.ComboBox()
        Me.ComboBoxName = New System.Windows.Forms.ComboBox()
        Me.ComboBoxComponent = New System.Windows.Forms.ComboBox()
        Me.TextBox1 = New System.Windows.Forms.TextBox()
        Me.Button1 = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        '
        'ComboBoxType
        '
        Me.ComboBoxType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.ComboBoxType.FormattingEnabled = True
        Me.ComboBoxType.Location = New System.Drawing.Point(12, 12)
        Me.ComboBoxType.Name = "ComboBoxType"
        Me.ComboBoxType.Size = New System.Drawing.Size(776, 21)
        Me.ComboBoxType.TabIndex = 0
        '
        'ComboBoxName
        '
        Me.ComboBoxName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.ComboBoxName.FormattingEnabled = True
        Me.ComboBoxName.Location = New System.Drawing.Point(12, 39)
        Me.ComboBoxName.Name = "ComboBoxName"
        Me.ComboBoxName.Size = New System.Drawing.Size(776, 21)
        Me.ComboBoxName.TabIndex = 1
        '
        'ComboBoxComponent
        '
        Me.ComboBoxComponent.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.ComboBoxComponent.FormattingEnabled = True
        Me.ComboBoxComponent.Location = New System.Drawing.Point(12, 66)
        Me.ComboBoxComponent.Name = "ComboBoxComponent"
        Me.ComboBoxComponent.Size = New System.Drawing.Size(776, 21)
        Me.ComboBoxComponent.TabIndex = 2
        '
        'TextBox1
        '
        Me.TextBox1.Location = New System.Drawing.Point(12, 93)
        Me.TextBox1.Multiline = True
        Me.TextBox1.Name = "TextBox1"
        Me.TextBox1.Size = New System.Drawing.Size(776, 342)
        Me.TextBox1.TabIndex = 3
        '
        'Button1
        '
        Me.Button1.Location = New System.Drawing.Point(713, 441)
        Me.Button1.Name = "Button1"
        Me.Button1.Size = New System.Drawing.Size(75, 23)
        Me.Button1.TabIndex = 4
        Me.Button1.Text = "Send"
        Me.Button1.UseVisualStyleBackColor = True
        '
        'DebugForm
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 12.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(800, 476)
        Me.Controls.Add(Me.Button1)
        Me.Controls.Add(Me.TextBox1)
        Me.Controls.Add(Me.ComboBoxComponent)
        Me.Controls.Add(Me.ComboBoxName)
        Me.Controls.Add(Me.ComboBoxType)
        Me.Name = "DebugForm"
        Me.Text = "DebugForm"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents ComboBoxType As ComboBox
    Friend WithEvents ComboBoxName As ComboBox
    Friend WithEvents ComboBoxComponent As ComboBox
    Friend WithEvents TextBox1 As TextBox
    Friend WithEvents Button1 As Button
End Class

