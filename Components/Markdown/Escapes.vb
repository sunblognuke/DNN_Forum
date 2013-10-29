Imports System.Collections.Generic
Imports System.Text
Imports System.Text.RegularExpressions

Namespace MarkdownSharp
    Friend NotInheritable Class Escapes
        Private Sub New()
        End Sub
        Private Const _escapeCharacters As String = "\`*_{}[]()>#+-.!"
        Private Shared ReadOnly _escapeTable As KeyValuePair(Of Char, String)()
        Private Shared ReadOnly _hashFinder As Regex

        Shared Sub New()
            _escapeTable = New KeyValuePair(Of Char, String)(_escapeCharacters.Length - 1) {}
            Dim pattern As String = ""
            For i As Integer = 0 To _escapeCharacters.Length - 1
                Dim c As Char = _escapeCharacters(i)
                Dim hash As String = c.ToString().GetHashCode().ToString()
                _escapeTable(i) = New KeyValuePair(Of Char, String)(c, hash)

                If pattern <> "" Then
                    pattern += "|(" & hash & ")"
                Else
                    pattern += "(" & hash & ")"
                End If
            Next
            _hashFinder = New Regex(pattern, RegexOptions.Compiled Or RegexOptions.ExplicitCapture)
        End Sub

        ''' <summary>
        ''' Gets the escape code for a single character
        ''' </summary>
        Public Shared Function [get](ByVal c As Char) As String
            For Each pair As KeyValuePair(Of Char, String) In _escapeTable
                If pair.Key = c Then
                    Return pair.Value
                End If
            Next
            Throw New IndexOutOfRangeException("The requested character can not be escaped")
        End Function

        ''' <summary>
        ''' Gets the character that a hash refers to
        ''' </summary>
        Private Shared Function getInverse(ByVal s As String) As Char
            For Each pair As KeyValuePair(Of Char, String) In _escapeTable
                If pair.Value = s Then
                    Return pair.Key
                End If
            Next
            Throw New IndexOutOfRangeException("The requested hash can not be found")
        End Function

        ''' <summary>
        ''' Encodes any escaped characters such as \`, \*, \[ etc
        ''' </summary>
        Public Shared Function BackslashEscapes(ByVal text As String) As String
            Dim len As Integer = text.Length, first As Integer = 0, i As Integer = 0
            Dim sb As StringBuilder = New StringBuilder(len)
            While i < len
                If text(i) = "\"c AndAlso i + 1 < len AndAlso Contains(_escapeCharacters, text(i + 1)) Then
                    sb.Append(text, first, i - first)
                    sb.Append([get](text(System.Threading.Interlocked.Increment(i))))
                    first = System.Threading.Interlocked.Increment(i)
                Else
                    i += 1
                End If
            End While
            If first = 0 Then
                Return text
            End If
            sb.Append(text, first, i - first)
            Return sb.ToString()
        End Function

        ''' <summary>
        ''' Encodes Bold [ * ] and Italic [ _ ] characters
        ''' </summary>
        Public Shared Function BoldItalic(ByVal text As String) As String
            Dim len As Integer = text.Length, first As Integer = 0, i As Integer = 0
            Dim sb As StringBuilder = New StringBuilder(len)
            While i < len
                If "*"c = text(i) Then
                    sb.Append(text, first, i - first)
                    sb.Append([get]("*"c))
                    first = System.Threading.Interlocked.Increment(i)
                ElseIf "_"c = text(i) Then
                    sb.Append(text, first, i - first)
                    sb.Append([get]("_"c))
                    first = System.Threading.Interlocked.Increment(i)
                Else
                    i += 1
                End If
            End While
            If first = 0 Then
                Return text
            End If
            sb.Append(text, first, i - first)
            Return sb.ToString()
        End Function

        ''' <summary>
        ''' Encodes all chars of the second parameter.
        ''' </summary>
        Public Shared Function Escape(ByVal text As String, ByVal escapes As String) As String
            Dim len As Integer = text.Length, first As Integer = 0, i As Integer = 0
            Dim sb As StringBuilder = New StringBuilder(len)
            While i < len
                If Contains(escapes, text(i)) Then
                    sb.Append(text, first, i - first)
                    sb.Append([get](text(i)))
                    first = System.Threading.Interlocked.Increment(i)
                Else
                    i += 1
                End If
            End While
            If first = 0 Then
                Return text
            End If
            sb.Append(text, first, i - first)
            Return sb.ToString()
        End Function

        ''' <summary>
        ''' encodes problem characters in URLs, such as 
        ''' * _  and optionally ' () []  :
        ''' this is to avoid problems with markup later
        ''' </summary>
        Public Shared Function ProblemUrlChars(ByVal url As String) As String
            url = url.Replace("*", "%2A")
            url = url.Replace("_", "%5F")
            url = url.Replace("'", "%27")
            url = url.Replace("(", "%28")
            url = url.Replace(")", "%29")
            url = url.Replace("[", "%5B")
            url = url.Replace("]", "%5D")
            If url.Length > 7 AndAlso Contains(url.Substring(7), ":"c) Then
                ' replace any colons in the body of the URL that are NOT followed by 2 or more numbers
                url = url.Substring(0, 7) & Regex.Replace(url.Substring(7), ":(?!\d{2,})", "%3A")
            End If

            Return url
        End Function

        Private Shared Function Contains(ByVal s As String, ByVal c As Char) As Boolean
            Dim len As Integer = s.Length
            For i As Integer = 0 To len - 1
                If s(i) = c Then
                    Return True
                End If
            Next
            Return False
        End Function

        ''' <summary>
        ''' swap back in all the special characters we've hidden
        ''' </summary>
        Public Shared Function Unescape(ByVal text As String) As String
            Return _hashFinder.Replace(text, Function(match) getInverse(match.Value).ToString())
        End Function
    End Class
End Namespace