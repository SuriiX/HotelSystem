<%@ Page Title="Ver Reservas de Habitaciones" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="VerReservas.aspx.cs" Inherits="HotelAuroraDreams.WebApp_Framework.Reservas.VerReservas" Async="true" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h2><%: Title %></h2>
    <asp:Label ID="lblMessage" runat="server" CssClass="text-danger" EnableViewState="false"></asp:Label>
    <asp:Label ID="lblSuccessMessage" runat="server" CssClass="text-success" EnableViewState="false"></asp:Label>

    <asp:Panel ID="pnlFiltros" runat="server" CssClass="form-inline" style="margin-bottom: 20px;">
        <div class="form-group">
            <asp:Label runat="server" AssociatedControlID="txtFiltroClienteID" Text="ID Cliente:" CssClass="control-label" />
            <asp:TextBox ID="txtFiltroClienteID" runat="server" CssClass="form-control" TextMode="Number" Width="80px" />
        </div>
        <div class="form-group" style="margin-left:10px;">
            <asp:Label runat="server" AssociatedControlID="ddlFiltroHotel" Text="Hotel:" CssClass="control-label" />
            <asp:DropDownList ID="ddlFiltroHotel" runat="server" CssClass="form-control" DataTextField="Nombre" DataValueField="HotelID" AppendDataBoundItems="true">
            </asp:DropDownList>
        </div>
        <div class="form-group" style="margin-left:10px;">
            <asp:Label runat="server" AssociatedControlID="txtFiltroFechaDesde" Text="Desde:" CssClass="control-label" />
            <asp:TextBox ID="txtFiltroFechaDesde" runat="server" CssClass="form-control" TextMode="Date" />
        </div>
        <div class="form-group" style="margin-left:10px;">
            <asp:Label runat="server" AssociatedControlID="txtFiltroFechaHasta" Text="Hasta:" CssClass="control-label" />
            <asp:TextBox ID="txtFiltroFechaHasta" runat="server" CssClass="form-control" TextMode="Date" />
        </div>
        <div class="form-group" style="margin-left:10px;">
            <asp:Label runat="server" AssociatedControlID="ddlFiltroEstado" Text="Estado:" CssClass="control-label" />
            <asp:DropDownList ID="ddlFiltroEstado" runat="server" CssClass="form-control">
                <asp:ListItem Text="-- Todos --" Value=""></asp:ListItem>
                <asp:ListItem Text="Pendiente" Value="Pendiente"></asp:ListItem>
                <asp:ListItem Text="Confirmada" Value="Confirmada"></asp:ListItem>
                <asp:ListItem Text="Hospedado" Value="Hospedado"></asp:ListItem>
                <asp:ListItem Text="Completada" Value="Completada"></asp:ListItem>
                <asp:ListItem Text="Cancelada" Value="Cancelada"></asp:ListItem>
                <asp:ListItem Text="No Show" Value="No_Show"></asp:ListItem>
            </asp:DropDownList>
        </div>
        <asp:Button ID="btnFiltrar" runat="server" Text="Filtrar" OnClick="btnFiltrar_Click" CssClass="btn btn-info" style="margin-left:10px;" />
        <asp:Button ID="btnLimpiarFiltros" runat="server" Text="Limpiar" OnClick="btnLimpiarFiltros_Click" CssClass="btn btn-default" style="margin-left:5px;" CausesValidation="false" />
    </asp:Panel>

    <asp:GridView ID="gvReservas" runat="server" AutoGenerateColumns="False" CssClass="table table-striped table-bordered table-hover"
        DataKeyNames="ReservaID" OnRowCommand="gvReservas_RowCommand" AllowPaging="True" PageSize="10" OnPageIndexChanging="gvReservas_PageIndexChanging" >
        <Columns>
            <asp:BoundField DataField="ReservaID" HeaderText="ID Reserva" ReadOnly="True" />
            <asp:BoundField DataField="NombreCliente" HeaderText="Cliente" />
            <asp:BoundField DataField="NombreHotel" HeaderText="Hotel" />
            <asp:BoundField DataField="FechaEntrada" HeaderText="Entrada" DataFormatString="{0:yyyy-MM-dd}" />
            <asp:BoundField DataField="FechaSalida" HeaderText="Salida" DataFormatString="{0:yyyy-MM-dd}" />
            <asp:BoundField DataField="NumeroHuespedes" HeaderText="Huéspedes" />
            <asp:BoundField DataField="Estado" HeaderText="Estado" />
            <asp:TemplateField HeaderText="Acciones">
                <ItemTemplate>
                    <asp:LinkButton ID="lnkVerDetalles" runat="server" CommandName="VerDetalles" CommandArgument='<%# Eval("ReservaID") %>' Text="Detalles" CssClass="btn btn-xs btn-default"></asp:LinkButton>
                    <asp:LinkButton ID="lnkEditarReserva" runat="server" CommandName="EditarReserva" CommandArgument='<%# Eval("ReservaID") %>' Text="Editar" CssClass="btn btn-xs btn-info"></asp:LinkButton>
                    <asp:LinkButton ID="lnkCancelarReserva" runat="server" CommandName="CancelarReserva" CommandArgument='<%# Eval("ReservaID") %>' Text="Cancelar" CssClass="btn btn-xs btn-warning" OnClientClick="return confirm('¿Está seguro de que desea cancelar esta reserva?');"></asp:LinkButton>
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
        <EmptyDataTemplate>
            No se encontraron reservas con los criterios seleccionados.
        </EmptyDataTemplate>
    </asp:GridView>

    <asp:Panel ID="pnlDetallesReserva" runat="server" Visible="false" CssClass="panel panel-info" style="margin-top:20px;">
        <div class="panel-heading">
            <h3 class="panel-title">Detalles de la Reserva #<asp:Literal ID="litReservaIDDetalle" runat="server"></asp:Literal></h3>
        </div>
        <div class="panel-body">
            <p><strong>Cliente:</strong> <asp:Literal ID="litClienteDetalle" runat="server"></asp:Literal></p>
            <p><strong>Hotel:</strong> <asp:Literal ID="litHotelDetalle" runat="server"></asp:Literal></p>
            <p><strong>Fechas:</strong> <asp:Literal ID="litFechasDetalle" runat="server"></asp:Literal></p>
            <p><strong>Huéspedes:</strong> <asp:Literal ID="litHuespedesDetalle" runat="server"></asp:Literal></p>
            <p><strong>Estado:</strong> <asp:Literal ID="litEstadoDetalle" runat="server"></asp:Literal></p>
            <p><strong>Monto Total Estimado:</strong> <asp:Literal ID="litMontoTotalDetalle" runat="server"></asp:Literal></p>
            <p><strong>Notas:</strong> <asp:Literal ID="litNotasDetalle" runat="server"></asp:Literal></p>
            <h4>Habitaciones Asignadas:</h4>
            <asp:Repeater ID="rptHabitacionesReservadas" runat="server">
                <HeaderTemplate><ul class="list-group"></HeaderTemplate>
                <ItemTemplate>
                    <li class="list-group-item">
                        Hab. N°: <%# Eval("NumeroHabitacion") %> (<%# Eval("NombreTipoHabitacion") %>) - Precio/Noche: <%# Eval("PrecioNocheCobrado", "{0:C}") %>
                    </li>
                </ItemTemplate>
                <FooterTemplate></ul></FooterTemplate>
            </asp:Repeater>
            <br />
            <asp:Button ID="btnCerrarDetalles" runat="server" Text="Cerrar Detalles" OnClick="btnCerrarDetalles_Click" CssClass="btn btn-default" CausesValidation="false" />
        </div>
    </asp:Panel>

    <asp:Panel ID="pnlEditarReserva" runat="server" Visible="false" CssClass="form-horizontal" style="margin-top: 20px; padding:15px; border: 1px solid #f0ad4e; border-radius:5px;">
        <h3>Editar Reserva #<asp:Literal ID="litReservaIDEditar" runat="server"></asp:Literal></h3>
        <asp:HiddenField ID="hfReservaIDEditar" runat="server" />
        <div class="form-group">
            <asp:Label runat="server" AssociatedControlID="txtEditNumeroHuespedes" CssClass="col-md-3 control-label">N° Huéspedes:</asp:Label>
            <div class="col-md-9">
                <asp:TextBox ID="txtEditNumeroHuespedes" runat="server" CssClass="form-control" TextMode="Number" />
                <asp:RequiredFieldValidator runat="server" ControlToValidate="txtEditNumeroHuespedes" ErrorMessage="N° Huéspedes es requerido." CssClass="text-danger" ValidationGroup="EditReservaVal" />
            </div>
        </div>
        <div class="form-group">
            <asp:Label runat="server" AssociatedControlID="txtEditNotas" CssClass="col-md-3 control-label">Notas:</asp:Label>
            <div class="col-md-9">
                <asp:TextBox ID="txtEditNotas" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3" />
            </div>
        </div>
        <div class="form-group">
            <asp:Label runat="server" AssociatedControlID="ddlEditEstado" CssClass="col-md-3 control-label">Estado:</asp:Label>
            <div class="col-md-9">
                 <asp:DropDownList ID="ddlEditEstado" runat="server" CssClass="form-control">
                    <asp:ListItem Text="Pendiente" Value="Pendiente"></asp:ListItem>
                    <asp:ListItem Text="Confirmada" Value="Confirmada"></asp:ListItem>
                    <asp:ListItem Text="Hospedado" Value="Hospedado"></asp:ListItem>
                    <%-- No permitir cambiar a Completada o Cancelada desde aquí directamente --%>
                    <asp:ListItem Text="No Show" Value="No_Show"></asp:ListItem>
                </asp:DropDownList>
            </div>
        </div>
        <div class="form-group">
            <div class="col-md-offset-3 col-md-9">
                <asp:Button ID="btnGuardarEdicionReserva" runat="server" Text="Guardar Cambios" OnClick="btnGuardarEdicionReserva_Click" CssClass="btn btn-primary" ValidationGroup="EditReservaVal" />
                <asp:Button ID="btnCancelarEdicionReserva" runat="server" Text="Cancelar Edición" OnClick="btnCancelarEdicionReserva_Click" CssClass="btn btn-default" CausesValidation="false" />
            </div>
        </div>
    </asp:Panel>

</asp:Content>