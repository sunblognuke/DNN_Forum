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
    ''' This renders the threads view (second view in hierarchy of forum)
    ''' </summary>
    ''' <remarks>All UI is done in code, no corresponding ascx
    ''' </remarks>
    Public Class Threads
        Inherits ForumObject

#Region "Private Declarations"

        Private _Filter As String = String.Empty
        Private _TotalRecords As Integer
        Private _CurrentPage As Integer = 0
        Private _ThreadCollection As New List(Of ThreadInfo)
        Private _ThreadRatings As New Hashtable

#Region "Controls"

        Private ddlDateFilter As DotNetNuke.Web.UI.WebControls.DnnComboBox
        Private cmdRead As LinkButton
        Private chkEmail As CheckBox
        Private txtForumSearch As TextBox
        Private cmdForumSearch As ImageButton
        Private trcRating As Telerik.Web.UI.RadRating
        Private cmdForumSubscribers As LinkButton

#End Region

#End Region

#Region "Private Properties"

        ''' <summary>
        ''' This is used to determine the permissions for the current user/forum combination. 
        ''' </summary>
        Private ReadOnly Property objSecurity() As ModuleSecurity
            Get
                Return New ModuleSecurity(ModuleID, TabID, ForumID, CurrentForumUser.UserID)
            End Get
        End Property

        ''' <summary>
        '''  The forum we are viewing the threads for.
        ''' </summary>
        Private ReadOnly Property CurrentForum() As ForumInfo
            Get
                Dim cntForum As New ForumController
                Return cntForum.GetForumItemCache(ForumID)
            End Get
        End Property

        ''' <summary>
        ''' Used to retrieve only posts with no replies if true.
        ''' </summary>
        Private ReadOnly Property NoReply() As Boolean
            Get
                If HttpContext.Current.Request.QueryString("noreply") IsNot Nothing Then
                    Return True
                Else
                    Return False
                End If
            End Get
        End Property

        ''' <summary>
        ''' Used to retrieve only posts with resolved status.
        ''' </summary>
        Private ReadOnly Property NoResolved() As Boolean
            Get
                If HttpContext.Current.Request.QueryString("noresolved") IsNot Nothing Then
                    Return True
                Else
                    Return False
                End If
            End Get
        End Property

        ''' <summary>
        ''' If a thread has new posts or not using the UserReads methods. 
        ''' </summary>
        Private ReadOnly Property HasNewPosts(ByVal UserID As Integer, ByVal objThread As ThreadInfo) As Boolean
            Get
                Dim userthreadController As New UserThreadsController
                Dim userthread As New UserThreadsInfo

                If UserID > 0 Then
                    If objThread Is Nothing Then
                        Return True
                    Else
                        userthread = userthreadController.GetThreadReadsByUser(UserID, objThread.ThreadID)
                        If userthread Is Nothing Then
                            Return True
                        Else
                            If userthread.LastVisitDate < objThread.LastApprovedPost.CreatedDate Then
                                Return True
                            Else
                                Return False
                            End If
                        End If
                    End If
                Else
                    Return True
                End If
            End Get
        End Property

        ''' <summary>
        ''' The item used to filter the returned threads. 
        ''' </summary>
        ''' <remarks>This can be things such as date</remarks>
        Private Property Filter() As String
            Get
                Return _Filter
            End Get
            Set(ByVal Value As String)
                _Filter = Value
            End Set
        End Property

        ''' <summary>
        ''' The total number of threads available for viewing.
        ''' </summary>
        Private Property TotalRecords() As Integer
            Get
                Return _TotalRecords
            End Get
            Set(ByVal Value As Integer)
                _TotalRecords = Value
            End Set
        End Property

        ''' <summary>
        ''' The current page the user is viewing.  
        ''' </summary>
        Private Property CurrentPage() As Integer
            Get
                Return _CurrentPage
            End Get
            Set(ByVal Value As Integer)
                _CurrentPage = Value
            End Set
        End Property

        ''' <summary>
        ''' The collection of threads returned. 
        ''' </summary>
        Private Property ThreadCollection() As List(Of ThreadInfo)
            Get
                Return _ThreadCollection
            End Get
            Set(ByVal Value As List(Of ThreadInfo))
                _ThreadCollection = Value
            End Set
        End Property

        ''' <summary>
        ''' A collection of ratings controls. 
        ''' </summary>
        Private Property ThreadRatings() As Hashtable
            Get
                Return _ThreadRatings
            End Get
            Set(ByVal Value As Hashtable)
                _ThreadRatings = Value
            End Set
        End Property

#End Region

#Region "Event Handlers"

        ''' <summary>
        ''' Filters the threads show by date on postback
        ''' </summary>
        Protected Sub ddlDateFilter_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs)
            Dim dFilter As String = ddlDateFilter.SelectedItem.Value
            Dim url As String
            url = Utilities.Links.ContainerThreadDateFilterLink(TabID, dFilter, ForumID, NoReply)

            If CurrentForumUser.UserID > 0 Then
                'update the user profile
                Dim fUserCnt As New ForumUserController
                fUserCnt.UserUpdateTrackingDuration(CType(dFilter, Integer), CurrentForumUser.UserID, ForumControl.PortalID)
                DotNetNuke.Modules.Forum.Components.Utilities.Caching.UpdateUserCache(CurrentForumUser.UserID, PortalID)
            End If

            MyBase.BasePage.Response.Redirect(url, False)
        End Sub

        ''' <summary>
        ''' Marks all threads in the forum as unread 
        ''' </summary>
        Protected Sub cmdRead_Clicked(ByVal sender As Object, ByVal e As EventArgs)
            If objConfig.EnableUserReadManagement Then
                Dim userThreadController As New UserThreadsController
                userThreadController.MarkAll(CurrentForumUser.UserID, ForumID, True)
            End If
        End Sub

        ''' <summary>
        ''' Toggles user notification for thread on/off on postback
        ''' </summary>
        Protected Sub chkEmail_CheckedChanged(ByVal sender As Object, ByVal e As EventArgs)
            Dim ctlTracking As New TrackingController
            ctlTracking.TrackingForumCreateDelete(ForumID, CurrentForumUser.UserID, CType(sender, CheckBox).Checked, ModuleID)
        End Sub

        ''' <summary>
        ''' This directs the user to the search results of this particular forum. It searches this forum and the subject, body of the post. 
        ''' </summary>
        Protected Sub cmdForumSearch_Click(ByVal sender As Object, ByVal e As ImageClickEventArgs)
            Dim url As String
            url = Utilities.Links.ContainerSingleForumSearchLink(TabID, ForumID, txtForumSearch.Text)
            MyBase.BasePage.Response.Redirect(url, False)
        End Sub

        ''' <summary>
        ''' Visible only to module admin, this allows them to view a list of subscribers to the current thread (via Admin Control Panel).
        ''' </summary>
        Protected Sub cmdForumSubscribers_Click(ByVal sender As Object, ByVal e As EventArgs)
            Dim url As String
            url = Utilities.Links.ForumEmailSubscribers(TabID, ModuleID, ForumID)
            MyBase.BasePage.Response.Redirect(url, False)
        End Sub

#End Region

#Region "Public Methods"

#Region "Constructors"

        ''' <summary>
        ''' Creates a new instance of this class
        ''' </summary>
        Public Sub New(ByVal forum As DNNForum)
            MyBase.New(forum)

            If ForumID = -1 Then
                ' Redirect the user to the aggregated view (Prior to 4.4.4, aggregated used this view so we have to handle legacy links)
                HttpContext.Current.Response.Redirect(Utilities.Links.ContainerAggregatedLink(TabID, False), True)
            Else
                If CurrentForum Is Nothing OrElse Not CurrentForum.IsActive Then
                    ' we should consider setting type of redirect here?
                    MyBase.BasePage.Response.Redirect(Utilities.Links.NoContentLink(TabID, ModuleID), True)
                End If

                ' The forum is private, see if we have proper view perms here
                If Not CurrentForum.PublicView OrElse Not objSecurity.IsAllowedToViewPrivateForum Then
                    ' we should consider setting type of redirect here?
                    MyBase.BasePage.Response.Redirect(Utilities.Links.UnAuthorizedLink(), True)
                End If
            End If

            'We are past knowing the user should be here, let's handle SEO oriented things
            If objConfig.OverrideTitle Then
                Me.BaseControl.BasePage.Title = CurrentForum.Name & " - " & Me.BaseControl.PortalName
            End If

            ' Consider add metakeywords via applied tags, when taxonomy is integrated
            If objConfig.OverrideDescription Then
                MyBase.BasePage.Description = "," + CurrentForum.Name + "," + Me.BaseControl.PortalName
            End If

            BuildPagingParameters()

            BuildFilter()
        End Sub

        Private Sub BuildPagingParameters()
            ' We need to make sure the user's thread pagesize can handle this 
            '(problem is, a link can be posted by one user w/ page size of 5 pointing to page 2, if logged in user has pagesize set to 15, there is no page 2)
            If Not HttpContext.Current.Request.QueryString("currentpage") Is Nothing Then
                Dim urlThreadPage As Integer = Int32.Parse(HttpContext.Current.Request.QueryString("currentpage"))
                Dim TotalThreads As Integer = CurrentForum.TotalThreads
                Dim userThreadsPerPage As Integer

                If CurrentForumUser.UserID > 0 Then
                    userThreadsPerPage = CurrentForumUser.ThreadsPerPage
                Else
                    userThreadsPerPage = objConfig.ThreadsPerPage
                End If

                Dim TotalPages As Integer = CInt(Math.Ceiling(TotalThreads / userThreadsPerPage))
                Dim ThreadPageToShow As Integer

                ' We need to check if it is possible for a pagesize seen in the URL for the user browsing (happens when coming from posted link by other user)
                If TotalPages >= urlThreadPage Then
                    ThreadPageToShow = urlThreadPage
                Else
                    ' We know for this user, total pages > user posts per page. Because of this, we know its not user using page change so show thread as normal
                    ThreadPageToShow = 0
                End If

                CurrentPage = ThreadPageToShow
            End If

            If CurrentPage > 0 Then
                CurrentPage = CurrentPage - 1
            End If
        End Sub

        Private Sub BuildFilter()
            Dim Term As New SearchTerms
            If CurrentForumUser.UserID > -1 Then
                Dim dateFilter As Integer = CurrentForumUser.TrackingDuration

                If dateFilter > 999 Then
                    ' we are not going to add a searchTerm, this means get all threads from all days
                ElseIf dateFilter = -1 Then ' Last Activity
                    Dim DateDiff As TimeSpan
                    Dim DateString As String
                    DateDiff = Date.Now.Subtract(CurrentForumUser.LastActivity)
                    DateString = " DATEADD(hh, " & -DateDiff.Hours & ", GETDATE()) "
                    Term.AddSearchTerm("FP.CreatedDate", CompareOperator.GreaterThan, DateString)
                ElseIf (dateFilter = 0) Then ' Today
                    Dim DateDiff As TimeSpan
                    Dim DateString As String
                    DateDiff = Date.Now.Subtract(Date.Today())
                    DateString = " DATEADD(hh, " & -DateDiff.Hours & ", GETDATE()) "
                    Term.AddSearchTerm("FP.CreatedDate", CompareOperator.GreaterThanDate, DateString)
                Else
                    ' get threads by date interval
                    Dim DateString As String
                    DateString = " DATEADD(dd, " & -dateFilter & ", GETDATE()) "
                    Term.AddSearchTerm("FP.CreatedDate", CompareOperator.GreaterThanDate, DateString)
                End If
            Else
                ' Add date filter
                If Not HttpContext.Current.Request.QueryString("datefilter") Is Nothing Then
                    Dim dateFilter As Integer = Int16.Parse(HttpContext.Current.Request.QueryString("datefilter"))

                    If (dateFilter = 0) Then    ' Today
                        Dim DateDiff As TimeSpan
                        Dim DateString As String
                        DateDiff = Date.Now.Subtract(Date.Today())
                        DateString = " DATEADD(hh, " & -DateDiff.Hours & ", GETDATE()) "
                        Term.AddSearchTerm("FP.CreatedDate", CompareOperator.GreaterThanDate, DateString)
                    ElseIf (Not dateFilter > 999) Then
                        ' get threads by date interval
                        Dim DateString As String
                        DateString = " DATEADD(dd, " & -dateFilter & ", GETDATE()) "
                        Term.AddSearchTerm("FP.CreatedDate", CompareOperator.GreaterThanDate, DateString)
                    End If
                End If
            End If

            If NoReply Then
                Term.AddSearchTerm("T.Replies", CompareOperator.EqualString, "0")
            End If
            If NoResolved Then
                Term.AddSearchTerm("T.ThreadStatus", CompareOperator.LessThan, "2") '2 is resolved and 3 is informative
            End If

            Filter = Term.WhereClause
        End Sub
#End Region

        ''' <summary>
        ''' Create an instance of the controls used here
        ''' </summary>
        Public Overrides Sub CreateChildControls()
            Controls.Clear()

            ' display tracking option only if user authenticated
            If CurrentForumUser.UserID > 0 Then
                cmdRead = New LinkButton
                With cmdRead
                    .CssClass = "Forum_Link"
                    .ID = "chkRead"
                    .Text = ForumControl.LocalizedText("MarkThreadAsRead")
                End With

                chkEmail = New CheckBox
                With chkEmail
                    .CssClass = "Forum_NormalTextBox"
                    .ID = "chkEmail"
                    .Text = ForumControl.LocalizedText("MailWhenNewThread")
                    .TextAlign = TextAlign.Left
                    .AutoPostBack = True
                    .Checked = False
                End With
            End If

            ddlDateFilter = New DotNetNuke.Web.UI.WebControls.DnnComboBox
            With ddlDateFilter
                .CssClass = "Forum_NormalTextBox"
                .Skin = "WebBlue"
                .ID = "lstDateFilter"
                .Width = Unit.Parse("160")
                .AutoPostBack = True
                .ClearSelection()
            End With

            txtForumSearch = New TextBox
            With txtForumSearch
                .CssClass = "Forum_NormalTextBox"
                .ID = "txtForumSearch"
            End With

            Me.cmdForumSearch = New ImageButton
            With cmdForumSearch
                .CssClass = "Forum_Profile"
                .ID = "cmdForumSearch"
                .AlternateText = ForumControl.LocalizedText("Search")
                .ToolTip = ForumControl.LocalizedText("Search")
                .ImageUrl = objConfig.GetThemeImageURL("s_lookup.") & objConfig.ImageExtension
            End With

            Me.cmdForumSubscribers = New LinkButton
            With cmdForumSubscribers
                .CssClass = "Forum_Profile"
                .ID = "cmdForumSubscribers"
                .Text = ForumControl.LocalizedText("cmdForumSubscribers")
            End With

            BindControls()
            AddControlHandlers()
            AddControlsToTree()

            For Each objThread As ThreadInfo In ThreadCollection
                Me.trcRating = New Telerik.Web.UI.RadRating
                With trcRating
                    .Enabled = False
                    .Skin = "Office2007"
                    .Width = Unit.Parse("200")
                    .SelectionMode = Telerik.Web.UI.RatingSelectionMode.Continuous
                    .IsDirectionReversed = False
                    .Orientation = Orientation.Horizontal
                    .Precision = Telerik.Web.UI.RatingPrecision.Half
                    .ItemCount = objConfig.RatingScale

                    .ID = "trcRating" + objThread.ThreadID.ToString()
                    .Value = CDec(objThread.Rating)
                    'AddHandler trcRating.Command, AddressOf trcRating_Rate
                End With
                ThreadRatings.Add(objThread.ThreadID, trcRating)
                Controls.Add(trcRating)
            Next
        End Sub

        ''' <summary>
        ''' Builds the control view seen on the page * This is the final step in 
        ''' in this views lifecycle (that is used)
        ''' </summary>
        Public Overrides Sub Render(ByVal wr As HtmlTextWriter)
            'update the UserForum record (leave even if permitting anonymous posting)
            If HttpContext.Current.Request.IsAuthenticated And ForumID <> -1 Then
                HandleReads()
            End If

            RenderTableBegin(wr, 0, 0, "tblThreads")

            RenderNavBar(wr, objConfig, ForumControl)
            RenderSpacerRow(wr, String.Empty, "1")
            RenderBreadCrumbRow(wr)
            RenderSpacerRow(wr, String.Empty, "1")
            RenderNewThreadAction(wr)
            RenderSpacerRow(wr, String.Empty, "1")
            RenderThreads(wr)
            RenderFooterRow(wr)
            'RenderNewThreadAction(wr)
            RenderSpacerRow(wr, String.Empty, "1")
            RenderBreadCrumbRow(wr)
            RenderThreadOptions(wr)

            RenderTableEnd(wr)
        End Sub

#End Region

#Region "Private Methods"

        ''' <summary>
        ''' Determines if the user reads need to be updated, if so it handles that.
        ''' </summary>
        Private Sub HandleReads()
            Dim userForumController As New UserForumsController
            Dim userForum As New UserForumsInfo

            userForum = userForumController.GetUsersForumReads(CurrentForumUser.UserID, ForumID)

            If Not userForum Is Nothing Then
                userForum.LastVisitDate = Now
                userForumController.Update(userForum)
                UserForumsController.ResetUsersForumReads(CurrentForumUser.UserID, ForumID)
            Else
                userForum = New UserForumsInfo
                With userForum
                    .UserID = CurrentForumUser.UserID
                    .ForumID = ForumID
                    .LastVisitDate = Now
                End With
                userForumController.Add(userForum)
                UserForumsController.ResetUsersForumReads(CurrentForumUser.UserID, ForumID)
            End If
            'ForumController.ResetForumInfoCache(ForumID)
        End Sub

        ''' <summary>
        ''' Loads all the handlers for the controls used in this view
        ''' </summary>
        Private Sub AddControlHandlers()
            Try
                If CurrentForumUser.UserID > 0 Then
                    AddHandler cmdRead.Click, AddressOf cmdRead_Clicked
                    AddHandler chkEmail.CheckedChanged, AddressOf chkEmail_CheckedChanged
                End If

                AddHandler ddlDateFilter.SelectedIndexChanged, AddressOf ddlDateFilter_SelectedIndexChanged
                AddHandler cmdForumSearch.Click, AddressOf cmdForumSearch_Click
                AddHandler cmdForumSubscribers.Click, AddressOf cmdForumSubscribers_Click
                ' would add rating control handler here if we permit rating in thread view in future. (may move to per post rating, in which case this would always be no). 
            Catch exc As Exception
                LogException(exc)
            End Try
        End Sub

        ''' <summary>
        ''' Loads the controls early on in the control's lifecycle so they can be used later on
        ''' </summary>
        Private Sub AddControlsToTree()
            Try
                If CurrentForumUser.UserID > 0 Then
                    Controls.Add(cmdRead)
                    Controls.Add(chkEmail)
                    Controls.Add(cmdForumSubscribers)
                End If
                Controls.Add(ddlDateFilter)
                Controls.Add(txtForumSearch)
                Controls.Add(cmdForumSearch)
            Catch exc As Exception
                LogException(exc)
            End Try
        End Sub

        ''' <summary>
        ''' Binds the controls used in this view, happens each postback too
        ''' </summary>
        Private Sub BindControls()
            Try
                ddlDateFilter.Items.Clear()

                ddlDateFilter.Items.Insert(0, New Telerik.Web.UI.RadComboBoxItem(Localization.GetString("Today", objConfig.SharedResourceFile), "0"))
                ddlDateFilter.Items.Insert(1, New Telerik.Web.UI.RadComboBoxItem(Localization.GetString("PastThreeDays", objConfig.SharedResourceFile), "3"))
                ddlDateFilter.Items.Insert(2, New Telerik.Web.UI.RadComboBoxItem(Localization.GetString("PastWeek", objConfig.SharedResourceFile), "7"))
                ddlDateFilter.Items.Insert(3, New Telerik.Web.UI.RadComboBoxItem(Localization.GetString("PastTwoWeek", objConfig.SharedResourceFile), "14"))
                ddlDateFilter.Items.Insert(4, New Telerik.Web.UI.RadComboBoxItem(Localization.GetString("PastMonth", objConfig.SharedResourceFile), "30"))
                ddlDateFilter.Items.Insert(5, New Telerik.Web.UI.RadComboBoxItem(Localization.GetString("PastThreeMonth", objConfig.SharedResourceFile), "92"))
                ddlDateFilter.Items.Insert(6, New Telerik.Web.UI.RadComboBoxItem(Localization.GetString("PastYear", objConfig.SharedResourceFile), "365"))
                ddlDateFilter.Items.Insert(7, New Telerik.Web.UI.RadComboBoxItem(Localization.GetString("AllDays", objConfig.SharedResourceFile), "3650"))
                'ddlDateFilter.Items.Insert(0, New DotNetNuke.Wrapper.UI.WebControls.DnnComboBoxItem(Localization.GetString("Today", objConfig.SharedResourceFile), "0"))
                'ddlDateFilter.Items.Insert(1, New DotNetNuke.Wrapper.UI.WebControls.DnnComboBoxItem(Localization.GetString("PastThreeDays", objConfig.SharedResourceFile), "3"))
                'ddlDateFilter.Items.Insert(2, New DotNetNuke.Wrapper.UI.WebControls.DnnComboBoxItem(Localization.GetString("PastWeek", objConfig.SharedResourceFile), "7"))
                'ddlDateFilter.Items.Insert(3, New DotNetNuke.Wrapper.UI.WebControls.DnnComboBoxItem(Localization.GetString("PastTwoWeek", objConfig.SharedResourceFile), "14"))
                'ddlDateFilter.Items.Insert(4, New DotNetNuke.Wrapper.UI.WebControls.DnnComboBoxItem(Localization.GetString("PastMonth", objConfig.SharedResourceFile), "30"))
                'ddlDateFilter.Items.Insert(5, New DotNetNuke.Wrapper.UI.WebControls.DnnComboBoxItem(Localization.GetString("PastThreeMonth", objConfig.SharedResourceFile), "92"))
                'ddlDateFilter.Items.Insert(6, New DotNetNuke.Wrapper.UI.WebControls.DnnComboBoxItem(Localization.GetString("PastYear", objConfig.SharedResourceFile), "365"))
                'ddlDateFilter.Items.Insert(7, New DotNetNuke.Wrapper.UI.WebControls.DnnComboBoxItem(Localization.GetString("AllDays", objConfig.SharedResourceFile), "3650"))

                If CurrentForumUser.UserID > 0 Then
                    ddlDateFilter.Items.Insert(0, New Telerik.Web.UI.RadComboBoxItem(Localization.GetString("LastVisit", objConfig.SharedResourceFile), "-1"))
                    'ddlDateFilter.Items.Insert(0, New DotNetNuke.Wrapper.UI.WebControls.DnnComboBoxItem(Localization.GetString("LastVisit", objConfig.SharedResourceFile), "-1"))
                End If

                Dim dateFilter As Integer = 1000

                ' create child control date filter dropdownlist
                If Not HttpContext.Current.Request.QueryString("datefilter") Is Nothing Then
                    dateFilter = Int16.Parse(HttpContext.Current.Request.QueryString("datefilter"))
                Else
                    ' Tracking Duration
                    If CurrentForumUser.UserID > 0 Then
                        dateFilter = CurrentForumUser.TrackingDuration
                    End If
                End If

                ddlDateFilter.SelectedValue = dateFilter.ToString()

                If CurrentForumUser.UserID > 0 And objConfig.MailNotification Then
                    ' We check if user is subscribed in this forum
                    Dim blnTrackedForum As Boolean = False

                    For Each objTrackedForum As TrackingInfo In CurrentForumUser.TrackedForums(ModuleID)
                        If objTrackedForum.ForumID = ForumID Then
                            blnTrackedForum = True
                            Exit For
                        End If
                    Next

                    chkEmail.Visible = True
                    chkEmail.Checked = blnTrackedForum
                End If

                ' Now we get threads to display for this user
                Dim ctlThread As New ThreadController
                ThreadCollection = ctlThread.GetForumThreads(ModuleID, ForumID, CurrentForumUser.ThreadsPerPage, CurrentPage, Filter, PortalID)

            Catch exc As Exception
                LogException(exc)
            End Try
        End Sub

        ''' <summary>
        ''' Renders the Rating selector, current rating image, search textbox and button
        ''' </summary>
        Private Sub RenderSearchBarRow(ByVal wr As HtmlTextWriter)
            RenderRowBegin(wr) '<tr>

            ' left cap
            RenderCapCell(wr, objConfig.GetThemeImageURL("spacer.gif"), "", "")

            RenderCellBegin(wr, "", "", "100%", "", "", "", "")
            RenderTableBegin(wr, "", "", "", "100%", "0", "0", "", "", "0")
            RenderRowBegin(wr) '<tr>

            RenderCellBegin(wr, "", "", "100%", "right", "", "", "")
            'Start table to hold module action buttons
            RenderTableBegin(wr, "", "", "", "", "0", "0", "", "", "0")
            RenderRowBegin(wr) '<tr>

            ' changed from forumpost because we have no rating here
            RenderCellBegin(wr, "", "", "", "", "", "", "") ' <td> 
            wr.Write("&nbsp;")
            RenderCellEnd(wr) ' </td>
            ' end changed

            RenderRowEnd(wr) ' </tr>
            RenderTableEnd(wr) ' </table>

            RenderCellEnd(wr) ' </td>
            RenderCellBegin(wr, "", "", "", "right", "middle", "", "") ' <td>

            RenderTableBegin(wr, 0, 0, "InnerTable") '<table>
            RenderRowBegin(wr) ' <tr>
            RenderCellBegin(wr, "", "", "", "", "middle", "", "") ' <td>
            txtForumSearch.RenderControl(wr)
            RenderCellEnd(wr) ' </td>

            RenderCellBegin(wr, "", "", "", "", "middle", "", "") ' <td>
            cmdForumSearch.RenderControl(wr)
            RenderCellEnd(wr) ' </td>
            RenderRowEnd(wr) ' </tr>
            RenderTableEnd(wr) ' </table>

            RenderCellEnd(wr) ' </td>
            RenderRowEnd(wr) ' </tr>
            RenderTableEnd(wr) ' </table>
            RenderCellEnd(wr) ' </td>

            ' right cap
            RenderCapCell(wr, objConfig.GetThemeImageURL("spacer.gif"), "", "")
            wr.RenderEndTag() ' </Tr>
        End Sub

        Private Sub RenderNewThreadAction(ByVal wr As HtmlTextWriter)
            Dim url As String

            RenderRowBegin(wr) '<tr>
            RenderCellBegin(wr, "", "", "", "", "left", "", "") ' <td> 

            RenderTableBegin(wr, "", "", "", "100%", "", "0", "", "", "0") '<Table>  
            RenderRowBegin(wr) ' <tr>

            RenderCellBegin(wr, "", "", "50%", "", "", "", "") ' <td>
            txtForumSearch.RenderControl(wr)
            cmdForumSearch.RenderControl(wr)
            RenderCellEnd(wr) ' </td>

            RenderCellBegin(wr, "Forum_NavBarButton", "", "50%", "right", "", "", "") ' <td> 
            'Remove LoggedOnUserID limitation if wishing to implement Anonymous Posting
            If CurrentForumUser.UserID > 0 And (Not ForumID = -1) Then
                If Not CurrentForum.PublicPosting Then
                    If objSecurity.IsAllowedToStartRestrictedThread Then
                        url = Utilities.Links.NewThreadLink(TabID, ForumID, ModuleID)
                        If CurrentForumUser.IsBanned Then
                            RenderLinkButton(wr, url, ForumControl.LocalizedText("NewThread"), "", False)
                        Else
                            RenderLinkButton(wr, url, ForumControl.LocalizedText("NewThread"), "")
                        End If
                    Else
                        wr.Write("&nbsp;")
                    End If
                Else
                    url = Utilities.Links.NewThreadLink(TabID, ForumID, ModuleID)
                    If CurrentForumUser.IsBanned Then
                        RenderLinkButton(wr, url, ForumControl.LocalizedText("NewThread"), "", False)
                    Else
                        RenderLinkButton(wr, url, ForumControl.LocalizedText("NewThread"), "")
                    End If
                End If
            End If
            RenderCellEnd(wr) ' </td>
            RenderRowEnd(wr) ' </tr>
            RenderTableEnd(wr) ' </table>
            RenderCellEnd(wr) ' </td>
            RenderRowEnd(wr) ' </tr>
        End Sub

        ''' <summary>
        '''  top header w/ subject after bredcrumb/solpart row, before post/avatar row
        ''' </summary>
        Private Sub RenderThreads(ByVal wr As HtmlTextWriter)
            RenderRowBegin(wr) ' <tr>

            ' left cap 
            RenderCapCell(wr, objConfig.GetThemeImageURL("spacer.gif"), "", "") '<td><img/></td>

            'CP-** This td must not be closed at end of this function, move that and right cap to end of next function this is the spanning master column
            'middle column, contains everything seen in thread view (using table)  - This needs to span into next function
            RenderCellBegin(wr, "", "", "100%", "center", "", "", "") ' <td>

            'Create a table here that will hold everything from the top header type colum (says Thread, Replies, Views, Last Post) through each thread but stop at the footer w/ this table
            RenderTableBegin(wr, "", "", "", "100%", "0", "0", "", "", "") ' <table>
            RenderRowBegin(wr) ' <Tr>

            ' Threads column
            RenderCellBegin(wr, "", "", "78%", "", "middle", "", "") '<td>
            'This table is made simply so we can have a height controlling image and apply left cap here
            RenderTableBegin(wr, "", "", "", "100%", "0", "0", "center", "", "0")   '<table>
            RenderRowBegin(wr) ' <Tr>

            RenderCapCell(wr, objConfig.GetThemeImageURL("headfoot_height.gif"), "Forum_HeaderCapLeft", "") ' <td></td>

            RenderCellBegin(wr, "Forum_Header", "", "", "left", "", "", "") ' <td>
            RenderDivBegin(wr, "", "Forum_HeaderText") ' <div>
            wr.Write("&nbsp;" & ForumControl.LocalizedText("Threads"))
            RenderDivEnd(wr) ' </div>
            RenderCellEnd(wr) ' </Td>

            RenderRowEnd(wr) ' </tr>
            RenderTableEnd(wr) ' </table>
            RenderCellEnd(wr) ' </td>

            ' Replies
            RenderCellBegin(wr, "Forum_Header", "", "11%", "center", "", "", "") ' <td>
            RenderDivBegin(wr, "", "Forum_HeaderText") ' <div>
            wr.Write(ForumControl.LocalizedText("Replies"))
            RenderDivEnd(wr) ' </div>
            RenderCellEnd(wr) ' </td>

            ' Views column
            RenderCellBegin(wr, "Forum_Header", "", "11%", "center", "", "", "") '<td>
            RenderDivBegin(wr, "", "Forum_HeaderText") ' <div>
            wr.Write(ForumControl.LocalizedText("Views"))
            RenderDivEnd(wr) ' </div>
            RenderCellEnd(wr) ' </td>

            RenderCapCell(wr, objConfig.GetThemeImageURL("headfoot_height.gif"), "Forum_HeaderCapRight", "") ' <td><img/></td>

            RenderRowEnd(wr) ' </tr>
            RenderTableEnd(wr) ' </table>
            RenderCellEnd(wr) ' </td>

            ' end middle table/colum
            RenderRowEnd(wr) ' </Tr>

            RenderThreadInfo(wr)
        End Sub

        Private Sub RenderThreadColumn(ByVal wr As HtmlTextWriter, ByVal objThread As ThreadInfo)
            ' table holds post icon/thread subject/rating
            RenderTableBegin(wr, "", "", "100%", "100%", "0", "0", "", "", "0") ' <table>
            RenderRowBegin(wr) ' <tr>

            Dim url As String = String.Empty
            ' cell within table for thread status icon
            RenderCellBegin(wr, "", "100%", "", "", "top", "", "")  ' <td>
            Url = Utilities.Links.ContainerViewThreadLink(TabID, ForumID, objThread.ThreadID)

            'link so icon is clickable (also used below for subject)
            'wr.AddAttribute(HtmlTextWriterAttribute.Href, Url)
            'wr.RenderBeginTag(HtmlTextWriterTag.A) ' <a>
            ' see if post is pinned, priority over other icons
            If objThread.IsPinned Then
                ' First see if the thread is popular
                If (objThread.IsPopular) Then
                    ' thread IS popular and pinned
                    ' see if thread is locked
                    If (objThread.IsClosed) Then
                        ' thread IS popular, pinned, locked
                        ' See if this is an unread post
                        If (HasNewPosts(CurrentForumUser.UserID, objThread)) Then
                            ' IS read
                            RenderImage(wr, objConfig.GetThemeImageURL("s_postlockedpinnedunreadplu.") & objConfig.ImageExtension, ForumControl.LocalizedText("imgHotNewLockedPinnedThread"), "")
                        Else
                            ' not read
                            RenderImage(wr, objConfig.GetThemeImageURL("s_postlockedpinnedreadplus.") & objConfig.ImageExtension, ForumControl.LocalizedText("imgHotLockedPinnedThread"), "")
                        End If
                    Else
                        ' thread IS popular, Pinned but NOT locked
                        ' See if this is an unread post
                        If (HasNewPosts(CurrentForumUser.UserID, objThread)) Then
                            ' IS read
                            RenderImage(wr, objConfig.GetThemeImageURL("s_postpinnedunreadplus.") & objConfig.ImageExtension, ForumControl.LocalizedText("imgNewHotPinnedThread"), "")
                        Else
                            ' not read
                            RenderImage(wr, objConfig.GetThemeImageURL("s_postpinnedreadplus.") & objConfig.ImageExtension, ForumControl.LocalizedText("imgHotPinnedThread"), "")
                        End If
                    End If
                Else
                    ' thread NOT popular but IS pinned
                    ' see if thread is locked
                    If (objThread.IsClosed) Then
                        ' thread IS pinned, Locked but NOT popular
                        If (HasNewPosts(CurrentForumUser.UserID, objThread)) Then
                            ' IS read
                            RenderImage(wr, objConfig.GetThemeImageURL("s_postpinnedlockedunread.") & objConfig.ImageExtension, ForumControl.LocalizedText("imgNewPinnedLockedThread"), "")
                        Else
                            ' not read
                            RenderImage(wr, objConfig.GetThemeImageURL("s_postpinnedlockedread.") & objConfig.ImageExtension, ForumControl.LocalizedText("imgPinnedLockedThread"), "")
                        End If
                    Else
                        'thread IS pinned but NOT popular, Locked
                        If (HasNewPosts(CurrentForumUser.UserID, objThread)) Then
                            ' IS read
                            RenderImage(wr, objConfig.GetThemeImageURL("s_postpinnedunread.") & objConfig.ImageExtension, ForumControl.LocalizedText("imgNewPinnedThread"), "")
                        Else
                            ' not read
                            RenderImage(wr, objConfig.GetThemeImageURL("s_postpinnedread.") & objConfig.ImageExtension, ForumControl.LocalizedText("imgPinnedThread"), "")
                        End If
                    End If
                End If
            Else
                ' thread not pinned, determine post icon
                RenderImage(wr, GetMediaURL(objThread), GetMediaText(objThread), "") ' <img/>
            End If

            'wr.RenderEndTag() ' </A>
            RenderCellEnd(wr) ' </td>

            ' cell for thread subject
            RenderCellBegin(wr, "", "", "100%", "left", "", "", "") ' <td>

            Dim SubjectCssClass As String
            If (HasNewPosts(CurrentForumUser.UserID, objThread)) Then
                SubjectCssClass = "Forum_NormalBold"
            Else
                SubjectCssClass = "Forum_Normal"
            End If
            Dim subject As String = objThread.Subject
            ' Format prohibited words
            If ForumControl.objConfig.FilterSubject Then
                subject = Utilities.ForumUtils.FormatProhibitedWord(objThread.Subject, objThread.CreatedDate, PortalID)
            End If
            RenderLinkButton(wr, url, subject, SubjectCssClass)

            RenderDivBegin(wr, "", "Forum_NormalSmall") ' <div>
            wr.Write(String.Format("{0}&nbsp;", ForumControl.LocalizedText("CreatedBy")))
            If Not objConfig.EnableExternalProfile Then
                Url = objThread.StartedByUser.UserCoreProfileLink
            Else
                Url = Utilities.Links.UserExternalProfileLink(objThread.StartedByUserID, objConfig.ExternalProfileParam, objConfig.ExternalProfilePage, objConfig.ExternalProfileUsername, objThread.StartedByUser.Username)
            End If
            RenderLinkButton(wr, Url, objThread.StartedByUser.SiteAlias, "Forum_NormalSmall") ' <a/>

            Dim publishedOnDate As String = Utilities.DateUtils.RelativeDate(objThread.LastApprovedPost.CreatedDate)
            wr.Write(String.Format(". {0} ", ForumControl.LocalizedText("LastestPostBy")))
            If Not objConfig.EnableExternalProfile Then
                url = objThread.LastApprovedUser.UserCoreProfileLink
            Else
                url = Utilities.Links.UserExternalProfileLink(objThread.LastApprovedUser.UserID, objConfig.ExternalProfileParam, objConfig.ExternalProfilePage, objConfig.ExternalProfileUsername, CurrentForumUser.Username)
            End If
            RenderLinkButton(wr, url, objThread.LastApprovedUser.SiteAlias, "Forum_NormalSmall") ' <a/>
            wr.Write(", " + publishedOnDate)

            ' correct logic to handle posts per page per user
            Dim userPostsPerPage As Integer
            ' CapPageCount is number of pages to show as option for user in threads view.
            Dim CapPageCount As Integer = objConfig.PostPagesCount

            If CurrentForumUser.UserID > 0 Then
                userPostsPerPage = CurrentForumUser.PostsPerPage
            Else
                userPostsPerPage = objConfig.PostsPerPage
            End If

            Dim UserPagesCount As Integer = CInt(Math.Ceiling(objThread.TotalPosts / userPostsPerPage))
            Dim ShowFinalPage As Boolean = (UserPagesCount >= CapPageCount)

            ' Only show Pager if there is more than 1 page for the user
            If UserPagesCount > 1 Then
                ' If thread spans several pages, then display text like (Page 1, 2, 3, ..., 5)

                wr.Write(" (" & ForumControl.LocalizedText("Page") & ": ")

                If UserPagesCount >= CapPageCount Then
                    For ThreadPage As Integer = 1 To CapPageCount - 1
                        Url = Utilities.Links.ContainerViewThreadPagedLink(TabID, ForumID, objThread.ThreadID, ThreadPage)
                        wr.AddAttribute(HtmlTextWriterAttribute.Href, Url)
                        wr.AddAttribute(HtmlTextWriterAttribute.Class, "Forum_NormalSmall")
                        wr.AddAttribute(HtmlTextWriterAttribute.Rel, "nofollow")
                        wr.RenderBeginTag(HtmlTextWriterTag.A) ' <a>
                        wr.Write(ThreadPage.ToString())
                        wr.RenderEndTag() ' </a>
                        If (ThreadPage < CapPageCount - 1) Or (ShowFinalPage And ThreadPage = CapPageCount) Then
                            wr.Write(", ")
                        End If
                    Next

                    If ShowFinalPage Then
                        If UserPagesCount > CapPageCount Then
                            wr.Write("..., ")
                        Else
                            wr.Write(", ")
                        End If
                        Url = Utilities.Links.ContainerViewThreadPagedLink(TabID, ForumID, objThread.ThreadID, UserPagesCount)
                        wr.AddAttribute(HtmlTextWriterAttribute.Href, Url)
                        wr.AddAttribute(HtmlTextWriterAttribute.Class, "Forum_NormalSmall")
                        wr.AddAttribute(HtmlTextWriterAttribute.Rel, "nofollow")
                        wr.RenderBeginTag(HtmlTextWriterTag.A) ' <a>
                        wr.Write(UserPagesCount.ToString())
                        wr.RenderEndTag() ' </A>
                    End If
                Else
                    For ThreadPage As Integer = 1 To UserPagesCount
                        Url = Utilities.Links.ContainerViewThreadPagedLink(TabID, ForumID, objThread.ThreadID, ThreadPage)
                        wr.AddAttribute(HtmlTextWriterAttribute.Href, Url)
                        wr.AddAttribute(HtmlTextWriterAttribute.Class, "Forum_NormalSmall")
                        wr.AddAttribute(HtmlTextWriterAttribute.Rel, "nofollow")
                        wr.RenderBeginTag(HtmlTextWriterTag.A) ' <a>
                        wr.Write(ThreadPage.ToString())
                        wr.RenderEndTag() ' </a>
                        If (ThreadPage < UserPagesCount) Or (ShowFinalPage And ThreadPage = UserPagesCount) Then
                            wr.Write(", ")
                        End If
                    Next
                End If

                wr.Write(")")
            End If

            RenderDivEnd(wr) ' </div>
            RenderCellEnd(wr) ' </td>

            If objConfig.EnableRatings And objThread.ContainingForum.EnableForumsRating Then
                RenderCellBegin(wr, "", "", "30%", "right", "", "", "") ' <td>

                If ThreadRatings.ContainsKey(objThread.ThreadID) Then
                    trcRating = CType(ThreadRatings(objThread.ThreadID), Telerik.Web.UI.RadRating)
                    ' CP - we alter statement below if we want to enable 0 rating still showing image.
                    If objThread.Rating > 0 Then
                        trcRating.RenderControl(wr)
                    End If
                End If
                RenderCellEnd(wr) ' </td>
            End If

            'CP - Add for thread status
            RenderCellBegin(wr, "", "", "5%", "right", "", "", "")   ' <td>
            If (objConfig.EnableThreadStatus And objThread.ContainingForum.EnableForumsThreadStatus) Or (objThread.ThreadStatus = ThreadStatus.Poll And objThread.ContainingForum.AllowPolls) Then
                RenderImage(wr, objConfig.GetThemeImageURL(objThread.StatusImage), objThread.StatusText, "") ' <img/>
            End If
            RenderCellEnd(wr) ' </td>

            RenderRowEnd(wr) ' </tr>
            RenderTableEnd(wr) ' </table>
        End Sub

        ''' <summary>
        ''' creates the rows that holds each thread
        ''' </summary>
        Private Sub RenderThreadInfo(ByVal wr As HtmlTextWriter)
            Try
                Dim Count As Integer = 1

                'loop through each post and make a new row within this table
                'Dim url As String
                For Each objThread As ThreadInfo In ThreadCollection
                    If Not objThread Is Nothing Then
                        Dim even As Boolean = IsEven(Count)
                        TotalRecords = objThread.TotalRecords

                        RenderRowBegin(wr) ' <tr>
                        RenderCapCell(wr, objConfig.GetThemeImageURL("spacer.gif"), "", "") '<td><img/></td>
                        RenderCellBegin(wr, "", "", "100%", "center", "", "", "") ' <td>
                        RenderTableBegin(wr, "", "", "", "100%", "0", "0", "", "", "") ' <table>
                        RenderRowBegin(wr) ' <tr>

                        ' cell holds table for post icon/thread subject/rating
                        If even Then
                            RenderCellBegin(wr, "Forum_Row", "100%", "78%", "", "", "", "") ' <td>
                        Else
                            RenderCellBegin(wr, "Forum_Row_Alt", "100%", "78%", "", "", "", "") ' <td>
                        End If
                        RenderThreadColumn(wr, objThread)
                        RenderCellEnd(wr) ' </td>

                        ' Replies
                        If even Then
                            RenderCellBegin(wr, "Forum_RowHighlight1", "", "11%", "center", "", "", "") ' <td>
                        Else
                            RenderCellBegin(wr, "Forum_RowHighlight1_Alt", "", "11%", "center", "", "", "") ' </td>
                        End If

                        RenderDivBegin(wr, "", "Forum_Posts") ' <div>
                        wr.Write(objThread.Replies)
                        RenderDivEnd(wr) ' </div>
                        RenderCellEnd(wr) ' </td>

                        ' Views
                        If even Then
                            RenderCellBegin(wr, "Forum_RowHighlight3", "", "11%", "center", "", "", "") '<td>
                        Else
                            RenderCellBegin(wr, "Forum_RowHighlight3_Alt", "", "11%", "center", "", "", "") ' </td>
                        End If

                        RenderDivBegin(wr, "", "Forum_Threads") ' <div>
                        wr.Write(objThread.Views)
                        RenderDivEnd(wr) ' </div>
                        RenderCellEnd(wr) ' </td>

                        '' Post date info & author
                        'If even Then
                        '    RenderCellBegin(wr, "Forum_RowHighlight3", "", "26%", "", "", "2", "") ' <td>
                        'Else
                        '    RenderCellBegin(wr, "Forum_RowHighlight3_Alt", "", "26%", "", "", "2", "")  ' <td>
                        'End If

                        '' Skeel - This is for showing link to first unread post for logged in users. 
                        'If CurrentForumUser.UserID > 0 Then
                        '    If HasNewPosts(CurrentForumUser.UserID, objThread) Then
                        '        Dim params As String()

                        '        params = New String(2) {"forumid=" & ForumID, "postid=" & objThread.LastApprovedPost.PostID, "scope=posts"}
                        '        url = NavigateURL(TabID, "", params)

                        '        If CurrentForumUser.PostsPerPage < objThread.TotalPosts Then
                        '            'Find the page on which the first unread post is located
                        '            wr.AddAttribute(HtmlTextWriterAttribute.Href, FirstUnreadLink(objThread))
                        '        Else
                        '            'Thread has only one page
                        '            wr.AddAttribute(HtmlTextWriterAttribute.Href, url + "#unread")
                        '        End If
                        '        wr.RenderBeginTag(HtmlTextWriterTag.A) ' <a>
                        '        RenderImage(wr, objConfig.GetThemeImageURL("thread_newest.") & objConfig.ImageExtension, ForumControl.LocalizedText("imgThreadNewest"), "")
                        '        wr.RenderEndTag() ' </a>
                        '    Else
                        '        url = Utilities.Links.ContainerViewPostLink(TabID, ForumID, objThread.LastApprovedPost.PostID)
                        '    End If
                        'Else
                        '    url = Utilities.Links.ContainerViewPostLink(TabID, ForumID, objThread.LastApprovedPost.PostID)
                        'End If
                        '' End Skeel

                        'RenderTitleLinkButton(wr, url, Utilities.ForumUtils.GetCreatedDateInfo(objThread.LastApprovedPost.CreatedDate, objConfig, ""), "Forum_LastPostText", objThread.LastPostShortBody) ' <a/>

                        'RenderDivBegin(wr, "", "Forum_LastPostText")    ' <div>
                        'wr.Write(ForumControl.LocalizedText("by") & " ")

                        'If Not objConfig.EnableExternalProfile Then
                        '    url = objThread.LastApprovedUser.UserCoreProfileLink
                        'Else
                        '    url = Utilities.Links.UserExternalProfileLink(objThread.LastApprovedUser.UserID, objConfig.ExternalProfileParam, objConfig.ExternalProfilePage, objConfig.ExternalProfileUsername, CurrentForumUser.Username)
                        'End If

                        'RenderLinkButton(wr, url, objThread.LastApprovedUser.SiteAlias, "Forum_LastPostText") ' <a/>
                        'RenderDivEnd(wr) ' </div>
                        'RenderCellEnd(wr) ' </td>

                        RenderRowEnd(wr) ' </tr>
                        RenderTableEnd(wr) ' </table>
                        RenderCellEnd(wr) ' </td>
                        RenderCapCell(wr, objConfig.GetThemeImageURL("spacer.gif"), "", "") ' <td><img/></td>
                        RenderRowEnd(wr) ' </tr>

                        Count = Count + 1
                    End If
                Next
            Catch ex As Exception
                LogException(ex)
            End Try
        End Sub

        ''' <summary>
        ''' Footer w/ paging 
        ''' </summary>
        Private Sub RenderFooterRow(ByVal wr As HtmlTextWriter)
            RenderRowBegin(wr) ' <tr>
            RenderCapCell(wr, objConfig.GetThemeImageURL("headfoot_height.gif"), "", "") ' <td><img/></td>

            'Middle Column
            RenderCellBegin(wr, "", "", "100%", "", "", "", "") ' <td>

            RenderTableBegin(wr, "tblFooterContents", "", "", "100%", "0", "0", "", "", "0")      ' <table>
            RenderRowBegin(wr) ' <tr>
            RenderCapCell(wr, objConfig.GetThemeImageURL("headfoot_height.gif"), "Forum_FooterCapLeft", "") ' <td><img/></td>

            RenderCellBegin(wr, "Forum_Footer", "", "100%", "left", "", "", "") ' <td>

            RenderTableBegin(wr, 0, 0, "") ' <table>
            RenderRowBegin(wr) ' <tr>

            ' xml link (for single forum syndication)
            If (ForumControl.objConfig.EnableRSS And CurrentForum.EnableRSS) AndAlso (CurrentForum.PublicView) Then
                RenderCellBegin(wr, "", "", "", "left", "middle", "", "") ' <td>

                wr.AddAttribute(HtmlTextWriterAttribute.Href, objConfig.SourceDirectory & "/Forum_Rss.aspx?forumid=" & Me.ForumID.ToString & "&tabid=" & TabID & "&mid=" & ModuleID)
                wr.AddAttribute(HtmlTextWriterAttribute.Target, "_blank")
                wr.RenderBeginTag(HtmlTextWriterTag.A) ' <a>

                RenderImage(wr, objConfig.GetThemeImageURL("s_rss.") & objConfig.ImageExtension, ForumControl.LocalizedText("imgRSS"), "")
                wr.RenderEndTag() ' </A>
                RenderCellEnd(wr) ' </td>
            End If

            RenderCellBegin(wr, "", "", "", "", "top", "", "") ' <td>
            RenderImage(wr, objConfig.GetThemeImageURL("headfoot_height.gif"), "", "")  ' <img/>
            RenderCellEnd(wr) ' </td>

            RenderCellBegin(wr, "", "", "100%", "", "middle", "", "") ' <td>
            RenderThreadsPaging(wr)
            RenderCellEnd(wr) ' </td>

            RenderRowEnd(wr) ' </tr>
            RenderTableEnd(wr) ' </table>
            RenderCellEnd(wr) ' </td>
            RenderCapCell(wr, objConfig.GetThemeImageURL("headfoot_height.gif"), "Forum_FooterCapRight", "")       ' <td><img/></td>

            'End middle column
            RenderRowEnd(wr) ' </tr>
            RenderTableEnd(wr) ' </table>
            RenderCellEnd(wr) ' </td>

            RenderCapCell(wr, objConfig.GetThemeImageURL("headfoot_height.gif"), "", "") ' <td><img/></td>
            RenderRowEnd(wr) ' </tr>
        End Sub

        ''' <summary>
        ''' bottom Breadcrumb and ddl's, along w/ subscribe chkbx (last row)
        ''' </summary>
        Private Sub RenderBreadCrumbRow(ByVal wr As HtmlTextWriter)
            RenderRowBegin(wr) '<tr>

            RenderCellBegin(wr, "", "", "100%", "left", "", "", "")
            RenderTableBegin(wr, "", "", "", "100%", "0", "0", "", "", "0")
            RenderRowBegin(wr) '<tr>
            RenderCapCell(wr, objConfig.GetThemeImageURL("spacer.gif"), "", "")

            RenderCellBegin(wr, "Forum_BreadCrumb", "", "100%", "left", "", "", "")
            Dim ChildGroupView As Boolean = False
            If CType(ForumControl.TabModuleSettings("groupid"), String) <> String.Empty Then
                ChildGroupView = True
            End If
            wr.Write(Utilities.ForumUtils.BreadCrumbs(TabID, ModuleID, ForumScope.Threads, CurrentForum, objConfig, ChildGroupView))
            If NoReply Then
                wr.Write(Utilities.ForumUtils.GetBreadCrumb(Utilities.Links.ContainerViewForumLink(TabID, ForumID, True), Localization.GetString("Unanswered.Text", objConfig.SharedResourceFile), "/"))
            End If
            If NoResolved Then
                wr.Write(Utilities.ForumUtils.GetBreadCrumb(Utilities.Links.ContainerViewForumLink(TabID, ForumID, "noresolved=1"), Localization.GetString("UnresolvedThreads.Text", objConfig.SharedResourceFile), "/"))
            End If
            RenderCellEnd(wr) ' </td>

            RenderCapCell(wr, objConfig.GetThemeImageURL("spacer.gif"), "", "")
            RenderRowEnd(wr) ' </tr>
            RenderTableEnd(wr) ' </table>
            RenderCellEnd(wr) ' </td>
            RenderRowEnd(wr) ' </tr>
        End Sub

        ''' <summary>
        ''' This allows for spacing between posts
        ''' </summary>
        Private Sub RenderSpacerRow(ByVal wr As HtmlTextWriter, _
                                    Optional ByVal cssClass As String = "Forum_SpacerRow", _
                                    Optional ByVal colspan As String = "2")
            RenderRowBegin(wr) '<tr> 
            RenderCellBegin(wr, cssClass, "", "", "", "", colspan, "")  ' <td>
            RenderImage(wr, objConfig.GetThemeImageURL("height_spacer.gif"), "", "")
            RenderCellEnd(wr) '</td>
            RenderRowEnd(wr) ' </tr>
        End Sub

        ''' <summary>
        ''' Renders the bottom area that includes date drop down, view subscribers link (for admin) and notification checkbox.
        ''' </summary>
        Private Sub RenderThreadOptions(ByVal wr As HtmlTextWriter)
            RenderRowBegin(wr) '<tr>

            RenderCellBegin(wr, "Forum_Subscription", "", "", "right", "", "", "") ' <td>

            RenderDivBegin(wr, "", "Forum_NormalTextBox") ' <div>
            wr.Write(ForumControl.LocalizedText("LatestThreads"))
            ddlDateFilter.RenderControl(wr)
            RenderDivEnd(wr) ' </div>

            ' Display tracking option if user is authenticated 
            If (CurrentForumUser.UserID > 0) And (objConfig.MailNotification) And (ForumID <> -1) Then
                ' Notifications section
                RenderDivBegin(wr, "", "")
                chkEmail.RenderControl(wr)
                RenderDivEnd(wr)

                If objSecurity.IsForumAdmin Then
                    RenderDivBegin(wr, "", "")
                    cmdForumSubscribers.RenderControl(wr)
                    RenderDivEnd(wr)
                End If
            End If

            RenderCellEnd(wr) ' </td> 
            RenderRowEnd(wr) ' </tr>
        End Sub

        ''' <summary>
        ''' Renders the paging shown in the footer (based on threads/page)
        ''' </summary>
        Private Sub RenderThreadsPaging(ByVal wr As HtmlTextWriter)
            ' First, previous, next, last thread hyperlinks
            Dim ctlPagingControl As New DotNetNuke.Modules.Forum.WebControls.PagingControl
            ctlPagingControl.CssClass = "Forum_FooterText"
            ctlPagingControl.TotalRecords = TotalRecords
            ctlPagingControl.PageSize = CurrentForumUser.ThreadsPerPage
            ctlPagingControl.CurrentPage = CurrentPage + 1

            Dim Params As String = "forumid=" & ForumID.ToString & "&scope=threads"
            If NoReply Then
                Params = Params & "&noreply=1"
            End If

            ctlPagingControl.QuerystringParams = Params
            ctlPagingControl.TabID = TabID
            ctlPagingControl.RenderControl(wr)
        End Sub

        ''' <summary>
        ''' Determins if thread is even or odd numbered row
        ''' </summary>
        Private Function IsEven(ByVal Count As Integer) As Boolean
            If Count Mod 2 = 0 Then
                'even
                Return True
            Else
                'odd
                Return False
            End If
        End Function

        ''' <summary>
        ''' Determines thread icon for each thread based on its status 
        ''' (Non Pinned)
        ''' </summary>
        Private Function GetMediaURL(ByVal Thread As ThreadInfo) As String
            If Thread.IsClosed Then
                ' thread IS locked
                If (Thread.IsPopular) Then
                    ' thread IS locked, popular
                    If HasNewPosts(CurrentForumUser.UserID, Thread) Then
                        'new
                        Return objConfig.GetThemeImageURL("s_postlockedunreadplus.") & objConfig.ImageExtension
                    Else
                        'read
                        Return objConfig.GetThemeImageURL("s_postlockedreadplus.") & objConfig.ImageExtension
                    End If
                Else
                    ' thread IS locked NOT popular
                    If HasNewPosts(CurrentForumUser.UserID, Thread) Then
                        'new
                        Return objConfig.GetThemeImageURL("s_postlockedunread.") & objConfig.ImageExtension
                    Else
                        'read
                        Return objConfig.GetThemeImageURL("s_postlockedread.") & objConfig.ImageExtension
                    End If
                End If

            Else
                ' thread NOT locked
                If (Thread.IsPopular) Then
                    If HasNewPosts(CurrentForumUser.UserID, Thread) Then
                        Return objConfig.GetThemeImageURL("s_postunreadplus.") & objConfig.ImageExtension
                    Else
                        Return objConfig.GetThemeImageURL("s_postreadplus.") & objConfig.ImageExtension
                    End If
                Else
                    If HasNewPosts(CurrentForumUser.UserID, Thread) Then
                        Return objConfig.GetThemeImageURL("s_postunread.") & objConfig.ImageExtension
                    Else
                        Return objConfig.GetThemeImageURL("s_postread.") & objConfig.ImageExtension
                    End If
                End If
            End If

        End Function

        ''' <summary>
        ''' Determines the tooltip for each thread icon based on its status for
        ''' the current users (Non Pinned)
        ''' </summary>
        Private Function GetMediaText(ByVal Thread As ThreadInfo) As String
            If Thread.IsClosed Then
                ' thread IS locked
                If (Thread.IsPopular) Then
                    ' thread IS locked, popular
                    If HasNewPosts(CurrentForumUser.UserID, Thread) Then
                        ' new
                        Return ForumControl.LocalizedText("imgNewPopLockedThread")
                    Else
                        ' read
                        Return ForumControl.LocalizedText("imgPopLockedThread")
                    End If
                Else
                    ' thread IS locked NOT popular
                    If HasNewPosts(CurrentForumUser.UserID, Thread) Then
                        ' new
                        Return ForumControl.LocalizedText("imgNewLockedThread")
                    Else
                        ' read
                        Return ForumControl.LocalizedText("imgLockedThread")
                    End If
                End If
            Else
                ' thread NOT locked
                If (Thread.IsPopular) Then
                    ' thread NOT locked IS popular
                    If HasNewPosts(CurrentForumUser.UserID, Thread) Then
                        ' new
                        Return ForumControl.LocalizedText("imgNewPopThread")
                    Else
                        ' read
                        Return ForumControl.LocalizedText("imgPopThread")
                    End If
                Else
                    ' thread NOT locked, popular
                    If HasNewPosts(CurrentForumUser.UserID, Thread) Then
                        ' new
                        Return ForumControl.LocalizedText("imgNewThread")
                    Else
                        ' read
                        Return ForumControl.LocalizedText("imgThread")
                    End If
                End If
            End If

        End Function

        ''' <summary>
        ''' Returns a URL to the first unread post in the specific Thread
        ''' </summary>
        Private Function FirstUnreadLink(ByVal Thread As ThreadInfo) As String
            Dim cltUserThread As New UserThreadsController
            Dim usrThread As New UserThreadsInfo
            Dim ReadLink As String

            ReadLink = Utilities.Links.ContainerViewThreadLink(TabID, ForumID, Thread.ThreadID) & "#unread"
            usrThread = cltUserThread.GetThreadReadsByUser(CurrentForumUser.UserID, Thread.ThreadID)

            If usrThread Is Nothing Then
                'All new
                If CurrentForumUser.ViewDescending = True Then
                    Dim PageCount As Decimal = CDec(Thread.TotalPosts / CurrentForumUser.PostsPerPage)
                    PageCount = Math.Ceiling(PageCount)
                    ReadLink = Utilities.Links.ContainerViewThreadPagedLink(TabID, ForumID, Thread.ThreadID, CInt(PageCount)) & "#unread"
                Else
                    ReadLink = Utilities.Links.ContainerViewThreadLink(TabID, ForumID, Thread.ThreadID) & "#unread"
                End If
            Else
                'Get the Index
                Dim PostIndex As Integer = cltUserThread.GetPostIndexFirstUnread(Thread.ThreadID, usrThread.LastVisitDate, CurrentForumUser.ViewDescending)
                Dim PageCount As Integer = CInt(Math.Ceiling(CDec(Thread.TotalPosts / CurrentForumUser.PostsPerPage)))
                Dim PageNumber As Integer = 1

                Do While PageNumber <= PageCount
                    If (CurrentForumUser.PostsPerPage * PageNumber) >= PostIndex Then
                        If PageNumber = 1 Then
                            ReadLink = Utilities.Links.ContainerViewThreadLink(TabID, ForumID, Thread.ThreadID) & "#unread"
                        Else
                            ReadLink = Utilities.Links.ContainerViewThreadPagedLink(TabID, ForumID, Thread.ThreadID, PageNumber) & "#unread"
                        End If
                        Exit Do
                    End If
                    PageNumber += 1
                Loop
            End If

            Return ReadLink
        End Function

#End Region

    End Class

End Namespace