<%@ Control Language="vb" AutoEventWireup="false" Codebehind="Forum_ContentRemoved.ascx.vb" Inherits="DotNetNuke.Modules.Forum.ContentRemoved" %>
<table class="Forum_Container" id="tblMain" cellspacing="0" cellpadding="0" width="100%"
	align="center">
	<tr>
		<td id="celHeader" width="100%" class="Forum_Header" valign="middle">
			&nbsp;<asp:Label ID="lblTitleContentRemoved" Runat="server" resourcekey="lblTitleContentRemoved" CssClass="Forum_HeaderText" />
		</td>
	</tr>
	<tr>
		<td valign="top" align="center" width="100%" class="Forum_Row_Admin">
            <h3><%=lblContentRemoved%></h3>
		</td>
	</tr>
	<tr>
		<td class="Forum_Footer" valign="middle" align="center">
			<asp:linkbutton class="CommandButton" id="cmdCancel" runat="server" style="color: #fff;font-weight: bold;" resourcekey="cmdCancel" />
		</td>
	</tr>
</table>