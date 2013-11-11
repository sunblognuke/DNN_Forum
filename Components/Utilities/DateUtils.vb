Namespace DotNetNuke.Modules.Forum.Utilities
    Public Class DateUtils

        Public Shared Function LocalDateTime(ByVal utcTime As Date, ByVal timeZone As TimeZoneInfo) As Date
            Return TimeZoneInfo.ConvertTimeFromUtc(utcTime, timeZone)
        End Function

        Public Shared Function RelativeDate(ByVal dt As DateTime) As String
            Dim ts As New TimeSpan(DateTime.UtcNow.Ticks - dt.Ticks)
            Dim delta As Double = Math.Abs(ts.TotalSeconds)

            If delta < 60 Then
                Return If(ts.Seconds = 1, "one second ago", Convert.ToString(ts.Seconds) & " seconds ago")
            End If
            If delta < 120 Then
                Return "a minute ago"
            End If
            If delta < 2700 Then
                ' 45 * 60
                Return Convert.ToString(ts.Minutes) & " minutes ago"
            End If
            If delta < 5400 Then
                ' 90 * 60
                Return "an hour ago"
            End If
            If delta < 86400 Then
                ' 24 * 60 * 60
                Return Convert.ToString(ts.Hours) & " hours ago"
            End If
            If delta < 172800 Then
                ' 48 * 60 * 60
                Return "yesterday"
            End If
            If delta < 2592000 Then
                ' 30 * 24 * 60 * 60
                Return Convert.ToString(ts.Days) & " days ago"
            End If
            If delta < 31104000 Then
                ' 12 * 30 * 24 * 60 * 60
                Dim months As Integer = Convert.ToInt32(Math.Floor(CDbl(ts.Days) / 30))
                Return If(months <= 1, "one month ago", months & " months ago")
            End If
            Dim years As Integer = Convert.ToInt32(Math.Floor(CDbl(ts.Days) / 365))

            Return If(years <= 1, "one year ago", years & " years ago")
        End Function
    End Class
End Namespace

