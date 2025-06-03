<%@ Page Title="Proceso de Check-Out" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="CheckOut.aspx.cs" Inherits="HotelAuroraDreams.WebApp_Framework.Operaciones.CheckOutPage" Async="true" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h2><%: Title %></h2>
    <asp:Label ID="lblMessage" runat="server" CssClass="text-danger" EnableViewState="false"></asp:Label>
    <asp:Label ID="lblSuccessMessage" runat="server" CssClass="text-success" EnableViewState="false"></asp:Label>

    <div class="row" style="margin-bottom:20px;">
        <div class="col-md-4">
            <asp:Label runat="server" AssociatedControlID="txtBusquedaReservaID_CheckOut" Text="Buscar por ID Reserva (Hospedada):" />
            <asp:TextBox ID="txtBusquedaReservaID_CheckOut" runat="server" CssClass="form-control" TextMode="Number" />
        </div>
        <div class="col-md-2" style="padding-top:25px;">
            <asp:Button ID="btnBuscarReserva_CheckOut" runat="server" Text="Buscar Reserva para Check-Out" OnClick="btnBuscarReserva_CheckOut_Click" CssClass="btn btn-primary" />
        </div>
    </div>

    <asp:Panel ID="pnlDetallesReserva_CheckOut" runat="server" Visible="false" CssClass="panel panel-info">
        <div class="panel-heading"><h3 class="panel-title">Detalles de la Reserva para Check-Out</h3></div>
        <div class="panel-body">
            <p><strong>ID Reserva:</strong> <asp:Literal ID="litReservaID_CheckOut" runat="server" /></p>
            <p><strong>Cliente:</strong> <asp:Literal ID="litClienteNombre_CheckOut" runat="server" /></p>
            <p><strong>Hotel:</strong> <asp:Literal ID="litHotelNombre_CheckOut" runat="server" /></p>
            <p><strong>Fechas:</strong> <asp:Literal ID="litFechas_CheckOut" runat="server" /></p>
            <p><strong>Estado Actual:</strong> <asp:Literal ID="litEstadoReserva_CheckOut" runat="server" /></p>
            <%-- Aquí podríamos mostrar un resumen de cargos si la API lo proveyera antes del POST de CheckOut --%>
        </div>
    </asp:Panel>

    <asp:Panel ID="pnlFormCheckOut" runat="server" Visible="false" CssClass="form-horizontal" style="margin-top: 20px; padding:15px; border: 1px solid #d9534f; border-radius:5px;">
        <h3>Confirmar Check-Out y Pago</h3>
        <asp:HiddenField ID="hfReservaID_CheckOut_Form" runat="server" />

        <div class="form-group">
            <asp:Label runat="server" AssociatedControlID="ddlMetodoPagoFinal" CssClass="col-md-3 control-label">Método de Pago Final:</asp:Label>
            <div class="col-md-9">
                <asp:DropDownList ID="ddlMetodoPagoFinal" runat="server" CssClass="form-control">
                    <asp:ListItem Text="Tarjeta" Value="tarjeta"></asp:ListItem>
                    <asp:ListItem Text="Efectivo" Value="efectivo"></asp:ListItem>
                    <asp:ListItem Text="Transferencia" Value="transferencia"></asp:ListItem>
                    <asp:ListItem Text="Otro" Value="otro"></asp:ListItem>
                </asp:DropDownList>
                <asp:RequiredFieldValidator runat="server" ControlToValidate="ddlMetodoPagoFinal" ErrorMessage="Método de pago es requerido." CssClass="text-danger" Display="Dynamic" ValidationGroup="CheckOutValidation" />
            </div>
        </div>
        <div class="form-group">
            <asp:Label runat="server" AssociatedControlID="txtObservacionesCheckOut" CssClass="col-md-3 control-label">Observaciones:</asp:Label>
            <div class="col-md-9">
                <asp:TextBox runat="server" ID="txtObservacionesCheckOut" TextMode="MultiLine" Rows="3" CssClass="form-control" />
            </div>
        </div>
        <div class="form-group">
            <div class="col-md-offset-3 col-md-9">
                <asp:Button runat="server" ID="btnRealizarCheckOut" Text="Realizar Check-Out y Facturar" OnClick="btnRealizarCheckOut_Click" CssClass="btn btn-danger" ValidationGroup="CheckOutValidation" />
            </div>
        </div>
    </asp:Panel>

    <asp:Panel ID="pnlResultadoFactura" runat="server" Visible="false" CssClass="panel panel-success" style="margin-top:20px;">
         <div class="panel-heading"><h3 class="panel-title">Check-Out y Facturación Completados</h3></div>
         <div class="panel-body">
             <p><strong>ID Check-Out:</strong> <asp:Literal ID="litCheckOutIDResultado" runat="server" /></p>
             <p><strong>Cliente:</strong> <asp:Literal ID="litClienteFacturaResultado" runat="server" /></p>
             <p><strong>Total Facturado:</strong> <asp:Literal ID="litTotalFacturaResultado" runat="server" /></p>
             <p><strong>ID Factura Generada:</strong> <asp:Literal ID="litFacturaIDResultado" runat="server" /></p>
             <p><asp:HyperLink ID="lnkVerFacturaPdf" runat="server" Text="Ver/Descargar PDF Factura" Target="_blank" CssClass="btn btn-info" Visible="false" /></p>
         </div>
    </asp:Panel>

</asp:Content>