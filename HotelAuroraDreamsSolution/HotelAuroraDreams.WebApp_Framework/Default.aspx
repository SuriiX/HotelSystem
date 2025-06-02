<%@ Page Title="Página Principal" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="HotelAuroraDreams.WebApp_Framework._Default" Async="true" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div style="padding: 20px; border: 1px solid #e3e3e3; border-radius: 4px; background-color: #f9f9f9; margin-top: 20px;">
        <h2>
            <asp:Label ID="lblWelcomeMessage" runat="server" Text="Bienvenido al Hotel Aurora Dreams!"></asp:Label>
        </h2>
        <p class="lead">Esta es tu página de inicio después de un login exitoso.</p>
        <hr />
        <h4>Información de la API:</h4>
        <p>
            <asp:Label ID="lblApiData" runat="server" EnableViewState="false"></asp:Label>
        </p>
        <p>
            <asp:Label ID="lblMessage" runat="server" ForeColor="Red" EnableViewState="false"></asp:Label>
        </p>

        <asp:Panel ID="pnlAdminOnly" runat="server" Visible="false" style="margin-top: 20px; padding: 15px; border: 1px solid #28a745; background-color: #e6ffe6; border-radius: 4px;">
            <h3>Sección de Administrador</h3>
            <p>Este contenido solo es visible para usuarios con el rol 'Administrador'.</p>
            <p><a href="#">Acceder al Panel de Administración (Ejemplo)</a></p>
        </asp:Panel>
    </div>
</asp:Content>