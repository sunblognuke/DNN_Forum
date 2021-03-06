'
' DotNetNukeŽ - http://www.dotnetnuke.com
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
	''' Creates an instance of the post info object
	''' </summary>
	''' <remarks>
	''' </remarks>
	Public Class PostInfo

#Region "Private Members"

		Dim _PostID As Integer
		Dim _ParentPostID As Integer
		Dim _UserID As Integer
		Dim _RemoteAddr As String
		Dim _Subject As String
		Dim _Body As String
		Dim _CreatedDate As DateTime
		Dim _ThreadID As Integer
		Dim _UpdatedDate As DateTime
		Dim _UpdatedByUser As Integer
		Dim _IsApproved As Boolean
		Dim _IsLocked As Boolean
		Dim _IsClosed As Boolean
		Dim _PostReported As Integer
		Dim _Addressed As Integer
		Dim _ParseInfo As Integer
		Dim _PostsBefore As Integer
		Dim _PostsAfter As Integer
		Dim _TotalRecords As Integer

#End Region

#Region "Public Properties"

		''' <summary>
		''' The moduleID this post is being accessed from.
		''' </summary>
		''' <value></value>
		''' <returns></returns>
		''' <remarks></remarks>
		Public ReadOnly Property ModuleId() As Integer
			Get
				If ParentThread IsNot Nothing Then
					Return ParentThread.ModuleID
				Else
					Return Nothing
				End If
			End Get
		End Property

		''' <summary>
		''' The forumID this post and thread belong to. 
		''' </summary>
		''' <value></value>
		''' <returns></returns>
		''' <remarks></remarks>
		Public ReadOnly Property ForumID() As Integer
			Get
				If ParentThread IsNot Nothing Then
					Return ParentThread.ForumID
				Else
					Return Nothing
				End If
			End Get
		End Property

		''' <summary>
		''' Details about the thread which contains the post.
		''' </summary>
		''' <value></value>
		''' <returns></returns>
		''' <remarks></remarks>
		Public ReadOnly Property ParentThread() As ThreadInfo
			Get
				Dim cntThread As New ThreadController()
				Return cntThread.GetThread(ThreadID)
			End Get
		End Property

		''' <summary>
		''' The user information about the user who posted the post.
		''' </summary>
		''' <value></value>
		''' <returns></returns>
		''' <remarks></remarks>
		Public ReadOnly Property Author() As ForumUserInfo
			Get
				If ParentThread IsNot Nothing Then
					Dim cntForumUser As New ForumUserController
					Return cntForumUser.GetForumUser(UserID, False, ModuleId, ParentThread.PortalID)
				Else
					Return Nothing
				End If
			End Get
		End Property

		''' <summary>
		''' The user information of the user who last modified the post. 
		''' </summary>
		''' <value></value>
		''' <returns></returns>
		''' <remarks>Returns anonymous user if it wasn't updated. </remarks>
		Public ReadOnly Property LastModifiedAuthor() As ForumUserInfo
			Get
				If ParentThread IsNot Nothing Then
					Dim cntForumUser As New ForumUserController
					Return cntForumUser.GetForumUser(UpdatedByUser, False, ModuleId, ParentThread.PortalID)
				Else
					Return Nothing
				End If
			End Get
		End Property

		''' <summary>
		''' If the thread has new posts since the user's last visit date.
		''' </summary>
		''' <param name="UserID"></param>
		''' <value></value>
		''' <returns></returns>
		''' <remarks></remarks>
		Public ReadOnly Property NewPost(ByVal UserID As Integer) As Boolean
			Get
				Dim userThreadController As New UserThreadsController
				Dim userThread As New UserThreadsInfo
				If UserID > 0 Then
					userThread = userThreadController.GetThreadReadsByUser(UserID, ThreadID)
					If userThread Is Nothing Then
						Return True
					Else
						'consider it a new post if made within the last minute of the most recent visit
						If userThread.LastVisitDate < CreatedDate Then
							Return True
						Else
							Return False
						End If
					End If
				Else
					Return True
				End If
			End Get
		End Property

		''' <summary>
		''' All Attachments related to the specific post 
		''' </summary>
		''' <value></value>
		''' <returns></returns>
		''' <remarks>Returns a list of AttachmentInfo, use Attachments.Count to see if anything is there</remarks>
		Public ReadOnly Property AttachmentCollection(ByVal Enabled As Boolean) As List(Of AttachmentInfo)
			Get
				If Enabled Then
					Dim cntAttachment As New AttachmentController
					Return cntAttachment.GetAllByPostID(PostID)
				Else
					Return Nothing
				End If
			End Get
		End Property

		''' <summary>
		''' The PostID of the post.
		''' </summary>
		''' <value></value>
		''' <returns></returns>
		''' <remarks></remarks>
		Public Property PostID() As Integer
			Get
				Return _PostID
			End Get
			Set(ByVal Value As Integer)
				_PostID = Value
			End Set
		End Property

		''' <summary>
		''' The PostID of the parent post this post was in response to. 
		''' </summary>
		''' <value></value>
		''' <returns></returns>
		''' <remarks></remarks>
		Public Property ParentPostID() As Integer
			Get
				Return _ParentPostID
			End Get
			Set(ByVal Value As Integer)
				_ParentPostID = Value
			End Set
		End Property

		''' <summary>
		''' The UserID who posted the post.
		''' </summary>
		''' <value></value>
		''' <returns></returns>
		''' <remarks></remarks>
		Public Property UserID() As Integer
			Get
				Return _UserID
			End Get
			Set(ByVal Value As Integer)
				_UserID = Value
			End Set
		End Property

		''' <summary>
		''' The IP Address of the user who posted the post.
		''' </summary>
		''' <value></value>
		''' <returns></returns>
		''' <remarks></remarks>
		Public Property RemoteAddr() As String
			Get
				Return _RemoteAddr
			End Get
			Set(ByVal Value As String)
				_RemoteAddr = Value
			End Set
		End Property

		''' <summary>
		''' The subject of the post.
		''' </summary>
		''' <value></value>
		''' <returns></returns>
		''' <remarks></remarks>
		Public Property Subject() As String
			Get
				Return _Subject
			End Get
			Set(ByVal Value As String)
				_Subject = Value
			End Set
		End Property

		''' <summary>
		''' The body of the post.
		''' </summary>
		''' <value></value>
		''' <returns></returns>
		''' <remarks></remarks>
		Public Property Body() As String
			Get
				Return _Body
			End Get
			Set(ByVal Value As String)
				_Body = Value
			End Set
		End Property

		''' <summary>
		''' The date the post was created. 
		''' </summary>
		''' <value></value>
		''' <returns></returns>
		''' <remarks></remarks>
		Public Property CreatedDate() As Date
			Get
				Return _CreatedDate
			End Get
			Set(ByVal Value As Date)
				_CreatedDate = Value
			End Set
		End Property

		''' <summary>
		''' The ThreadID the post belongs to. 
		''' </summary>
		''' <value></value>
		''' <returns></returns>
		''' <remarks></remarks>
		Public Property ThreadID() As Integer
			Get
				Return _ThreadID
			End Get
			Set(ByVal Value As Integer)
				_ThreadID = Value
			End Set
		End Property

		''' <summary>
		''' The date the post was last updated.
		''' </summary>
		''' <value></value>
		''' <returns></returns>
		''' <remarks></remarks>
		Public Property UpdatedDate() As Date
			Get
				Return _UpdatedDate
			End Get
			Set(ByVal Value As Date)
				_UpdatedDate = Value
			End Set
		End Property

		''' <summary>
		''' The userID of the user to last update the post.
		''' </summary>
		''' <value></value>
		''' <returns>Returns UserID of the last user to update the post or -1 if the post has never been updated.</returns>
		''' <remarks></remarks>
		Public Property UpdatedByUser() As Integer
			Get
				Return _UpdatedByUser
			End Get
			Set(ByVal Value As Integer)
				_UpdatedByUser = Value
			End Set
		End Property

		''' <summary>
		''' If the post is approved or not.
		''' </summary>
		''' <value></value>
		''' <returns></returns>
		''' <remarks></remarks>
		Public Property IsApproved() As Boolean
			Get
				Return _IsApproved
			End Get
			Set(ByVal Value As Boolean)
				_IsApproved = Value
			End Set
		End Property

		''' <summary>
		''' If the post is locked or not.
		''' </summary>
		''' <value></value>
		''' <returns></returns>
		''' <remarks></remarks>
		Public Property IsLocked() As Boolean
			Get
				Return _IsLocked
			End Get
			Set(ByVal Value As Boolean)
				_IsLocked = Value
			End Set
		End Property

		''' <summary>
		''' If the post is closed or not. 
		''' </summary>
		''' <value></value>
		''' <returns></returns>
		''' <remarks></remarks>
		Public Property IsClosed() As Boolean
			Get
				Return _IsClosed
			End Get
			Set(ByVal Value As Boolean)
				_IsClosed = Value
			End Set
		End Property

		''' <summary>
		''' The number of times the post has been reported. 
		''' </summary>
		''' <value></value>
		''' <returns></returns>
		''' <remarks></remarks>
		Public Property PostReported() As Integer
			Get
				Return _PostReported
			End Get
			Set(ByVal Value As Integer)
				_PostReported = Value
			End Set
		End Property

		''' <summary>
		''' The number of post reports that have been addressed (for this post). 
		''' </summary>
		''' <value></value>
		''' <returns></returns>
		''' <remarks>If this doesn't match PostReported, it shows up in the moderator reported posts area.</remarks>
		Public Property Addressed() As Integer
			Get
				Return _Addressed
			End Get
			Set(ByVal Value As Integer)
				_Addressed = Value
			End Set
		End Property

		''' <summary>
		''' The ParseInfo of the post as a sum of Enum PostParserInfo 
		''' </summary>
		''' <value></value>
		''' <returns></returns>
		''' <remarks></remarks>
		Public Property ParseInfo() As Integer
			Get
				Return _ParseInfo
			End Get
			Set(ByVal Value As Integer)
				_ParseInfo = Value
			End Set
		End Property

		''' <summary>
		''' The number of approved posts before the current one. 
		''' </summary>
		''' <value></value>
		''' <returns></returns>
		''' <remarks>Replaces sort order, in combination with post after.</remarks>
		Public Property PostsBefore() As Integer
			Get
				Return _PostsBefore
			End Get
			Set(ByVal Value As Integer)
				_PostsBefore = Value
			End Set
		End Property

		''' <summary>
		''' The number of approved posts after the current one. 
		''' </summary>
		''' <value></value>
		''' <returns></returns>
		''' <remarks>Replaces sort order, in combination with post before.</remarks>
		Public Property PostsAfter() As Integer
			Get
				Return _PostsAfter
			End Get
			Set(ByVal Value As Integer)
				_PostsAfter = Value
			End Set
		End Property

		''' <summary>
		''' Total number of posts that meet the criteria (of whatever retrieved them). 
		''' </summary>
		''' <value></value>
		''' <returns></returns>
		''' <remarks>Used for paging.</remarks>
		Public Property TotalRecords() As Integer
			Get
				Return _TotalRecords
			End Get
			Set(ByVal Value As Integer)
				_TotalRecords = Value
			End Set
		End Property

#End Region

	End Class

End Namespace