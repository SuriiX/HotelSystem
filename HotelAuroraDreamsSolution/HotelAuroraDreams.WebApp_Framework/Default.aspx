<%@ Page Title="Página Principal" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="HotelAuroraDreams.WebApp_Framework._Default" Async="true" %> <%-- Añadido Async="true" --%>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <div class="jumbotron">
        <h1><asp:Label ID="lblWelcomeMessage" runat="server" Text="Bienvenido al Hotel Aurora Dreams!"></asp:Label></h1>
        <p class="lead">Esta es tu página de inicio después de un login exitoso.</p>
        <asp:Label ID="lblApiData" runat="server" EnableViewState="false"></asp:Label>
        <br />
        <asp:Label ID="lblMessage" runat="server" ForeColor="Red" EnableViewState="false"></asp:Label>
    </div>

</asp:Content>