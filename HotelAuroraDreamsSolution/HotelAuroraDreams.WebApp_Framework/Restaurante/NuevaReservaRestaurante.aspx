<%@ Page Title="Nueva Reserva de Restaurante" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="NuevaReservaRestaurante.aspx.cs" Inherits="HotelAuroraDreams.WebApp_Framework.Restaurante.NuevaReservaRestaurante" Async="true" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h2><%: Title %></h2>
    <asp:Label ID="lblMessage" runat="server" CssClass="text-danger" EnableViewState="false"></asp:Label>
    <asp:Label ID="lblSuccessMessage" runat="server" CssClass="text-success" EnableViewState="false"></asp:Label>

    <div class="panel panel-default">
        <div class="panel-heading">Detalles de la Reserva</div>
        <div class="panel-body form-horizontal">
            <div class="form-group">
                <asp:Label runat="server" AssociatedControlID="ddlClienteRestaurante" CssClass="col-md-2 control-label">Cliente:</asp:Label>
                <div class="col-md-10">
                    <asp:DropDownList ID="ddlClienteRestaurante" runat="server" CssClass="form-control" DataTextField="NombreCompleto" DataValueField="ClienteID" AppendDataBoundItems="true">
                    </asp:DropDownList>
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="ddlClienteRestaurante" ErrorMessage="Cliente es requerido." CssClass="text-danger" Display="Dynamic" InitialValue="0" ValidationGroup="ReservaRestVal"/>
                </div>
            </div>
            <div class="form-group">
                <asp:Label runat="server" AssociatedControlID="ddlRestaurante" CssClass="col-md-2 control-label">Restaurante:</asp:Label>
                <div class="col-md-10">
                    <asp:DropDownList ID="ddlRestaurante" runat="server" CssClass="form-control" DataTextField="Nombre" DataValueField="RestauranteID" AutoPostBack="true" OnSelectedIndexChanged="ddlRestaurante_SelectedIndexChanged" AppendDataBoundItems="true">
                    </asp:DropDownList>
                     <asp:RequiredFieldValidator runat="server" ControlToValidate="ddlRestaurante" ErrorMessage="Restaurante es requerido." CssClass="text-danger" Display="Dynamic" InitialValue="0" ValidationGroup="ReservaRestVal"/>
                </div>
            </div>
            <div class="form-group">
                <asp:Label runat="server" AssociatedControlID="txtFechaReservaRestaurante" CssClass="col-md-2 control-label">Fecha Reserva:</asp:Label>
                <div class="col-md-4">
                    <asp:TextBox ID="txtFechaReservaRestaurante" runat="server" CssClass="form-control" TextMode="Date" AutoPostBack="true" OnTextChanged="txtFechaHoraRestaurante_TextChanged" />
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="txtFechaReservaRestaurante" ErrorMessage="Fecha de reserva requerida." CssClass="text-danger" Display="Dynamic" ValidationGroup="ReservaRestVal"/>
                </div>
            </div>
            <div class="form-group">
                 <asp:Label runat="server" AssociatedControlID="txtHoraReservaRestaurante" CssClass="col-md-2 control-label">Hora Reserva:</asp:Label>
                <div class="col-md-4">
                    <asp:TextBox ID="txtHoraReservaRestaurante" runat="server" CssClass="form-control" TextMode="Time" AutoPostBack="true" OnTextChanged="txtFechaHoraRestaurante_TextChanged"/>
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="txtHoraReservaRestaurante" ErrorMessage="Hora de reserva requerida." CssClass="text-danger" Display="Dynamic" ValidationGroup="ReservaRestVal"/>
                </div>
            </div>
             <div class="form-group">
                <asp:Label runat="server" AssociatedControlID="txtNumComensalesRestaurante" CssClass="col-md-2 control-label">N° Comensales:</asp:Label>
                <div class="col-md-4">
                    <asp:TextBox ID="txtNumComensalesRestaurante" runat="server" CssClass="form-control" TextMode="Number" />
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="txtNumComensalesRestaurante" ErrorMessage="N° comensales requerido." CssClass="text-danger" Display="Dynamic" ValidationGroup="ReservaRestVal"/>
                    <asp:RangeValidator runat="server" ControlToValidate="txtNumComensalesRestaurante" MinimumValue="1" MaximumValue="100" Type="Integer" ErrorMessage="N° comensales inválido." CssClass="text-danger" Display="Dynamic" ValidationGroup="ReservaRestVal"/>
                </div>
            </div>
             <div class="form-group">s
                 <div class="col-md-offset-2 col-md-10">
                    <asp:Button ID="btnVerificarDisponibilidadRestaurante" runat="server" Text="Verificar Disponibilidad" OnClick="btnVerificarDisponibilidadRestaurante_Click" CssClass="btn btn-info" ValidationGroup="ReservaRestValFechas" />
                    <asp:Label ID="lblDisponibilidadRestaurante" runat="server" EnableViewState="false" style="margin-left:10px; font-weight:bold;"></asp:Label>
                </div>
            </div>
        </div>
    </div>

    <asp:Panel ID="pnlConfirmacionReservaRestaurante" runat="server" Visible="false" CssClass="form-horizontal" style="margin-top: 20px; padding:15px; border: 1px solid #007bff; border-radius:5px;">
        <h4>Confirmar Reserva de Restaurante</h4>
         <div class="form-group">
            <asp:Label runat="server" AssociatedControlID="txtNotasReservaRestaurante" CssClass="col-md-2 control-label">Notas Adicionales:</asp:Label>
            <div class="col-md-10">
                <asp:TextBox ID="txtNotasReservaRestaurante" runat="server" TextMode="MultiLine" Rows="3" CssClass="form-control" />
            </div>
        </div>
        <div class="form-group">
            <div class="col-md-offset-2 col-md-10">
                <asp:Button ID="btnConfirmarReservaRestaurante" runat="server" Text="Confirmar Reserva" OnClick="btnConfirmarReservaRestaurante_Click" CssClass="btn btn-primary" ValidationGroup="ReservaRestVal" />
            </div>
        </div>
    </asp:Panel>
</asp:Content>