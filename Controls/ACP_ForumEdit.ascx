<%@ Control language="vb" CodeBehind="ACP_ForumEdit.ascx.vb" AutoEventWireup="false" Explicit="true" Inherits="DotNetNuke.Modules.Forum.ACP.ForumEdit" %>
<%@ Register TagPrefix="dnnforum" Namespace="DotNetNuke.Modules.Forum.Controls" Assembly="DotNetNuke.Modules.Forum" %>
<%@ Register TagPrefix="dnn" TagName="label" Src="~/controls/LabelControl.ascx" %>
<%@ Register TagPrefix="dnn" TagName="URL" Src="~/controls/URLControl.ascx" %>
<%@ Register TagPrefix="dnn" TagName="Audit" Src="~/controls/ModuleAuditControl.ascx" %>
<%@ Register TagPrefix="dnn" TagName="Tracking" Src="~/controls/URLTrackingControl.ascx" %>
<%@ Register TagPrefix="forum" TagName="ACPmenu" src="~/DesktopModules/Forum/Controls/ACP_Menu.ascx" %>
<%@ Register TagPrefix="DNN" Namespace="DotNetNuke.UI.WebControls" Assembly="DotNetNuke.WebControls" %>
<%@ Register Assembly="DotNetNuke.Web" Namespace="DotNetNuke.Web.UI.WebControls" TagPrefix="dnnweb" %>
<%@ Register TagPrefix="wrapper" Namespace="Telerik.Web.UI" Assembly="Telerik.Web.UI" %>
<div class="ACP-ForumEdit dnnForm">
    <table cellpadding="0" cellspacing="0" width="100%" border="0" class="Forum_SearchContainer" >
		<tr valign="top">
			<td class="Forum_UCP_Left"><forum:ACPmenu ID="ACPmenu" runat="server" /></td>
			<td class="Forum_UCP_Right">
                <h2 class="dnnFormSectionHead">
                    <a href="" class="dnnSectionExpanded"><%=LocalizeString("TabGeneral")%></a></h2>
                <fieldset>
                    <div class="dnnFormItem" style="display:none">
                        <asp:textbox id="txtForumID" runat="server" cssclass="Forum_NormalTextBox" width="250px" Enabled="False" />
                    </div> 
                    <div class="dnnFormItem">
                        <dnn:Label id="plEnableForum" runat="server" Suffix=":" controlname="chkActive" />
                        <asp:checkbox id="chkActive" runat="server" CssClass="Forum_NormalTextBox" />
                    </div>
                    <div class="dnnFormItem">
                        <dnn:label id="plGroupName" runat="server" Suffix=":" controlname="ddlGroup" />
                        <dnnweb:DnnComboBox ID="rcbGroup" runat="server" AutoPostBack="true" />
                    </div>
                    <div class="dnnFormItem" id="rowParentForum" runat="server">
                        <dnn:label id="plParentForumName" runat="server" Suffix=":" controlname="ddlParentForum" />
                        <dnnweb:DnnComboBox ID="rcbParentForum" runat="server" AutoPostBack="true" />
                    </div> 
                    <div class="dnnFormItem">
                        <dnn:label id="plForumName" runat="server" Suffix=":" controlname="txtForumName" />
                        <asp:textbox id="txtForumName" runat="server" cssclass="Forum_NormalTextBox" width="250px" MaxLength="255" />
						<asp:RequiredFieldValidator ID="valName" runat="server" ErrorMessage="*" CssClass="NormalRed" Display="Dynamic" ControlToValidate="txtForumName" />
                    </div>
                    <div class="dnnFormItem">
                        <dnn:label id="plForumDescription" runat="server" Suffix=":" controlname="txtForumDescription" />
                         <asp:textbox id="txtForumDescription" runat="server" cssclass="Forum_NormalTextBox" width="250px" Rows="3" TextMode="MultiLine" MaxLength="2048" />
                    </div> 
                </fieldset>
                <h2 class="dnnFormSectionHead">
                    <a href=""><%= LocalizeString("TabOptions")%></a></h2>
                <fieldset>
                    <div class="dnnFormItem">
                        <dnn:label id="plForumType" runat="server" Suffix=":" controlname="ddlForumType" />
                        <dnnweb:DnnComboBox ID="rcbForumType" runat="server" AutoPostBack="true" />
                    </div> 
                    <div class="dnnFormItem" id="rowForumBehavior" runat="server"> 
                        <dnn:label id="plForumBehavior" runat="server" Suffix=":" controlname="ddlForumBehavior" />
                        <dnnweb:DnnComboBox ID="rcbForumBehavior" runat="server" AutoPostBack="true" Width="270" />
                    </div> 
                    <div class="dnnFormItem" id="rowPolls" runat="server">
                        <dnn:label id="plAllowPolls" runat="server" Suffix=":" controlname="chkAllowPolls" />
                        <asp:checkbox id="chkAllowPolls" runat="server" CssClass="Forum_NormalTextBox" />
                    </div> 
                    <div class="dnnFormItem" id="rowThreadStatus" runat="server">
                        <dnn:label id="plEnableForumsThreadStatus" runat="server" Suffix=":" controlname="chkEnableForumsThreadStatus" />
                        <asp:checkbox id="chkEnableForumsThreadStatus" runat="server" CssClass="Forum_NormalTextBox" />
                    </div> 
                    <div class="dnnFormItem" id="rowRating" runat="server">
                        <dnn:label id="plEnableForumsRating" runat="server" Suffix=":" controlname="chkEnableForumsRating" />
                        <asp:checkbox id="chkEnableForumsRating" runat="server" CssClass="Forum_NormalTextBox" />
                    </div> 
                    <div class="dnnFormItem" id="rowEnableRSS" runat="server">
                        <dnn:label id="plEnableRSS" runat="server" Suffix=":" controlname="chkEnableRSS" />
                        <asp:checkbox id="chkEnableRSS" runat="server" CssClass="Forum_NormalTextBox" />
                    </div> 
                    <div class="dnnFormItem" id="rowEnableSitemap" runat="server">
                        <dnn:label id="plEnableSitemap" runat="server" Suffix=":" controlname="chkEnableSitemap" />
                        <asp:checkbox id="chkEnableSitemap" runat="server" CssClass="Forum_NormalTextBox" AutoPostBack="true" />
                    </div> 
                    <div class="dnnFormItem" id="rowSitemapPriority" runat="server">
                        <dnn:label id="plSitemapPriority" runat="server" Suffix=":" controlname="txtSitemapPriority" />
                        <wrapper:RadNumericTextBox ID="txtSitemapPriority" runat="server" MinValue="0" MaxValue="1" NumberFormat-DecimalDigits="2" ShowSpinButtons="true" IncrementSettings-Step=".1" />
                    </div> 
                    <div id="rowPermissions" runat="server">
                        <dnnforum:forumpermissionsgrid id="dgPermissions" runat="server"></dnnforum:forumpermissionsgrid>
						<br /><asp:Label id="lblPrivateNote" runat="server" CssClass="Normal" resourcekey="lblPrivateNote" EnableViewState="false" />
                    </div> 
                    <div class="dnnFormItem">
                        <dnn:label id="plForumPermTemplate" runat="server" controlname="ddlForumPermTemplate" suffix=":" />
                        <dnnweb:DnnComboBox ID="rcbForumPermTemplate" runat="server" AutoPostBack="true" DataTextField="Name" DataValueField="ForumID" />
                    </div> 
                    <div class="dnnFormItem" id="rowForumLink" runat="server">
                    </div>
                    <div class="dnnFormItem" id="rowLinkTracking" runat="server">
                    </div>
                </fieldset>
                <h2 class="dnnFormSectionHead">
                    <a href=""><%= LocalizeString("TabEmail")%></a></h2>
                <fieldset>
                    <div class="dnnFormItem">
                        <dnn:label ID="plEmailAddress" runat="server" controlname="txtEmailAddress" Suffix=":" />
                        <asp:TextBox ID="txtEmailAddress" runat="server" MaxLength="100" cssclass="Forum_NormalTextBox" Width="250px" />
						<asp:RequiredFieldValidator ID="valAddy" runat="server" ErrorMessage="*" CssClass="NormalRed" 
                            Display="Dynamic" ControlToValidate="txtEmailAddress" />
						<asp:RegularExpressionValidator ID="valEmailAddy" runat="server" ControlToValidate="txtEmailAddress" 
                            CssClass="NormalRed" Display="Dynamic" SetFocusOnError="True" 
                            ValidationExpression="\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*" resourcekey="valEmailAddy.ErrorMessage" />
                    </div> 
                    <div class="dnnFormItem">
                        <dnn:label ID="plEmailFriendlyFrom" runat="server" controlname="txtEmailFriendlyFrom" Suffix=":" />
                        <asp:TextBox ID="txtEmailFriendlyFrom" runat="server" MaxLength="50" cssclass="Forum_NormalTextBox" Width="250px" />
						<asp:RequiredFieldValidator ID="valDisplay" runat="server" ErrorMessage="*" CssClass="NormalRed" 
                            Display="Dynamic" ControlToValidate="txtEmailFriendlyFrom" />
                    </div> 
                    <div id="divHidden" runat="server" visible="false">
                    <div class="dnnFormItem">
                        <dnn:label ID="plNotifyByDefault" runat="server" controlname="chkNotifyByDefault" Suffix=":" />
                        <asp:CheckBox ID="chkNotifyByDefault" runat="server" cssclass="Forum_NormalTextBox" />
                    </div> 
                    <div class="dnnFormItem">
                        <dnn:label ID="plEmailStatusChange" runat="server" controlname="chkEmailStatusChange" Suffix=":" />
                        <asp:CheckBox ID="chkEmailStatusChange" runat="server" cssclass="Forum_NormalTextBox" />
                    </div> 
                    <div class="dnnFormItem">
                        <dnn:label ID="plEmailServer" runat="server" controlname="txtEmailServer" Suffix=":" />
                        <asp:TextBox ID="txtEmailServer" runat="server" MaxLength="150" cssclass="Forum_NormalTextBox" Width="250px" />
                    </div> 
                    <div class="dnnFormItem">
                        <dnn:label ID="plEmailUser" runat="server" controlname="txtEmailUser" Suffix=":" />
                        <asp:TextBox ID="txtEmailUser" runat="server" MaxLength="100" cssclass="Forum_NormalTextBox" Width="250px" />
                    </div> 
                    <div class="dnnFormItem">
                        <dnn:label ID="plEmailPass" runat="server" controlname="txtEmailPass" Suffix=":" />
                        <asp:TextBox ID="txtEmailPass" runat="server" MaxLength="50" cssclass="Forum_NormalTextBox" Width="250px" TextMode="Password" />
                    </div> 
                    <div class="dnnFormItem">
                        <dnn:label ID="plEmailPort" runat="server" controlname="txtEmailPort" Suffix=":" />
                        <asp:TextBox ID="txtEmailPort" runat="server" MaxLength="5" cssclass="Forum_NormalTextBox" Width="50px" />
                    </div> 
                   <div class="dnnFormItem">
                        <dnn:label ID="plEmailEnableSSL" runat="server" controlname="chkEmailEnableSSL" Suffix=":" />
                        <asp:Checkbox ID="chkEmailEnableSSL" runat="server" cssclass="Forum_NormalTextBox" />
                    </div>
                    <div class="dnnFormItem">
                        <dnn:label ID="plEmailAuth" runat="server" controlname="txtForumEmailFriendly" Suffix=":" />
                        <asp:DropDownList ID="ddlEmailAuth" runat="server" CssClass="Forum_NormalTextBox" />
                    </div>
                    </div>
                </fieldset>
                <div class="dnnActions dnnClear">
					<asp:linkbutton class="dnnPrimaryAction" id="cmdAdd" runat="server" resourcekey="cmdAdd" />
					<asp:linkbutton class="dnnPrimaryAction" id="cmdUpdate" runat="server" resourcekey="cmdUpdate" />&nbsp;
					<asp:linkbutton class="CommandButton" id="cmdDelete" runat="server" resourcekey="cmdDelete" CausesValidation="false" />
				</div>
				 <div class="dnnssStat dnnClear">
                    <dnn:audit id="ctlAudit" runat="server" />
                </div>
			</td>
		</tr>
		<tr>
			<td align="center" colspan="2">
				<asp:LinkButton ID="cmdHome" runat="server" CssClass="CommandButton" resourcekey="cmdHome" />
			</td>
		</tr>
	</table>
</div>
<script language="javascript" type="text/javascript">
    /*globals jQuery, window, Sys */
    (function ($, Sys) {
        function setupSettings() {
            $('.ACP-ForumEdit').dnnPanels();
        }

        $(document).ready(function () {
            setupSettings();
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
                setupSettings();
            });
        });
    } (jQuery, window.Sys));
</script>