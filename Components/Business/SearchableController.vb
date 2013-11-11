Imports DotNetNuke.Entities.Modules
Imports DotNetNuke.Services.Search

Namespace DotNetNuke.Modules.Forum
    Partial Public Class InterfaceController
        Implements ISearchable
        ''' <summary>
        ''' This is called by the framework's indexing.  This is the only time it 
        ''' is called.  It gathers all posts added which are not private based on
        ''' the date of the post and the last date set of indexing.  This is what
        ''' exposes data to site search.
        ''' </summary>
        Public Function GetSearchItems(ByVal ModInfo As ModuleInfo) As SearchItemInfoCollection Implements ISearchable.GetSearchItems
            Dim objModules As New ModuleController

            ' get the date of the last index operation from module settings
            Dim StartDate As DateTime = Null.NullDate
            Dim settings As Hashtable = objModules.GetModuleSettings(ModInfo.ModuleID)
            If Not settings("LastIndexDate") Is Nothing Then
                Try
                    Dim tempDate As Double
                    tempDate = (CType(settings("LastIndexDate"), Double))
                    StartDate = Utilities.ForumUtils.NumToDate(tempDate)
                Catch exc As Exception
                    LogException(exc)
                End Try
            End If

            ' save the current date
            Dim LastIndexDate As DateTime = Now()

            ' get all posts since the last index date
            Dim SearchItemCollection As New SearchItemInfoCollection
            Dim ThreadSearchCollection As ArrayList = CBO.FillCollection(DotNetNuke.Modules.Forum.DataProvider.Instance().ISearchable(ModInfo.ModuleID, StartDate), GetType(ThreadSearchInfo))
            Dim thread As ThreadSearchInfo

            ' iterate through the posts and create a search item collection
            For Each thread In ThreadSearchCollection
                Dim threadBody As String = HttpUtility.HtmlDecode(thread.Body)
                Dim threadDescription As String = HtmlUtils.Shorten(threadBody, 100, "...")
                Dim SearchItem As SearchItemInfo = New SearchItemInfo(thread.Subject, threadDescription, thread.CreatedByUser, thread.CreatedDate, ModInfo.ModuleID, thread.PostID.ToString, threadBody, "forumid=" & thread.ForumID & "&postid=" & thread.PostID & "&scope=posts", 0)
                If Not SearchItem Is Nothing Then
                    SearchItemCollection.Add(SearchItem)
                End If
            Next

            ' update the last index date
            objModules.UpdateModuleSetting(ModInfo.ModuleID, "LastIndexDate", Utilities.ForumUtils.DateToNum(LastIndexDate).ToString)

            ' return the search item collection
            Return SearchItemCollection
        End Function
    End Class
End Namespace
