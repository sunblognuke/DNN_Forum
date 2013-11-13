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

Namespace DotNetNuke.Modules.Forum

    ''' <summary>
    ''' This is the initial view seen by the forums module. (Group View)
    ''' All rendering is done in code to create UI or code is called from here (in utilities, for example). 
    ''' </summary>
    ''' <remarks>
    ''' </remarks>
    Public Class Groups
        Inherits ForumObject

#Region "Private Declarations"

        Dim _AuthorizedGroups As List(Of GroupInfo)
        Dim _AuthForumsCount As Integer = 0

#End Region

#Region "Properties"

        ''' <summary>
        ''' This is the selected group. When a group is selected, only its forums will be displayed (and not other groups). 
        ''' </summary>
        Private ReadOnly Property SelectedGroupID() As Integer
            Get
                Dim groudID As Integer = 0
                If CheckQueryStringWithIntType("groupid") Then
                    Int32.TryParse(HttpContext.Current.Request.QueryString("groupid"), groudID)
                End If

                Return groudID

                '' get the group specification ( if it exists )
                'Dim GroupID As Integer = 0
                'If CType(ForumControl.TabModuleSettings("groupid"), String) <> String.Empty Then
                '    If CType(ForumControl.TabModuleSettings("groupid"), Integer) = 0 Then
                '        ' We know here group feature (parent/child) is not being used, check for url set item to show single group
                '        If SelectedGroupID > 0 Then
                '            ' assign a specific groupid so we only show a single group
                '            GroupID = SelectedGroupID
                '        End If
                '    Else
                '        GroupID = CType(ForumControl.TabModuleSettings("groupid"), Integer)
                '    End If
                'Else    ' from else to end of if is correct
                '    ' We know here group feature (parent/child) is not being used, check for url set item to show single group
                '    If SelectedGroupID > 0 Then
                '        ' assign a specific groupid so we only show a single group
                '        GroupID = SelectedGroupID
                '    End If
                'End If
            End Get
        End Property

        ''' <summary>
        ''' This is the selected parent forum (since we render child forums, like forums, within this group view). 
        ''' </summary>
        ''' <remarks>Results in similar view as selected group.</remarks>
        Private ReadOnly Property SelectedForumID() As Integer
            Get
                Dim forumID As Integer = 0
                If CheckQueryStringWithIntType("forumid") Then
                    Int32.TryParse(HttpContext.Current.Request.QueryString("forumid"), forumID)
                End If

                Return forumID
            End Get
        End Property

        ''' <summary>
        ''' Collection of groups that contain at least one authorized forum for the current user.
        ''' </summary>
        Private Property AuthorizedGroups() As List(Of GroupInfo)
            Get
                Return _AuthorizedGroups
            End Get
            Set(ByVal Value As List(Of GroupInfo))
                _AuthorizedGroups = Value
            End Set
        End Property

        ''' <summary>
        ''' This is the total number of forums the end user is authorized to view (used for total # count).
        ''' </summary>
        Private Property AuthorizedForumsCount() As Integer
            Get
                Return _AuthForumsCount
            End Get
            Set(ByVal Value As Integer)
                _AuthForumsCount = Value
            End Set
        End Property

#End Region

#Region "Public Methods"

#Region "Contructors"

        ''' <summary>
        ''' Creates a new instance of this class.
        ''' </summary>
        Public Sub New(ByVal forum As DNNForum)
            MyBase.New(forum)
        End Sub

#End Region

        ''' <summary>
        ''' Creates all controls like drop down lists, image buttons, etc.
        ''' This also uses defacto standards from the module's settings
        ''' </summary>
        Public Overrides Sub CreateChildControls()
            Controls.Clear()

            ' Get groups
            Dim cntGroup As New GroupController
            AuthorizedGroups = cntGroup.GroupGetAllAuthorized(ModuleID, CurrentForumUser.UserID, False, TabID)
            AuthorizedForumsCount = 0

            If AuthorizedGroups.Count > 0 Then
                For Each objGroup As GroupInfo In AuthorizedGroups
                    Dim authForums As New List(Of ForumInfo)
                    authForums = cntGroup.AuthorizedForums(CurrentForumUser.UserID, objGroup.GroupID, False, ModuleID, TabID)
                    If authForums.Count > 0 Then
                        AuthorizedForumsCount += authForums.Count
                    End If
                Next
            End If
        End Sub

        ''' <summary>
        ''' This renders the initial UI seen in the module (by calling other private 
        ''' methods.  This is the group view.  It also checks first to see if only a 
        ''' specific group should be shown based
        ''' on TabModuleSettings.
        ''' </summary>
        Public Overrides Sub Render(ByVal wr As HtmlTextWriter)
            RenderTableBegin(wr, 0, 0, "tblGroup") ' <table>

            Dim objGroupCnt As New GroupController
            Dim arrGroups As List(Of GroupInfo) = objGroupCnt.GroupsGetByModuleID(ModuleID)
            If arrGroups.Count > 0 Then
                RenderNavBar(wr, objConfig, ForumControl)
                RenderSpacerRow(wr)
                RenderBreadCrumbRow(wr)
                RenderSpacerRow(wr)
                RenderForums(wr)
                RenderForumFooter(wr)
            Else
                'No Groups are configured for this module
                RenderCellBegin(wr, "Forum_NavBarButton", "", "", "", "", "", "") ' <td> 
                RenderDivBegin(wr, "Config", "NormalRed") ' <div>
                wr.Write(ForumControl.LocalizedText("ForumContainsNothing"))
                RenderDivEnd(wr) ' </div>
                RenderCellEnd(wr) ' </Td>
            End If

            RenderTableEnd(wr) '</table>
        End Sub

#End Region

#Region "Private Methods"

        Private Sub RenderBreadCrumbRow(ByVal wr As HtmlTextWriter)
            RenderRowBegin(wr) '<tr>

            RenderCellBegin(wr, "", "", "100%", "left", "", "", "")
            RenderTableBegin(wr, "", "", "", "100%", "0", "0", "", "", "0")
            RenderRowBegin(wr) '<tr>
            RenderCapCell(wr, objConfig.GetThemeImageURL("spacer.gif"), "", "")

            Dim ChildGroupView As Boolean = False
            If CType(ForumControl.TabModuleSettings("groupid"), String) <> String.Empty Then
                ChildGroupView = True
            End If

            '[Skeel] add support for full breadcrumb on groupview and parentforum view
            If SelectedForumID > 0 And SelectedGroupID > 0 Then
                'Parent Forum view
                Dim cltForum As New ForumController
                Dim objForumInfo As ForumInfo
                objForumInfo = cltForum.GetForumItemCache(SelectedForumID)

                RenderCellBegin(wr, "Forum_BreadCrumb", "", "", "left", "", "", "")
                wr.Write(Utilities.ForumUtils.BreadCrumbs(TabID, ModuleID, ForumScope.Groups, objForumInfo, objConfig, ChildGroupView))
                RenderCellEnd(wr) ' </td>
            ElseIf SelectedGroupID > 0 Then
                'Group view
                Dim cltGroups As New GroupController
                Dim objGroupInfo As GroupInfo
                objGroupInfo = cltGroups.GroupGet(SelectedGroupID)

                RenderCellBegin(wr, "Forum_BreadCrumb", "", "", "left", "", "", "")
                wr.Write(Utilities.ForumUtils.BreadCrumbs(TabID, ModuleID, ForumScope.Groups, objGroupInfo, objConfig, ChildGroupView))
                RenderCellEnd(wr) ' </td>
            Else
                'Forum Home view
                'wr.Write(Utilities.ForumUtils.BreadCrumbs(TabID, ModuleID, ForumScope.Groups, Nothing, objConfig, ChildGroupView))

                RenderCellBegin(wr, "Forum_LastPostText", "", "", "right", "", "", "") ' <td>
                RenderStatistics(wr)
                RenderCellEnd(wr) ' </td>
            End If

            RenderCapCell(wr, objConfig.GetThemeImageURL("spacer.gif"), "", "")
            RenderRowEnd(wr) ' </tr>
            RenderTableEnd(wr) ' </table>
            RenderCellEnd(wr) ' </td>
            RenderRowEnd(wr) ' </tr>
        End Sub

        Private Sub RenderStatistics(ByVal wr As HtmlTextWriter)
            'View latest x hours
            Dim url As String
            wr.Write(Localization.GetString("ViewLatest", objConfig.SharedResourceFile) & " ")
            url = Utilities.Links.ContainerViewLatestHoursLink(TabID, 6)
            RenderLinkButton(wr, url, Localization.GetString("6", objConfig.SharedResourceFile), "Forum_LastPostText", "", False, objConfig.NoFollowLatestThreads)
            wr.Write(", ")
            url = Utilities.Links.ContainerViewLatestHoursLink(TabID, 12)
            RenderLinkButton(wr, url, Localization.GetString("12", objConfig.SharedResourceFile), "Forum_LastPostText", "", False, objConfig.NoFollowLatestThreads)
            wr.Write(", ")
            url = Utilities.Links.ContainerViewLatestHoursLink(TabID, 24)
            RenderLinkButton(wr, url, Localization.GetString("24", objConfig.SharedResourceFile), "Forum_LastPostText", "", False, objConfig.NoFollowLatestThreads)
            wr.Write(", ")
            url = Utilities.Links.ContainerViewLatestHoursLink(TabID, 48)
            RenderLinkButton(wr, url, Localization.GetString("48", objConfig.SharedResourceFile), "Forum_LastPostText", "", False, objConfig.NoFollowLatestThreads)
            wr.Write("&nbsp;" & Localization.GetString("Hours", objConfig.SharedResourceFile) & "&nbsp;")

            'View unread threads link
            If CurrentForumUser.UserID > 0 Then
                wr.Write("|&nbsp;")
                url = Utilities.Links.ContainerViewUnreadThreadsLink(TabID)
                RenderLinkButton(wr, url, Localization.GetString("ViewUnreadThreads", objConfig.SharedResourceFile), "Forum_LastPostText", "", False, objConfig.NoFollowLatestThreads)
                wr.Write("&nbsp;")
            End If
        End Sub

        ''' <summary>
        ''' This allows for spacing between posts
        ''' </summary>
        Private Sub RenderSpacerRow(ByVal wr As HtmlTextWriter)
            RenderRowBegin(wr) '<tr> 
            RenderCellBegin(wr, "", "", "", "", "", "", "")  ' <td>
            RenderImage(wr, objConfig.GetThemeImageURL("height_spacer.gif"), "", "")
            RenderCellEnd(wr) '</td>
            RenderRowEnd(wr) ' </tr>
        End Sub

        Private Sub RenderForumHeader(ByVal wr As HtmlTextWriter, ByVal group As GroupInfo)
            RenderRowBegin(wr) '<tr>
            ' Status icon/Subject/# Currently Viewing  column
            RenderCellBegin(wr, "Forum_Header", "", "52%", "left", "middle", "", "")  '<td>
            RenderDivBegin(wr, "", "Forum_HeaderText") ' <span>
            wr.Write("&nbsp;")
            If AuthorizedGroups.Count > 0 Then
                'wr.Write(ForumControl.LocalizedText("Forums"))
                RenderLinkButton(wr, Utilities.Links.ContainerSingleGroupLink(TabID, group.GroupID), group.Name, "Forum_AltHeaderText")
            Else
                wr.Write(ForumControl.LocalizedText("ForumContainsNothing"))
            End If
            RenderDivEnd(wr) ' </span>
            RenderCellEnd(wr) ' </td>

            ' Threads column 
            RenderCellBegin(wr, "Forum_Header", "", "11%", "center", "middle", "", "")   '<td>
            If AuthorizedGroups.Count > 0 Then
                RenderDivBegin(wr, "", "Forum_HeaderText") ' <span>
                wr.Write(ForumControl.LocalizedText("Threads"))
                RenderDivEnd(wr) ' </span>
            End If
            RenderCellEnd(wr) ' </td>

            ' Posts column 
            RenderCellBegin(wr, "Forum_Header", "", "11%", "center", "middle", "", "")   '<td>
            If AuthorizedGroups.Count > 0 Then
                RenderDivBegin(wr, "", "Forum_HeaderText") ' <span>
                wr.Write(ForumControl.LocalizedText("Posts"))
                RenderDivEnd(wr) ' </span>
            End If
            RenderCellEnd(wr) ' </td>

            ' Last Post column 
            RenderCellBegin(wr, "Forum_Header", "", "26%", "left", "middle", "", "")  '<td>
            If AuthorizedGroups.Count > 0 Then
                RenderDivBegin(wr, "", "Forum_HeaderText") ' <span>
                wr.Write(ForumControl.LocalizedText("LastPost"))
                RenderDivEnd(wr) ' </span>
            End If
            RenderCellEnd(wr) ' </td>

            RenderRowEnd(wr) ' </tr>
        End Sub

        ''' <summary>
        ''' Same as RenderThreads in ForumThread.vb
        ''' This renders the groups then all available forums for the user to view
        ''' If no forums available to user, the group is not rendered at all and moves to the next.
        ''' </summary>
        Private Sub RenderForums(ByVal wr As HtmlTextWriter)
            RenderRowBegin(wr) '<tr>
            RenderCapCell(wr, objConfig.GetThemeImageURL("spacer.gif"), "", "") ' <td><img/></td>
            RenderCellBegin(wr, "", "", "100%", "", "", "", "") '<td>
            RenderTableBegin(wr, "", "", "", "100%", "0", "0", "left", "", "0") ' <table>

            ' Loop through each forum group visible to the user
            If AuthorizedGroups.Count > 0 Then
                Dim objGroup As New GroupInfo
                If ForumControl.objConfig.AggregatedForums Then
                    objGroup = New GroupInfo
                    objGroup.PortalID = PortalID
                    objGroup.ModuleID = ModuleID
                    objGroup.GroupID = -1
                    objGroup.Name = Localization.GetString("AggregatedGroupName", ForumControl.objConfig.SharedResourceFile)
                    AuthorizedGroups.Insert(0, objGroup)
                End If

                ' group is expand or collapse depends on user settings handled a few lines below
                For Each objGroup In AuthorizedGroups
                    ' filter based on group:  - 1 means show all, matching the groupid to the current groupid in the colleciton means show a single one
                    If SelectedGroupID = 0 Or SelectedGroupID = objGroup.GroupID Then
                        '[skeel] Subforums
                        Dim arrForums As List(Of ForumInfo)
                        Dim cntGroup As New GroupController()

                        If SelectedForumID > 0 Then
                            arrForums = cntGroup.AuthorizedSubForums(CurrentForumUser.UserID, objGroup.GroupID, False, SelectedForumID, ModuleID, TabID)
                        Else
                            arrForums = cntGroup.AuthorizedTopLevelForums(CurrentForumUser.UserID, objGroup.GroupID, False, ModuleID, TabID)
                        End If

                        ' display group only if group contains atleast one authorized forum
                        If arrForums.Count > 0 Then
                            ''[skeel] Subforums
                            'If SelectedForumID > 0 Then
                            '    RenderGroupForumInfo(wr, objGroup)
                            'Else
                            '    RenderGroupInfo(wr, objGroup)
                            'End If
                            RenderForumHeader(wr, objGroup)
                            Dim Count As Integer = 1
                            ' Render a row for each forum in this group exposed to this user
                            For Each objForum As ForumInfo In arrForums
                                RenderForum(wr, objForum, CurrentForumUser, Count)
                                Count += 1
                            Next
                        End If
                    End If
                Next

                'CP-** This end td (ends middle column started in previous function and right cap moved to end of RenderThreadInfo)
                RenderTableEnd(wr) ' </table>
                RenderCellEnd(wr) ' </td>
                RenderCapCell(wr, objConfig.GetThemeImageURL("spacer.gif"), "", "") ' <td><img/></td>
                RenderRowEnd(wr) ' </Tr>
            End If
        End Sub

        Private Sub RenderSubjectWithStatus(ByVal wr As HtmlTextWriter, ByVal objForum As ForumInfo)
            ' table holds post icon/thread subject/number viewing
            RenderTableBegin(wr, "", "", "", "100%", "0", "5", "", "", "0")
            RenderRowBegin(wr)

            'status icon column
            RenderCellBegin(wr, "", "", "", "left", "top", "", "")

            Dim NewWindow As Boolean = False
            Dim url As String

            If objForum.ForumType = DotNetNuke.Modules.Forum.ForumType.Link Then
                Dim objCnt As New DotNetNuke.Common.Utilities.UrlController
                Dim objURLTrack As New DotNetNuke.Common.Utilities.UrlTrackingInfo
                Dim TrackClicks As Boolean = False

                objURLTrack = objCnt.GetUrlTracking(PortalID, objForum.ForumLink, ModuleID)

                If Not objURLTrack Is Nothing Then
                    TrackClicks = objURLTrack.TrackClicks
                    NewWindow = objURLTrack.NewWindow
                End If

                url = DotNetNuke.Common.Globals.LinkClick(objForum.ForumLink, TabID, ModuleID, TrackClicks)
            Else
                If objForum.GroupID = -1 Then
                    ' aggregated
                    url = Utilities.Links.ContainerAggregatedLink(TabID, False)
                Else
                    '[Skeel] Check if this is a parent forum
                    If objForum.ContainsChildForums = True Then
                        'Parent forum, link to group view
                        url = Utilities.Links.ContainerParentForumLink(TabID, objForum.GroupID, objForum.ForumID)
                    Else
                        'Normal Forum, link goes to Thread view
                        url = Utilities.Links.ContainerViewForumLink(TabID, objForum.ForumID, False)
                    End If
                End If
            End If

            ' handle HasNewThreads here (because it's user specific)
            Dim HasNewThreads As Boolean = True

            ' Only worry about user forum reads if the user is logged in (performance reasons)
            ' [skeel] .. and not a link type forum
            If CurrentForumUser.UserID > 0 And objForum.ForumType <> 2 Then
                Dim userForumController As New UserForumsController

                '[skeel] added support for subforums
                If objForum.ContainsChildForums = True Then
                    'Parent Forum
                    Dim LastVisitDate As Date = Now.AddYears(1)
                    Dim cntForum As New ForumController()
                    Dim colChildForums As New List(Of ForumInfo)
                    colChildForums = cntForum.GetChildForums(objForum.ForumID, objForum.GroupID, True)

                    For Each objChildForum As ForumInfo In colChildForums
                        Dim userForum As New UserForumsInfo
                        userForum = userForumController.GetUsersForumReads(CurrentForumUser.UserID, objChildForum.ForumID)
                        If Not userForum Is Nothing Then
                            If LastVisitDate > userForum.LastVisitDate Then
                                LastVisitDate = userForum.LastVisitDate
                            End If
                        End If
                    Next

                    If objForum.MostRecentPost Is Nothing Then
                        HasNewThreads = False
                    Else
                        If Not LastVisitDate < objForum.MostRecentPost.CreatedDate Then
                            HasNewThreads = False
                        End If
                    End If
                Else
                    Dim userForum As New UserForumsInfo
                    If Not (objForum.ForumID = -1) Then
                        userForum = userForumController.GetUsersForumReads(CurrentForumUser.UserID, objForum.ForumID)
                    End If

                    If Not userForum Is Nothing Then
                        If objForum.MostRecentPost Is Nothing Then
                            HasNewThreads = False
                        Else
                            If Not userForum.LastVisitDate < objForum.MostRecentPost.CreatedDate Then
                                HasNewThreads = False
                            End If
                        End If
                    End If
                End If
            End If

            ' display image depends on new post status 
            If Not objForum.PublicView Then
                ' See if the forum is a Link Type forum
                If objForum.ForumType = 2 Then
                    RenderImageButton(wr, url, objConfig.GetThemeImageURL("forum_linktype.") & objConfig.ImageExtension, Me.BaseControl.LocalizedText("imgLinkType") & " " & url, "", NewWindow)
                Else
                    ' See if the forum is moderated
                    If objForum.IsModerated Then
                        If HasNewThreads AndAlso objForum.TotalThreads > 0 Then
                            RenderImageButton(wr, url, objConfig.GetThemeImageURL("forum_private_moderated_new.") & objConfig.ImageExtension, Me.BaseControl.LocalizedText("imgNewPrivateModerated"), "", False)
                        Else
                            RenderImageButton(wr, url, objConfig.GetThemeImageURL("forum_private_moderated.") & objConfig.ImageExtension, Me.BaseControl.LocalizedText("imgPrivateModerated"), "", False)
                        End If
                    Else
                        If HasNewThreads AndAlso objForum.TotalThreads > 0 Then
                            RenderImageButton(wr, url, objConfig.GetThemeImageURL("forum_private_new.") & objConfig.ImageExtension, Me.BaseControl.LocalizedText("imgNewPrivate"), "", False)
                        Else
                            RenderImageButton(wr, url, objConfig.GetThemeImageURL("forum_private.") & objConfig.ImageExtension, Me.BaseControl.LocalizedText("imgPrivate"), "", False)
                        End If
                    End If
                End If
            Else
                ' See if the forum is a Link Type forum
                If objForum.ContainsChildForums = True Then
                    '[skeel] parent forum
                    If HasNewThreads AndAlso objForum.TotalThreads > 0 Then
                        RenderImageButton(wr, url, objConfig.GetThemeImageURL("forum_parent_new.") & objConfig.ImageExtension, Me.BaseControl.LocalizedText("imgNewUnmoderated"), "", False)
                    Else
                        RenderImageButton(wr, url, objConfig.GetThemeImageURL("forum_parent.") & objConfig.ImageExtension, Me.BaseControl.LocalizedText("imgUnmoderated"), "", False)
                    End If
                ElseIf objForum.ForumType = 2 Then
                    RenderImageButton(wr, url, objConfig.GetThemeImageURL("forum_linktype.") & objConfig.ImageExtension, Me.BaseControl.LocalizedText("imgLinkType") & " " & url, "", NewWindow)
                Else
                    If objForum.ForumID = -1 Then
                        RenderImageButton(wr, url, objConfig.GetThemeImageURL("forum_aggregate.") & objConfig.ImageExtension, Me.BaseControl.LocalizedText("imgAggregated"), "", False)
                    Else
                        ' Determine if forum is moderated
                        If objForum.IsModerated Then
                            If HasNewThreads AndAlso objForum.TotalThreads > 0 Then
                                RenderImageButton(wr, url, objConfig.GetThemeImageURL("forum_moderated_new.") & objConfig.ImageExtension, Me.BaseControl.LocalizedText("imgNewModerated"), "", False)
                            Else
                                RenderImageButton(wr, url, objConfig.GetThemeImageURL("forum_moderated.") & objConfig.ImageExtension, Me.BaseControl.LocalizedText("imgModerated"), "", False)
                            End If
                        Else
                            If HasNewThreads AndAlso objForum.TotalThreads > 0 Then
                                RenderImageButton(wr, url, objConfig.GetThemeImageURL("forum_unmoderated_new.") & objConfig.ImageExtension, Me.BaseControl.LocalizedText("imgNewUnmoderated"), "", False)
                            Else
                                RenderImageButton(wr, url, objConfig.GetThemeImageURL("forum_unmoderated.") & objConfig.ImageExtension, Me.BaseControl.LocalizedText("imgUnmoderated"), "", False)
                            End If
                        End If
                    End If
                End If
            End If
            ' end status icon column
            RenderCellEnd(wr)

            ' subject/# currently viewing column
            RenderCellBegin(wr, "", "", "100%", "left", "top", "", "")
            If NewWindow Then
                RenderLinkButton(wr, url, objForum.Name, "Forum_NormalBold", "", True, False)
            Else
                RenderLinkButton(wr, url, objForum.Name, "Forum_NormalBold")
            End If
            ' Display forum description
            If Len(objForum.Description) > 0 Then
                wr.Write("<br />")
                wr.AddAttribute(HtmlTextWriterAttribute.Class, "Forum_GroupDetails")
                wr.RenderBeginTag(HtmlTextWriterTag.Span) '<Span>
                wr.Write(objForum.Description)
                wr.RenderEndTag() ' </Span>
            End If

            '[skeel] here we place subforums, if any
            If objForum.ContainsChildForums Then
                wr.Write("<br />")
                wr.AddAttribute(HtmlTextWriterAttribute.Class, "Forum_SubForumContainer")
                wr.RenderBeginTag(HtmlTextWriterTag.Div) '<div>
                RenderSubForums(wr, objForum.ForumID, objForum.GroupID, "Forum_SubForumLink")
                wr.RenderEndTag() '</div>
            End If

            'End column which holds subject/# viewing table
            RenderCellEnd(wr)
            ' end table that holds post icon/thread subject/number viewing
            RenderRowEnd(wr)
            RenderTableEnd(wr)
        End Sub

        ''' <summary>
        ''' Renders forum group view seen on initial forum display.  This shows all expanded
        ''' forum information. (ie. Forum Name, Threads, Posts, Last Post and status icons)
        ''' </summary>
        Public Sub RenderForum(ByVal wr As HtmlTextWriter, ByVal objForum As ForumInfo, ByVal CurrentForumUser As ForumUserInfo, Optional ByVal Count As Integer = 0)
            Try
                Dim fPostsPerPage As Integer = CurrentForumUser.PostsPerPage
                Dim fPage As Page = Me.ForumControl.DNNPage
                Dim fTabID As Integer = Me.ForumControl.TabID
                Dim fModuleID As Integer = Me.ForumControl.ModuleID

                If objForum.ForumID = -1 Then
                    'Aggregated Forum (so this is never cached, since it is per user). 
                    objForum.ModuleID = ModuleID
                    objForum.GroupID = -1
                    objForum.ForumID = -1
                    objForum.ForumType = 0
                    objForum.IsActive = objConfig.AggregatedForums
                    objForum.TotalThreads = 0
                    objForum.TotalPosts = 0
                    objForum.ParentID = 0
                    objForum.SubForums = 0
                    objForum.NotifyByDefault = False
                    objForum.Name = Localization.GetString("AggregatedForumName", objConfig.SharedResourceFile)
                    objForum.Description = Localization.GetString("AggregatedForumDescription", objConfig.SharedResourceFile)

                    Dim SearchCollection As New List(Of ThreadInfo)
                    Dim cntSearch As New SearchController
                    SearchCollection = cntSearch.SearchGetResults("", 0, 1, CurrentForumUser.UserID, ModuleID, DateAdd(DateInterval.Year, -1, DateTime.Today), DateAdd(DateInterval.Day, 1, DateTime.Today), -1)

                    For Each objSearch As ThreadInfo In SearchCollection
                        If objSearch IsNot Nothing Then
                            objForum.MostRecentPostID = objSearch.LastApprovedPostID
                            objForum.ForumID = objSearch.ForumID
                        End If
                    Next
                End If

                If Not objForum Is Nothing Then
                    wr.RenderBeginTag(HtmlTextWriterTag.Tr) ' <tr>   

                    Dim even As Boolean = ForumIsEven(Count)

                    Dim tdCssClass As String = "Forum_Row_Alt"
                    If even Then
                        tdCssClass = "Forum_Row"
                    End If
                    'see threads to determine how to build table here
                    RenderCellBegin(wr, tdCssClass, "", "52%", "left", "top", "", "")
                    ' table holds post icon/thread subject/number viewing
                    RenderSubjectWithStatus(wr, objForum)
                    ' end column that holds table for post icon/thread subject
                    RenderCellEnd(wr)

                    ' Threads column
                    tdCssClass = "Forum_RowHighlight1_Alt"
                    If even Then
                        tdCssClass = "Forum_RowHighlight1"
                    End If
                    RenderCellBegin(wr, tdCssClass, "", "11%", "center", "", "", "")

                    wr.AddAttribute(HtmlTextWriterAttribute.Class, "Forum_Threads")
                    wr.RenderBeginTag(HtmlTextWriterTag.Span) ' <span>

                    If objForum.ForumType = 2 Or objForum.ForumID = -1 Then
                        wr.Write("-")
                    Else
                        wr.Write(objForum.TotalThreads.ToString)
                    End If

                    wr.RenderEndTag() ' </span>
                    RenderCellEnd(wr)

                    'Posts column
                    tdCssClass = "Forum_RowHighlight2_Alt"
                    If even Then
                        tdCssClass = "Forum_RowHighlight2"
                    End If
                    RenderCellBegin(wr, tdCssClass, "", "11%", "center", "", "", "")

                    wr.AddAttribute(HtmlTextWriterAttribute.Class, "Forum_Posts")
                    wr.RenderBeginTag(HtmlTextWriterTag.Span) ' <span>
                    If objForum.ForumType = 2 Or objForum.ForumID = -1 Then
                        wr.Write("-")
                    Else
                        wr.Write(objForum.TotalPosts.ToString)
                    End If
                    wr.RenderEndTag() ' </span>
                    RenderCellEnd(wr)

                    ' last Post date info & author
                    tdCssClass = "Forum_RowHighlight3_Alt"
                    If even Then
                        tdCssClass = "Forum_RowHighlight3"
                    End If
                    RenderCellBegin(wr, tdCssClass, "", "26%", "", "", "", "")
                    RenderLastPost(wr, objForum)
                    RenderCellEnd(wr) ' </Td>

                    RenderRowEnd(wr) ' </Tr>
                End If
            Catch ex As Exception
                LogException(ex)
            End Try
        End Sub

        Private Sub RenderLastPost(ByVal wr As HtmlTextWriter, ByVal objForum As ForumInfo)
            If objForum.ForumType = 2 Then
                'Link forum
                wr.AddAttribute(HtmlTextWriterAttribute.Class, "Forum_LastPostText")
                wr.RenderBeginTag(HtmlTextWriterTag.Span) '<span>
                wr.Write("-")
                wr.RenderEndTag() ' </span>
            Else
                If objForum.MostRecentPost IsNot Nothing Then
                    Dim objLastPost As PostInfo = objForum.MostRecentPost

                    Dim publishedOnDate As String = Utilities.DateUtils.RelativeDate(objLastPost.CreatedDate) 'Utilities.ForumUtils.GetCreatedDateInfo(objLastPost.CreatedDate, objConfig, "")
                    ' shows only first 30 letters of the post subject title
                    Dim title As String = HttpUtility.HtmlDecode(objLastPost.Subject) 'HtmlDecode function prevent to cut string inside html code like: &#245; -> &#2 ...45;
                    Dim truncatedTitle As String = HtmlUtils.Shorten(title, 30, "...")
                    'url = Utilities.Links.ContainerViewPostLink(TabID, objForum.ForumID, objForum.MostRecentPostID)
                    Dim Url As String = Utilities.Links.ContainerViewThreadLink(PortalID, TabID, objLastPost.ThreadID, objLastPost.ParentThread.Subject, objLastPost.PostID)
                    RenderDivBegin(wr, "", "")
                    RenderTitleLinkButton(wr, Url, truncatedTitle, "Forum_LastPostText", title)
                    RenderDivEnd(wr)

                    wr.AddAttribute(HtmlTextWriterAttribute.Class, "Forum_LastPostText")
                    wr.RenderBeginTag(HtmlTextWriterTag.Span) ' <span>
                    wr.Write(Me.BaseControl.LocalizedText("by") & " ")
                    wr.RenderEndTag() ' </span>

                    Url = String.Empty
                    If Not objConfig.EnableExternalProfile Then
                        If objLastPost.Author IsNot Nothing Then
                            Url = objLastPost.Author.UserCoreProfileLink
                        End If
                    Else
                        If objLastPost.Author IsNot Nothing Then
                            Url = Utilities.Links.UserExternalProfileLink(objLastPost.Author.UserID, objConfig.ExternalProfileParam, objConfig.ExternalProfilePage, objConfig.ExternalProfileUsername, objLastPost.Author.Username)
                        End If
                    End If
                    Dim siteAlias As String = String.Empty
                    If objLastPost.Author IsNot Nothing Then
                        siteAlias = objLastPost.Author.SiteAlias
                    End If
                    RenderLinkButton(wr, Url, siteAlias, "Forum_LastPostText")

                    RenderDivBegin(wr, "", "Forum_LastPostText")
                    wr.Write(publishedOnDate)
                    RenderDivEnd(wr)
                Else
                    wr.AddAttribute(HtmlTextWriterAttribute.Class, "Forum_LastPostText")
                    wr.RenderBeginTag(HtmlTextWriterTag.Span) '<span>
                    wr.Write(Me.BaseControl.LocalizedText("None"))
                    wr.RenderEndTag() ' </span>
                End If
            End If
        End Sub

        ''' <summary>
        ''' Renders a list of subforum links
        ''' </summary>
        Private Sub RenderSubForums(ByVal wr As HtmlTextWriter, ByVal ParentID As Integer, ByVal GroupId As Integer, ByVal Css As String)
            Dim Url As String
            Dim i As Integer = 1
            Dim SubForum As New ForumInfo
            Dim forumCtl As New ForumController
            Dim arrSubForums As List(Of ForumInfo) = forumCtl.GetChildForums(ParentID, GroupId, True)

            wr.RenderBeginTag(HtmlTextWriterTag.B) '<b>
            wr.Write(Localization.GetString("SubForums", objConfig.SharedResourceFile) & ": ")
            wr.RenderEndTag() '</b>

            For Each SubForum In arrSubForums
                If SubForum.IsActive Then
                    Dim NewWindow As Boolean = False

                    If SubForum.ForumType = ForumType.Link Then
                        Dim objCnt As New DotNetNuke.Common.Utilities.UrlController
                        Dim objURLTrack As New DotNetNuke.Common.Utilities.UrlTrackingInfo
                        Dim TrackClicks As Boolean = False

                        objURLTrack = objCnt.GetUrlTracking(PortalID, SubForum.ForumLink, ModuleID)

                        If Not objURLTrack Is Nothing Then
                            TrackClicks = objURLTrack.TrackClicks
                            NewWindow = objURLTrack.NewWindow
                        End If

                        Url = DotNetNuke.Common.Globals.LinkClick(SubForum.ForumLink, objConfig.CurrentPortalSettings.ActiveTab.TabID, ModuleID, TrackClicks)
                    Else
                        If SubForum.GroupID = -1 Then
                            ' aggregated
                            Url = Utilities.Links.ContainerAggregatedLink(objConfig.CurrentPortalSettings.ActiveTab.TabID, False)
                        Else
                            Url = Utilities.Links.ContainerViewForumLink(objConfig.CurrentPortalSettings.ActiveTab.TabID, SubForum.ForumID, False)
                        End If
                    End If

                    wr.AddAttribute(HtmlTextWriterAttribute.Href, Url.Replace("~/", ""))

                    If Css.Length > 0 Then
                        wr.AddAttribute(HtmlTextWriterAttribute.Class, Css)
                    End If
                    wr.RenderBeginTag(HtmlTextWriterTag.A) '<a>

                    wr.Write(SubForum.Name)
                    wr.RenderEndTag() ' </a>

                    If i < arrSubForums.Count Then
                        wr.Write(", ")
                    End If

                    i = i + 1
                End If
            Next
        End Sub

        ''' <summary>
        ''' Footer area of group section
        ''' </summary>
        Private Sub RenderForumFooter(ByVal wr As HtmlTextWriter)
            RenderRowBegin(wr) '<tr>
            RenderCapCell(wr, objConfig.GetThemeImageURL("headfoot_height.gif"), "", "") ' <td><img/></td>

            RenderCellBegin(wr, "Forum_Footer", "", "", "left", "", "", "") ' <td>
            RenderDivBegin(wr, "spCounting", "Forum_FooterText") ' <span>
            wr.Write("&nbsp;")
            wr.Write(FooterStats())
            RenderDivEnd(wr) ' </span>
            RenderCellEnd(wr) ' </td>

            RenderCapCell(wr, objConfig.GetThemeImageURL("headfoot_height.gif"), "", "") ' <td><img/></td>
            RenderRowEnd(wr) ' </Tr>
        End Sub

        ''' <summary>
        ''' Replaces tokens for stats w/ actual numbers
        ''' </summary>
        Private Function FooterStats() As String
            Dim sb As New StringBuilder
            'no group stats will be show
            If SelectedGroupID <= 0 Then
                sb.Append(ForumControl.LocalizedText("ForumsCountInfoPosts"))
                sb.Replace("[ForumCount]", AuthorizedForumsCount.ToString)

                If objConfig.AggregatedForums Then
                    sb.Replace("[GroupCount]", (AuthorizedGroups.Count - 1).ToString())
                Else
                    sb.Replace("[GroupCount]", AuthorizedGroups.Count.ToString())
                End If
            End If

            Return sb.ToString
        End Function

        ''' <summary>
        ''' Determines if thread is even or odd numbered row
        ''' </summary>
        Private Function ForumIsEven(ByVal Count As Integer) As Boolean
            If Count Mod 2 = 0 Then
                'even
                Return True
            Else
                'odd
                Return False
            End If
        End Function

#End Region

    End Class

End Namespace