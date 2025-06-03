<%@ Page Title="Ver Facturas de Eventos" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="VerFacturasEvento.aspx.cs" Inherits="HotelAuroraDreams.WebApp_Framework.Facturacion.VerFacturasEvento" Async="true" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h2><%: Title %></h2>
    <asp:Label ID="lblMessage" runat="server" CssClass="text-danger" EnableViewState="false"></asp:Label>
    <asp:Label ID="lblSuccessMessage" runat="server" CssClass="text-success" EnableViewState="false"></asp:Label>

    <asp:Panel ID="pnlFiltrosFacturasEvento" runat="server" CssClass="form-inline" style="margin-bottom: 20px;">
        <div class="form-group">
            <asp:Label runat="server" AssociatedControlID="txtFiltroClienteIDFacturaEv" Text="ID Cliente:" CssClass="control-label" />
            <asp:TextBox ID="txtFiltroClienteIDFacturaEv" runat="server" CssClass="form-control" TextMode="Number" Width="80px" />
        </div>
        <div class="form-group" style="margin-left:10px;">
            <asp:Label runat="server" AssociatedControlID="txtFiltroReservaEventoIDFacturaEv" Text="ID Reserva Ev.:" CssClass="control-label" />
            <asp:TextBox ID="txtFiltroReservaEventoIDFacturaEv" runat="server" CssClass="form-control" TextMode="Number" Width="80px" />
        </div>
        <div class="form-group" style="margin-left:10px;">
            <asp:Label runat="server" AssociatedControlID="txtFiltroFechaDesdeFacturaEv" Text="Emitida Desde:" CssClass="control-label" />
            <asp:TextBox ID="txtFiltroFechaDesdeFacturaEv" runat="server" CssClass="form-control" TextMode="Date" />
        </div>
        <div class="form-group" style="margin-left:10px;">
            <asp:Label runat="server" AssociatedControlID="txtFiltroFechaHastaFacturaEv" Text="Emitida Hasta:" CssClass="control-label" />
            <asp:TextBox ID="txtFiltroFechaHastaFacturaEv" runat="server" CssClass="form-control" TextMode="Date" />
        </div>
        <asp:Button ID="btnFiltrarFacturasEvento" runat="server" Text="Filtrar Facturas" OnClick="btnFiltrarFacturasEvento_Click" CssClass="btn btn-info" style="margin-left:10px;" />
        <asp:Button ID="btnLimpiarFiltrosFacturasEvento" runat="server" Text="Limpiar Filtros" OnClick="btnLimpiarFiltrosFacturasEvento_Click" CssClass="btn btn-default" style="margin-left:5px;" CausesValidation="false" />
    </asp:Panel>

    <asp:GridView ID="gvFacturasEvento" runat="server" AutoGenerateColumns="False" CssClass="table table-striped table-bordered table-hover"
        DataKeyNames="FacturaEventoID" OnRowCommand="gvFacturasEvento_RowCommand" AllowPaging="True" PageSize="10" OnPageIndexChanging="gvFacturasEvento_PageIndexChanging">
        <Columns>
            <asp:BoundField DataField="FacturaEventoID" HeaderText="ID Factura Ev." ReadOnly="True" />
            <asp:BoundField DataField="ReservaEventoID" HeaderText="ID Reserva Ev." />
            <asp:BoundField DataField="NombreEvento" HeaderText="Nombre Evento" />
            <asp:BoundField DataField="NombreCliente" HeaderText="Cliente" />
            <asp:BoundField DataField="FechaEmision" HeaderText="Fecha Emisión" DataFormatString="{0:yyyy-MM-dd HH:mm}" />
            <asp:BoundField DataField="TotalFactura" HeaderText="Total" DataFormatString="{0:C}" />
            <asp:BoundField DataField="Estado" HeaderText="Estado" />
            <asp:TemplateField HeaderText="Acciones">
                <ItemTemplate>
                    <asp:HyperLink ID="lnkVerPdfFacturaEvento" runat="server" Text="Ver PDF" CssClass="btn btn-xs btn-primary" Target="_blank"
                        NavigateUrl='<%# GetFacturaEventoPdfUrl(Eval("FacturaEventoID")) %>' />
                    <%-- Podríamos añadir un botón para ver detalles completos en la página si es necesario --%>
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
        <EmptyDataTemplate>
            No se encontraron facturas de evento con los criterios seleccionados.
        </EmptyDataTemplate>
    </asp:GridView>
</asp:Content>