Imports System.IO.Ports
Imports Sharp7

Public Class Form1
    Dim plc As New S7Client()

    'Arayüz ve Donanım
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        Label6.Text = "Adet: 0" ' YERLİ ÜRETİM
        Label7.Text = "Adet: 0" ' İTHAL ÜRÜN
        Label5.Text = "Adet: 0" ' YERLİ MONTAJ


        Label8.Text = "İletim Durumu: Bekleniyor..."

        Try
            Dim plcSonuc As Integer = plc.ConnectTo("192.168.2.219", 0, 1)

            If Not SerialPort1.IsOpen Then
                SerialPort1.PortName = "COM5"
                SerialPort1.BaudRate = 115200
                SerialPort1.NewLine = vbCr
                SerialPort1.Open()
            End If

            If plcSonuc = 0 AndAlso SerialPort1.IsOpen Then
                Label9.Text = "Bağlantı Durumu: Sistem Hazır (PLC ve COM5 Aktif)"
                Label9.ForeColor = Color.Green
            Else
                Label9.Text = "Bağlantı Durumu: PLC veya COM5 Bağlantısında Sorun Var!"
                Label9.ForeColor = Color.Orange
            End If

        Catch ex As Exception
            MessageBox.Show("Başlatma Hatası: " & ex.Message, "Kritik Uyarı")
        End Try
    End Sub

    'Barkod okuma ve sınıflandırma
    Private Sub SerialPort1_DataReceived(sender As Object, e As SerialDataReceivedEventArgs) Handles SerialPort1.DataReceived
        Try
            Dim gelenBarkod As String = SerialPort1.ReadLine().Trim()

            Me.Invoke(Sub()

                          TextBox1.Text = gelenBarkod

                          'Sınıflandırma şartları
                          If gelenBarkod.StartsWith("8689") OrElse gelenBarkod.StartsWith("8699") Then
                              ListBox1.Items.Add(gelenBarkod)
                              Label6.Text = "Adet: " & ListBox1.Items.Count.ToString()



                          ElseIf gelenBarkod.StartsWith("8682") Then
                              ListBox3.Items.Add(gelenBarkod)
                              Label5.Text = "Adet: " & ListBox3.Items.Count.ToString()

                          Else
                              ListBox2.Items.Add(gelenBarkod)
                              Label7.Text = "Adet: " & ListBox2.Items.Count.ToString()
                          End If

                          ' Plc veri aktarımı
                          If plc.Connected Then
                              ' Stringe çevirme
                              Dim stringUzunlugu As Byte = CByte(gelenBarkod.Length)
                              Dim s7StringData((2 + stringUzunlugu) - 1) As Byte

                              ' İlk iki bayt boyut bilgisidir
                              s7StringData(0) = 254 ' Toplam kapasite
                              s7StringData(1) = stringUzunlugu ' Gerçek uzunluk

                              ' Barkod metnini ASCII'ye çevirip 2. bayttan itibaren pakete ekle
                              System.Text.Encoding.ASCII.GetBytes(gelenBarkod).CopyTo(s7StringData, 2)

                              ' Veriyi DB2'nin ilk offset değerine gönderir
                              Dim yazmaSonucu As Integer = plc.DBWrite(2, 0, s7StringData.Length, s7StringData)

                              If yazmaSonucu = 0 Then
                                  Label8.Text = "İletim Durumu: [" & gelenBarkod & "] DB2'ye Yazıldı!"
                                  Label8.ForeColor = Color.Green
                              Else
                                  Label8.Text = "İletim Durumu: PLC Yazma Hatası (Kod: " & yazmaSonucu & ")"
                                  Label8.ForeColor = Color.Red
                              End If
                          Else
                              Label8.Text = "İletim Durumu: Başarısız (PLC'ye Ulaşılamıyor)"
                              Label8.ForeColor = Color.Red
                          End If
                      End Sub)

        Catch ex As IO.IOException

        Catch ex As Exception

        End Try
    End Sub

    ' Kapatma güvenliği
    Private Sub Form1_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        If SerialPort1.IsOpen Then SerialPort1.Close()
        If plc.Connected Then plc.Disconnect()
    End Sub

    Private Sub ListBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ListBox1.SelectedIndexChanged

    End Sub
End Class