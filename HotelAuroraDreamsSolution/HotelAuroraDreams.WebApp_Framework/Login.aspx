<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="HotelAuroraDreams.WebApp_Framework.Login" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Iniciar Sesión - Hotel Aurora Dreams</title>
    <style>
        body { font-family: Arial, sans-serif; margin: 20px; }
        .container { width: 300px; margin: 0 auto; padding: 20px; border: 1px solid #ccc; border-radius: 5px; }
        .form-group { margin-bottom: 15px; }
        .form-group label { display: block; margin-bottom: 5px; }
        .form-group input[type="text"], .form-group input[type="password"] { width: calc(100% - 10px); padding: 5px; }
        .error-message { color: red; margin-top: 10px; }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="container">
            <h2>Iniciar Sesión</h2>
            <div class="form-group">
                <asp:Label ID="lblEmail" runat="server" Text="Correo Electrónico:"></asp:Label>
                <asp:TextBox ID="txtEmail" runat="server" Width="95%"></asp:TextBox>
            </div>
            <div class="form-group">
                <asp:Label ID="lblPassword" runat="server" Text="Contraseña:"></asp:Label>
                <asp:TextBox ID="txtPassword" runat="server" TextMode="Password" Width="95%"></asp:TextBox>
            </div>
            <div class="form-group">
                <asp:Button ID="btnLogin" runat="server" Text="Iniciar Sesión" OnClick="btnLogin_Click" />
            </div>
            <asp:Label ID="lblMessage" runat="server" CssClass="error-message" EnableViewState="false"></asp:Label>
        </div>
    </form>
</body>
</html>