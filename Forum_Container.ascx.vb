'
' DotNetNuke?- http://www.dotnetnuke.com
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

Imports DotNetNuke.Modules.Forum.Utilities
Imports DotNetNuke.Entities.Modules
Imports DotNetNuke.Entities.Modules.Actions
Imports DotNetNuke.Security.Permissions

Namespace DotNetNuke.Modules.Forum

    ''' <summary>
    ''' Loads the proper control to display to the end user based on various parameters and settings.
    ''' </summary>
    ''' <remarks>This is a 'Dispatch' control, only the UI folder classes are rendered here.</remarks>
    Public MustInherit Class Container
        Inherits ForumModuleBase
        Implements IActionable

#Region "Private Members"

        Private _GroupID As Integer = Null.NullInteger
        Private _ForumID As Integer = Null.NullInteger
        Private _ThreadID As Integer = Null.NullInteger
        Private _PostID As Integer = Null.NullInteger

#End Region

#Region "Optional Interfaces"

        ''' <summary>
        ''' Gets a list of module actions available to the user to provide it to DNN core.
        ''' </summary>
        ''' <value></value>
        ''' <returns>The collection of module actions available to the user</returns>
        ''' <remarks></remarks>
        Public ReadOnly Property ModuleActions() As ModuleActionCollection Implements IActionable.ModuleActions
            Get
                Return Utilities.ForumUtils.PerUserModuleActions(objConfig, Me)
            End Get
        End Property

#End Region

#Region "Event Handlers"

        ''' <summary>
        ''' Since this is a dispatch page, we need to set base properties and 
        ''' then load the proper view (Based on scope variable). This currently 
        ''' loads all non ascx views used throughout this module. It is important 
        ''' the base object be tied to the current moduleid, tabid. (Navigation)
        ''' </summary>
        ''' <param name="sender">The Object</param>
        ''' <param name="e">The event arguement.</param>
        ''' <remarks>The event arguement and the object are not used. ObjectID needs to be reconsidered.
        ''' </remarks>
        Protected Sub Page_Init(ByVal sender As Object, ByVal e As EventArgs) Handles MyBase.Init
            If DotNetNuke.Framework.AJAX.IsInstalled Then
                DotNetNuke.Framework.AJAX.RegisterScriptManager()
            End If

            If CheckQueryStringWithIntType("groupid") Then
                Integer.TryParse(Request.QueryString("groupid"), _GroupID)
            End If

            If CheckQueryStringWithIntType("forumid") Then
                Integer.TryParse(Request.QueryString("forumid"), _ForumID)
            End If

            If CheckQueryStringWithIntType("threadid") Then
                Integer.TryParse(Request.QueryString("threadid"), _ThreadID)
            End If

            'If Not (Request.QueryString("postid") Is Nothing) Then
            '    _PostID = Int32.Parse(Request.QueryString("postid"))
            'End If
            If CheckQueryStringWithIntType("postid") Then
                Integer.TryParse(Request.QueryString("postid"), _PostID)
            End If

            With DNNForum
                .PortalID = PortalSettings.PortalId
                .TabID = PortalSettings.ActiveTab.TabID
                .ModuleID = ModuleId
                .objConfig = objConfig
                .BasePage = CType(Me.Page, CDefault)
                .PortalName = PortalSettings.PortalName
                .LocalResourceFile = TemplateSourceDirectory & "/" & Localization.LocalResourceDirectory & "/" & Me.ID
                .TabModuleSettings = Settings

                Select Case DNNForum.ViewType
                    Case ForumScope.Groups
                        .GenericObjectID = ModuleId
                    Case ForumScope.Threads
                        .GenericObjectID = _ForumID
                    Case ForumScope.Posts
                        .GenericObjectID = _ThreadID
                        PrepareWmdResources()
                    Case ForumScope.ThreadSearch
                        .GenericObjectID = _ForumID
                    Case ForumScope.Unread
                        .GenericObjectID = ModuleId
                End Select
            End With

            ' Check out whether the url request is valid, otherwise just redirect to the forum home
            'If DNNForum.GenericObjectID > 0 Then
            'Else
            '    Response.Redirect(Utilities.Links.ContainerForumHome(TabId), False)
            'End If
        End Sub

        ''' <summary>
        ''' Determines if we need to redirect to this page again to change the scope, as well as sets the module actions 
        ''' </summary>
        ''' <param name="sender">The object.</param>
        ''' <param name="e">The event arguement being passed in.</param>
        ''' <remarks>We have to load the javascript files on every load of this control.</remarks>
        Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles MyBase.Load
            Try
                Dim DefaultPage As CDefault = DirectCast(Page, CDefault)
                ForumUtils.LoadCssFile(DefaultPage, objConfig)

                ' Redirect to user's default forum if user access forum via menu
                If (Not CType(Settings("defaultforumid"), String) Is Nothing) AndAlso
                   (DNNForum.ViewType = ForumScope.Groups) AndAlso
                   (Request.UrlReferrer Is Nothing) Then
                    If Not CType(Settings("defaultforumid"), Integer) = 0 Then
                        Response.Redirect(Utilities.Links.ContainerViewForumLink(TabId, CType(Settings("defaultforumid"), Integer), False), False)
                    End If
                End If

                'Set the Navigator Actions Collection
                For Each action As ModuleAction In ModuleActions
                    If action.CommandName = ModuleActionType.ContentOptions Then
                        If ModulePermissionController.HasModuleAccess(action.Secure, "Edit", ModuleConfiguration) Then
                            DNNForum.NavigatorActions.Add(action)
                        End If
                    End If
                Next
            Catch exc As Exception
                ProcessModuleLoadException(Me, exc)
            End Try
        End Sub

        Private Sub PrepareWmdResources()
            Dim litLinks As LiteralControl = New LiteralControl()
            With litLinks
                .ID = "wmdControl"
            End With

            Dim BasePage As CDefault = DirectCast(Page, CDefault)
            If BasePage.Header.FindControl("wmdControl") Is Nothing Then
                Dim sb As StringBuilder = New StringBuilder()

                Const js As String = "<script type=""text/javascript"" src=""{0}""></script>"
                Const css As String = "<link type=""text/css"" href=""{0}"" rel=""stylesheet"" />"
                Dim wmdFolder As String = "~/DesktopModules/Forum/Extensions/MarkDown/wmd"

                sb.Append(String.Format(css, ResolveUrl(wmdFolder & "/wmd.css")))
                sb.Append(String.Format(js, ResolveUrl(wmdFolder & "/jquery.wmd.min.js")))
                sb.Append(String.Format(js, ResolveUrl(wmdFolder & "/wmdStarter.js")))

                litLinks.Text = sb.ToString()
                BasePage.Header.Controls.Add(litLinks)
            End If
        End Sub

#End Region

    End Class

End Namespace