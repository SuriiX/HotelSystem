<%@ Page Title="Ver Facturas de Alojamiento" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="VerFacturasAlojamiento.aspx.cs" Inherits="HotelAuroraDreams.WebApp_Framework.Facturacion.VerFacturasAlojamiento" Async="true" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h2><%: Title %></h2>
    <asp:Label ID="lblMessage" runat="server" CssClass="text-danger" EnableViewState="false"></asp:Label>
    <asp:Label ID="lblSuccessMessage" runat="server" CssClass="text-success" EnableViewState="false"></asp:Label>

    <asp:Panel ID="pnlFiltrosFacturas" runat="server" CssClass="form-inline" style="margin-bottom: 20px; padding: 10px; border: 1px solid #eee; border-radius: 4px;">
        <div class="form-group">
            <asp:Label runat="server" AssociatedControlID="txtFiltroClienteIDFactura" Text="ID Cliente:" CssClass="control-label" />
            <asp:TextBox ID="txtFiltroClienteIDFactura" runat="server" CssClass="form-control input-sm" TextMode="Number" Width="80px" />
        </div>
        <div class="form-group" style="margin-left:10px;">
            <asp:Label runat="server" AssociatedControlID="txtFiltroReservaIDFactura" Text="ID Reserva:" CssClass="control-label" />
            <asp:TextBox ID="txtFiltroReservaIDFactura" runat="server" CssClass="form-control input-sm" TextMode="Number" Width="80px" />
        </div>
        <div class="form-group" style="margin-left:10px;">
            <asp:Label runat="server" AssociatedControlID="txtFiltroFechaDesdeFactura" Text="Emitida Desde:" CssClass="control-label" />
            <asp:TextBox ID="txtFiltroFechaDesdeFactura" runat="server" CssClass="form-control input-sm" TextMode="Date" />
        </div>
        <div class="form-group" style="margin-left:10px;">
            <asp:Label runat="server" AssociatedControlID="txtFiltroFechaHastaFactura" Text="Emitida Hasta:" CssClass="control-label" />
            <asp:TextBox ID="txtFiltroFechaHastaFactura" runat="server" CssClass="form-control input-sm" TextMode="Date" />
        </div>
         <div class="form-group" style="margin-left:10px;">
            <asp:Label runat="server" AssociatedControlID="ddlFiltroEstadoFactura" Text="Estado:" CssClass="control-label" />
            <asp:DropDownList ID="ddlFiltroEstadoFactura" runat="server" CssClass="form-control input-sm">
                <asp:ListItem Text="-- Todos --" Value=""></asp:ListItem>
                <asp:ListItem Text="Pagada" Value="Pagada"></asp:ListItem>
                <asp:ListItem Text="Pendiente" Value="Pendiente"></asp:ListItem>
                <asp:ListItem Text="Cancelada" Value="Cancelada"></asp:ListItem>
                <asp:ListItem Text="Anulada" Value="Anulada"></asp:ListItem>
            </asp:DropDownList>
        </div>
        <asp:Button ID="btnFiltrarFacturas" runat="server" Text="Filtrar" OnClick="btnFiltrarFacturas_Click" CssClass="btn btn-info btn-sm" style="margin-left:10px;" />
        <asp:Button ID="btnLimpiarFiltrosFacturas" runat="server" Text="Limpiar" OnClick="btnLimpiarFiltrosFacturas_Click" CssClass="btn btn-default btn-sm" style="margin-left:5px;" CausesValidation="false" />
    </asp:Panel>

    <asp:GridView ID="gvFacturasAlojamiento" runat="server" AutoGenerateColumns="False" CssClass="table table-striped table-bordered table-hover table-condensed"
        DataKeyNames="FacturaID" OnRowCommand="gvFacturasAlojamiento_RowCommand" AllowPaging="True" PageSize="15" OnPageIndexChanging="gvFacturasAlojamiento_PageIndexChanging">
        <Columns>
            <asp:BoundField DataField="FacturaID" HeaderText="ID Factura" ReadOnly="True" SortExpression="FacturaID" />
            <asp:BoundField DataField="ReservaID" HeaderText="ID Reserva" SortExpression="ReservaID" />
            <asp:BoundField DataField="NombreCliente" HeaderText="Cliente" SortExpression="NombreCliente" />
            <asp:BoundField DataField="FechaEmision" HeaderText="Fecha Emisión" DataFormatString="{0:yyyy-MM-dd HH:mm}" SortExpression="FechaEmision" />
            <asp:BoundField DataField="Total" HeaderText="Total" DataFormatString="{0:C}" SortExpression="Total" />
            <asp:BoundField DataField="Estado" HeaderText="Estado" SortExpression="Estado" />
            <asp:TemplateField HeaderText="Acciones">
                <ItemTemplate>
                    <asp:HyperLink ID="lnkVerPdfFactura" runat="server" Text="Ver PDF" CssClass="btn btn-xs btn-primary" Target="_blank"
                        NavigateUrl='<%# GetFacturaPdfUrl(Eval("FacturaID")) %>' ToolTip="Ver/Descargar PDF de la Factura" />
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
        <EmptyDataTemplate>
            No se encontraron facturas de alojamiento con los criterios seleccionados.
        </EmptyDataTemplate>
        <PagerStyle CssClass="pagination-ys" />
    </asp:GridView>
</asp:Content>