<%@ Page Title="Proceso de Check-In" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="CheckIn.aspx.cs" Inherits="HotelAuroraDreams.WebApp_Framework.Operaciones.CheckInPage" Async="true" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h2><%: Title %></h2>
    <asp:Label ID="lblMessage" runat="server" CssClass="text-danger" EnableViewState="false"></asp:Label>
    <asp:Label ID="lblSuccessMessage" runat="server" CssClass="text-success" EnableViewState="false"></asp:Label>

    <div class="row" style="margin-bottom:20px;">
        <div class="col-md-4">
            <asp:Label runat="server" AssociatedControlID="txtBusquedaReservaID" Text="Buscar por ID Reserva:" />
            <asp:TextBox ID="txtBusquedaReservaID" runat="server" CssClass="form-control" TextMode="Number" />
        </div>
        <div class="col-md-2" style="padding-top:25px;">
            <asp:Button ID="btnBuscarReserva" runat="server" Text="Buscar Reserva" OnClick="btnBuscarReserva_Click" CssClass="btn btn-primary" />
        </div>
    </div>

    <asp:Panel ID="pnlDetallesReserva" runat="server" Visible="false" CssClass="panel panel-info">
        <div class="panel-heading"><h3 class="panel-title">Detalles de la Reserva a Procesar</h3></div>
        <div class="panel-body">
            <p><strong>ID Reserva:</strong> <asp:Literal ID="litReservaID" runat="server" /></p>
            <p><strong>Cliente:</strong> <asp:Literal ID="litClienteNombre" runat="server" /></p>
            <p><strong>Hotel:</strong> <asp:Literal ID="litHotelNombre" runat="server" /></p>
            <p><strong>Fecha Entrada:</strong> <asp:Literal ID="litFechaEntrada" runat="server" /></p>
            <p><strong>Fecha Salida:</strong> <asp:Literal ID="litFechaSalida" runat="server" /></p>
            <p><strong>Huéspedes:</strong> <asp:Literal ID="litNumeroHuespedes" runat="server" /></p>
            <p><strong>Estado Actual:</strong> <asp:Literal ID="litEstadoReserva" runat="server" /></p>
            <h4>Habitaciones Asignadas:</h4>
            <asp:Repeater ID="rptHabitacionesCheckIn" runat="server">
                <HeaderTemplate><ul class="list-group"></HeaderTemplate>
                <ItemTemplate>
                    <li class="list-group-item">
                        N°: <%# Eval("NumeroHabitacion") %> (<%# Eval("NombreTipoHabitacion") %>) - Precio/Noche: <%# Eval("PrecioNocheCobrado", "{0:C}") %>
                    </li>
                </ItemTemplate>
                <FooterTemplate></ul></FooterTemplate>
            </asp:Repeater>
        </div>
    </asp:Panel>

    <asp:Panel ID="pnlFormCheckIn" runat="server" Visible="false" CssClass="form-horizontal" style="margin-top: 20px; padding:15px; border: 1px solid #5cb85c; border-radius:5px;">
        <h3>Confirmar Check-In</h3>
        <asp:HiddenField ID="hfReservaIDCheckIn" runat="server" />

        <div class="form-group">
            <asp:Label runat="server" AssociatedControlID="txtMetodoPagoAdelanto" CssClass="col-md-3 control-label">Método Pago Adelanto (Opc):</asp:Label>
            <div class="col-md-9">
                <asp:TextBox runat="server" ID="txtMetodoPagoAdelanto" CssClass="form-control" />
            </div>
        </div>
        <div class="form-group">
            <asp:Label runat="server" AssociatedControlID="chkDocumentosVerificados" CssClass="col-md-3 control-label">Documentos Verificados:</asp:Label>
            <div class="col-md-9">
                <asp:CheckBox ID="chkDocumentosVerificados" runat="server" />
            </div>
        </div>
        <div class="form-group">
            <asp:Label runat="server" AssociatedControlID="txtObservacionesCheckIn" CssClass="col-md-3 control-label">Observaciones:</asp:Label>
            <div class="col-md-9">
                <asp:TextBox runat="server" ID="txtObservacionesCheckIn" TextMode="MultiLine" Rows="3" CssClass="form-control" />
            </div>
        </div>
        <div class="form-group">
            <div class="col-md-offset-3 col-md-9">
                <asp:Button runat="server" ID="btnRealizarCheckIn" Text="Realizar Check-In" OnClick="btnRealizarCheckIn_Click" CssClass="btn btn-success" />
            </div>
        </div>
    </asp:Panel>
</asp:Content>