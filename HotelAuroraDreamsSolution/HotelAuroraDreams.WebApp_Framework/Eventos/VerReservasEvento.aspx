<%@ Page Title="Ver Reservas de Eventos" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="VerReservasEvento.aspx.cs" Inherits="HotelAuroraDreams.WebApp_Framework.Eventos.VerReservasEvento" Async="true" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h2><%: Title %></h2>
    <asp:Label ID="lblMessage" runat="server" CssClass="text-danger" EnableViewState="false"></asp:Label>
    <asp:Label ID="lblSuccessMessage" runat="server" CssClass="text-success" EnableViewState="false"></asp:Label>

    <asp:Panel ID="pnlFiltrosReservasEvento" runat="server" CssClass="form-inline" style="margin-bottom: 20px;">
        <div class="form-group">
            <asp:Label runat="server" AssociatedControlID="txtFiltroClienteIDEvento" Text="ID Cliente:" CssClass="control-label" />
            <asp:TextBox ID="txtFiltroClienteIDEvento" runat="server" CssClass="form-control" TextMode="Number" Width="80px" />
        </div>
        <div class="form-group" style="margin-left:10px;">
            <asp:Label runat="server" AssociatedControlID="ddlFiltroSalonEvento" Text="Salón:" CssClass="control-label" />
            <asp:DropDownList ID="ddlFiltroSalonEvento" runat="server" CssClass="form-control" DataTextField="Nombre" DataValueField="SalonEventoID" AppendDataBoundItems="true">
                <asp:ListItem Text="-- Todos --" Value="0"></asp:ListItem>
            </asp:DropDownList>
        </div>
        <div class="form-group" style="margin-left:10px;">
            <asp:Label runat="server" AssociatedControlID="txtFiltroFechaEvento" Text="Fecha Evento:" CssClass="control-label" />
            <asp:TextBox ID="txtFiltroFechaEvento" runat="server" CssClass="form-control" TextMode="Date" />
        </div>
        <div class="form-group" style="margin-left:10px;">
            <asp:Label runat="server" AssociatedControlID="ddlFiltroEstadoEvento" Text="Estado:" CssClass="control-label" />
            <asp:DropDownList ID="ddlFiltroEstadoEvento" runat="server" CssClass="form-control">
                <asp:ListItem Text="-- Todos --" Value=""></asp:ListItem>
                <asp:ListItem Text="Solicitada" Value="Solicitada"></asp:ListItem>
                <asp:ListItem Text="Confirmada" Value="Confirmada"></asp:ListItem>
                <asp:ListItem Text="En Curso" Value="En Curso"></asp:ListItem>
                <asp:ListItem Text="Realizada" Value="Realizada"></asp:ListItem>
                <asp:ListItem Text="Cancelada" Value="Cancelada"></asp:ListItem>
            </asp:DropDownList>
        </div>
        <asp:Button ID="btnFiltrarReservasEvento" runat="server" Text="Filtrar Reservas" OnClick="btnFiltrarReservasEvento_Click" CssClass="btn btn-info" style="margin-left:10px;" />
        <asp:Button ID="btnLimpiarFiltrosReservasEvento" runat="server" Text="Limpiar" OnClick="btnLimpiarFiltrosReservasEvento_Click" CssClass="btn btn-default" style="margin-left:5px;" CausesValidation="false" />
    </asp:Panel>

    <asp:GridView ID="gvReservasEvento" runat="server" AutoGenerateColumns="False" CssClass="table table-striped table-bordered table-hover"
        DataKeyNames="ReservaEventoID" OnRowCommand="gvReservasEvento_RowCommand" AllowPaging="True" PageSize="10" OnPageIndexChanging="gvReservasEvento_PageIndexChanging" >
        <Columns>
            <asp:BoundField DataField="ReservaEventoID" HeaderText="ID Reserva Ev." ReadOnly="True" />
            <asp:BoundField DataField="NombreEvento" HeaderText="Nombre Evento" />
            <asp:BoundField DataField="NombreCliente" HeaderText="Cliente" /> <%-- Asumiendo que el ViewModel simplificado para el grid tiene esto --%>
            <asp:BoundField DataField="NombreSalon" HeaderText="Salón" /> <%-- Asumiendo que el ViewModel simplificado para el grid tiene esto --%>
            <asp:BoundField DataField="FechaEvento" HeaderText="Fecha Evento" DataFormatString="{0:yyyy-MM-dd}" />
            <asp:BoundField DataField="HoraInicio" HeaderText="Hora Inicio" />
            <asp:BoundField DataField="Estado" HeaderText="Estado" />
            <asp:TemplateField HeaderText="Acciones">
                <ItemTemplate>
                    <asp:LinkButton ID="lnkVerDetallesEvento" runat="server" CommandName="VerDetallesEvento" CommandArgument='<%# Eval("ReservaEventoID") %>' Text="Detalles" CssClass="btn btn-xs btn-default"></asp:LinkButton>
                    <asp:LinkButton ID="lnkEditarReservaEvento" runat="server" CommandName="EditarReservaEvento" CommandArgument='<%# Eval("ReservaEventoID") %>' Text="Editar" CssClass="btn btn-xs btn-info"></asp:LinkButton>
                    <asp:LinkButton ID="lnkCancelarReservaEvento" runat="server" CommandName="CancelarReservaEvento" CommandArgument='<%# Eval("ReservaEventoID") %>' Text="Cancelar" CssClass="btn btn-xs btn-warning" OnClientClick="return confirm('¿Está seguro de que desea cancelar esta reserva de evento?');"></asp:LinkButton>
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
        <EmptyDataTemplate>
            No se encontraron reservas de evento con los criterios seleccionados.
        </EmptyDataTemplate>
    </asp:GridView>

    <asp:Panel ID="pnlDetallesReservaEvento" runat="server" Visible="false" CssClass="panel panel-info" style="margin-top:20px;">
        <div class="panel-heading">
            <h3 class="panel-title">Detalles de la Reserva de Evento #<asp:Literal ID="litReservaEventoIDDetalle" runat="server"></asp:Literal></h3>
        </div>
        <div class="panel-body">
            <p><strong>Nombre Evento:</strong> <asp:Literal ID="litNombreEventoDetalle" runat="server" /></p>
            <p><strong>Cliente:</strong> <asp:Literal ID="litClienteEventoDetalle" runat="server" /></p>
            <p><strong>Salón:</strong> <asp:Literal ID="litSalonEventoDetalle" runat="server" /></p>
            <p><strong>Tipo Evento:</strong> <asp:Literal ID="litTipoEventoDetalle" runat="server" /></p>
            <p><strong>Fecha y Hora:</strong> <asp:Literal ID="litFechaHoraEventoDetalle" runat="server" /></p>
            <p><strong>Asistentes (Est.):</strong> <asp:Literal ID="litAsistentesEstDetalle" runat="server" /></p>
            <p><strong>Asistentes (Conf.):</strong> <asp:Literal ID="litAsistentesConfDetalle" runat="server" /></p>
            <p><strong>Estado:</strong> <asp:Literal ID="litEstadoEventoDetalle" runat="server" /></p>
            <p><strong>Monto Salón (Est.):</strong> <asp:Literal ID="litMontoSalonDetalle" runat="server" /></p>
            <p><strong>Monto Servicios (Est.):</strong> <asp:Literal ID="litMontoServiciosDetalle" runat="server" /></p>
            <p><strong>Monto Total Evento (Est.):</strong> <asp:Literal ID="litMontoTotalEventoDetalle" runat="server" /></p>
            <p><strong>Notas:</strong> <asp:Literal ID="litNotasEventoDetalle" runat="server" /></p>
            <h4>Servicios Adicionales Contratados:</h4>
            <asp:Repeater ID="rptServiciosEventoDetalle" runat="server">
                <HeaderTemplate><ul class="list-group"></HeaderTemplate>
                <ItemTemplate>
                    <li class="list-group-item">
                        <%# Eval("NombreServicio") %> (Cant: <%# Eval("Cantidad") %>) - Precio Unit: <%# Eval("PrecioCobradoPorUnidad", "{0:C}") %> - Subtotal: <%# Eval("Subtotal", "{0:C}") %>
                        <br /><small><i>Notas: <%# Eval("Notas") %></i></small>
                    </li>
                </ItemTemplate>
                <FooterTemplate></ul></FooterTemplate>
            </asp:Repeater>
            <br />
            <asp:Button ID="btnCerrarDetallesEvento" runat="server" Text="Cerrar Detalles" OnClick="btnCerrarDetallesEvento_Click" CssClass="btn btn-default" CausesValidation="false" />
        </div>
    </asp:Panel>
    <asp:Panel ID="pnlEditarReservaEvento" runat="server" Visible="false" CssClass="form-horizontal" style="margin-top: 20px; padding:15px; border: 1px solid #f0ad4e; border-radius:5px;">
        <h3>Editar Reserva de Evento #<asp:Literal ID="litReservaEventoIDEditar" runat="server"></asp:Literal></h3>
        <asp:HiddenField ID="hfReservaEventoIDEditar" runat="server" />
         <div class="form-group">
            <asp:Label runat="server" AssociatedControlID="txtEditNombreEvento" CssClass="col-md-3 control-label">Nombre Evento:</asp:Label>
            <div class="col-md-9">
                <asp:TextBox ID="txtEditNombreEvento" runat="server" CssClass="form-control" />
            </div>
        </div>
        <div class="form-group">
            <asp:Label runat="server" AssociatedControlID="txtEditNumAsistentesEst" CssClass="col-md-3 control-label">N° Asistentes (Est.):</asp:Label>
            <div class="col-md-9">
                <asp:TextBox ID="txtEditNumAsistentesEst" runat="server" CssClass="form-control" TextMode="Number" />
            </div>
        </div>
        <div class="form-group">
            <asp:Label runat="server" AssociatedControlID="txtEditNumAsistentesConf" CssClass="col-md-3 control-label">N° Asistentes (Conf.):</asp:Label>
            <div class="col-md-9">
                <asp:TextBox ID="txtEditNumAsistentesConf" runat="server" CssClass="form-control" TextMode="Number" />
            </div>
        </div>
        <div class="form-group">
            <asp:Label runat="server" AssociatedControlID="txtEditNotasEvento" CssClass="col-md-3 control-label">Notas Generales:</asp:Label>
            <div class="col-md-9">
                <asp:TextBox ID="txtEditNotasEvento" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3"/>
            </div>
        </div>
        <div class="form-group">
            <asp:Label runat="server" AssociatedControlID="ddlEditEstadoEvento" CssClass="col-md-3 control-label">Estado:</asp:Label>
            <div class="col-md-9">
                 <asp:DropDownList ID="ddlEditEstadoEvento" runat="server" CssClass="form-control">
                    <asp:ListItem Text="Solicitada" Value="Solicitada"></asp:ListItem>
                    <asp:ListItem Text="Confirmada" Value="Confirmada"></asp:ListItem>
                    <asp:ListItem Text="En Curso" Value="En Curso"></asp:ListItem>
                    <asp:ListItem Text="Realizada" Value="Realizada"></asp:ListItem>
                </asp:DropDownList>
            </div>
        </div>
        <div class="form-group"> 
            <asp:Label runat="server" CssClass="col-md-3 control-label">Servicios Adicionales:</asp:Label>
            <div class="col-md-9">
                <asp:Literal ID="litEditServiciosInfo" runat="server" Text="La edición de servicios detallados se implementará. Por ahora, use el formulario de creación para modificar servicios (cancelando y creando de nuevo si es necesario)." />
            </div>
        </div>
        <div class="form-group">
            <div class="col-md-offset-3 col-md-9">
                <asp:Button ID="btnGuardarEdicionReservaEvento" runat="server" Text="Guardar Cambios" OnClick="btnGuardarEdicionReservaEvento_Click" CssClass="btn btn-primary" ValidationGroup="EditReservaEventoVal" />
                <asp:Button ID="btnCancelarEdicionReservaEvento" runat="server" Text="Cancelar Edición" OnClick="btnCancelarEdicionReservaEvento_Click" CssClass="btn btn-default" CausesValidation="false" />
            </div>
        </div>
    </asp:Panel>

</asp:Content>