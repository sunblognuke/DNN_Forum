'
' DotNetNuke® - http://www.dotnetnuke.com
' Copyright (c) 2002-2011
' by DotNetNuke Corporation
'
' Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
' documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
' the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
' to permit persons to whom the Software is furnished to do so, subject to the following conditions:
'
' The above copyright notice and this permission notice shall be included in all copies or substantial portions 
' of the Software.
'
' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
' TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
' THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
' CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
' DEALINGS IN THE SOFTWARE.
'
Option Strict On
Option Explicit On

Imports DotNetNuke.Entities.Modules
Imports DotNetNuke.Entities.Users

Namespace DotNetNuke.Modules.Forum

    ''' <summary>
    ''' This class should be inherited by all controls that are registred as actions in DNN that should inherit PortalModuleBase. 
    ''' </summary>
    ''' <remarks>The settings class, since it normally doesn't load PortalModuleBase, should not inherit this class.</remarks>
    Public MustInherit Class ForumModuleBase
        Inherits PortalModuleBase

#Region "Public ReadOnly Properties"

        ''' <summary>
        ''' This is the user who is viewing the forum.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property CurrentForumUser() As ForumUserInfo
            Get
                Dim cntForumUser As New ForumUserController
                Return cntForumUser.GetForumUser(UserController.GetCurrentUserInfo.UserID, False, ModuleId, PortalId)
            End Get
        End Property

        ''' <summary>
        ''' This is the user who's profile we are altering (typically, the logged in user unless a forum admin).
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property ProfileUserID() As Integer
            Get
                Dim _ProfileUserID As Integer = Null.NullInteger
                'If Request.QueryString("userid") IsNot Nothing Then
                If CheckQueryStringWithIntType("userid") Then
                    Integer.TryParse(Request.QueryString("userid"), _ProfileUserID)
                    Return _ProfileUserID
                Else
                    Return CurrentForumUser.UserID
                End If
            End Get
        End Property

        ''' <summary>
        ''' The current forum the user is viewing. 
        ''' </summary>
        ''' <value></value>
        ''' <returns>An integer representing the current forum the usre is viewing. It will return -1 if there is no forumid parameter in the URL.</returns>
        ''' <remarks>The module depends heavily on this always being available in the URL. However, this is typically not used for security reasons.</remarks>
        Public Overridable ReadOnly Property ForumID() As Integer
            Get
                Dim _forumID As Integer = Null.NullInteger
                'If (Request.QueryString("forumid") IsNot Nothing) AndAlso 
                'Integer.TryParse(Request.QueryString("forumid"), _forumID) Then
                If CheckQueryStringWithIntType("forumid") Then
                    Integer.TryParse(Request.QueryString("forumid"), _forumID)
                    Return _forumID
                Else
                    ' maybe something else is in the url
                    Return -1
                End If
            End Get
        End Property

        ''' <summary>
        ''' This is the forum's configuration so it can be used by loaded controls.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property objConfig() As Forum.Configuration
            Get
                Return Configuration.GetForumConfig(ModuleId)
            End Get
        End Property

#End Region

        Public Overridable Function CheckQueryStringWithIntType(ByVal key As String) As Boolean
            Return Request.QueryString(key) IsNot Nothing AndAlso Regex.IsMatch(Request.QueryString(key), "^\d+$")
        End Function

    End Class

End Namespace