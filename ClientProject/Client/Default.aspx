<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" Async="true" CodeBehind="Default.aspx.cs" Inherits="Client._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
  <script type="text/javascript">

    $(function () {
      $('#oFile').on('change', function () {
        var filePath = $(this).val();
        console.log(filePath);
      });
    });

  </script>

  <input id="oFile" type="file" runat="server" name="oFile">
  <br />
  <label for="oUsername">Username</label><br />
  <input type="text" id="oUsername"  runat="server" name="oUsername" value="user"><br />
  <label for="oPassword">Password</label><br />
  <input type="password" id="oPassword"  runat="server" name="oPassword" value="password"><br />
  <br />
  <asp:Button ID="SubmitBtn" runat="server" Text="Send data to DB" OnClick="SubmitButton_Click" />

</asp:Content>
