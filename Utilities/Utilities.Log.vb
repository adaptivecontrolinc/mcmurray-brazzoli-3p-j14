Partial Public NotInheritable Class Utilities
  Public NotInheritable Class Log

    Public Shared Sub LogError(ByVal message As String)
      Try
        Dim applicationPath As String = My.Application.Info.DirectoryPath
        Dim fileLocation As String = applicationPath & "\Log.txt"

        WriteToFile(fileLocation, Message)
      Catch Ex As Exception
        'Some code
      End Try
    End Sub

    Public Shared Sub LogError(ByVal message As String, ByVal detail As String)
      Try
        Dim applicationPath As String = My.Application.Info.DirectoryPath
        Dim fileLocation As String = applicationPath & "\Log.txt"

        WriteToFile(fileLocation, Message, detail)
      Catch Ex As Exception
        'Some code
      End Try
    End Sub

    Public Shared Sub LogError(ByVal err As Exception)
      Try
        Dim applicationPath As String = My.Application.Info.DirectoryPath
        Dim fileLocation As String = applicationPath & "\Log.txt"

        WriteToFile(fileLocation, err.Message, err.StackTrace)
      Catch Ex As Exception
        'Some code
      End Try
    End Sub

    Public Shared Sub LogError(ByVal err As Exception, ByVal sql As String)
      Try
        Dim applicationPath As String = My.Application.Info.DirectoryPath
        Dim fileLocation As String = applicationPath & "\Log.txt"

        WriteToFile(fileLocation, err.Message, err.StackTrace)
      Catch Ex As Exception
        'Some code
      End Try
    End Sub

    Public Shared Sub LogEvent(ByVal message As String)
      Try
        Dim applicationPath As String = My.Application.Info.DirectoryPath
        Dim fileLocation As String = applicationPath & "\Log.txt"

        WriteToFile(fileLocation, message)
      Catch ex As Exception
        'Some code
      End Try
    End Sub

    Private Shared Sub WriteToFile(ByVal fileLocation As String, ByVal message As String)
      Try
        Using sw As New System.IO.StreamWriter(fileLocation, True)
          sw.WriteLine()
          sw.WriteLine()
          sw.WriteLine(Date.Now.ToString("yyyy-MM-dd hh:mm:ss"))
          sw.WriteLine("-------------------")
          sw.WriteLine(message)
        End Using
      Catch ex As Exception
        'Some code
      End Try
    End Sub

    Private Shared Sub WriteToFile(ByVal fileLocation As String, ByVal message As String, ByVal detail As String)
      Try
        Using sw As New System.IO.StreamWriter(fileLocation, True)
          sw.WriteLine()
          sw.WriteLine()
          sw.WriteLine(Date.Now.ToString("yyyy-MM-dd hh:mm:ss"))
          sw.WriteLine("-------------------")
          sw.WriteLine(message)
          sw.WriteLine(detail)
        End Using
      Catch ex As Exception
        'Some code
      End Try
    End Sub

    Private Shared Sub WriteToFile(ByVal fileLocation As String, ByVal message As String, ByVal detail As String, ByVal sql As String)
      Try
        Using sw As New System.IO.StreamWriter(fileLocation, True)
          sw.WriteLine()
          sw.WriteLine()
          sw.WriteLine(Date.Now.ToString("yyyy-MM-dd hh:mm:ss"))
          sw.WriteLine("-------------------")
          sw.WriteLine(message)
          sw.WriteLine(detail)
          sw.WriteLine(sql)
        End Using
      Catch ex As Exception
        'Some code
      End Try
    End Sub

  End Class
End Class