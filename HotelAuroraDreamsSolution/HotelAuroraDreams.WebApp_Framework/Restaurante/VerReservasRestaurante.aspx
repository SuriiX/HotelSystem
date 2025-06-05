<%@ Page Title="Ver Reservas de Restaurante" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="VerReservasRestaurante.aspx.cs" Inherits="HotelAuroraDreams.WebApp_Framework.Restaurante.VerReservasRestaurante" Async="true" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h2><%: Title %></h2>
    <asp:Label ID="lblMessage" runat="server" CssClass="text-danger" EnableViewState="false"></asp:Label>
    <asp:Label ID="lblSuccessMessage" runat="server" CssClass="text-success" EnableViewState="false"></asp:Label>

    <asp:Panel ID="pnlFiltrosReservasRest" runat="server" CssClass="form-inline" style="margin-bottom: 20px; padding: 10px; border: 1px solid #eee;">
        <div class="form-group">
            <asp:Label runat="server" AssociatedControlID="txtFiltroClienteTermino" Text="Buscar Cliente:" CssClass="control-label" />
            <asp:TextBox ID="txtFiltroClienteTermino" runat="server" CssClass="form-control" placeholder="Nombre, Apellido o Documento" Width="200px" />
            <asp:Label ID="lblClienteFiltradoInfo" runat="server" CssClass="text-info small" style="margin-left: 5px;" EnableViewState="false" />
        </div>
        <div class="form-group" style="margin-left:10px;">
            <asp:Label runat="server" AssociatedControlID="ddlFiltroRestaurante" Text="Restaurante:" CssClass="control-label" />
            <asp:DropDownList ID="ddlFiltroRestaurante" runat="server" CssClass="form-control" DataTextField="Nombre" DataValueField="RestauranteID" AppendDataBoundItems="true">
                <asp:ListItem Text="-- Todos --" Value="0"></asp:ListItem>
            </asp:DropDownList>
        </div>
        <div class="form-group" style="margin-left:10px;">
            <asp:Label runat="server" AssociatedControlID="txtFiltroFechaRest" Text="Fecha Reserva:" CssClass="control-label" />
            <asp:TextBox ID="txtFiltroFechaRest" runat="server" CssClass="form-control" TextMode="Date" />
        </div>
        <asp:Button ID="btnFiltrarReservasRest" runat="server" Text="Filtrar" OnClick="btnFiltrarReservasRest_Click" CssClass="btn btn-primary" style="margin-left:10px;" />
        <asp:Button ID="btnLimpiarFiltrosReservasRest" runat="server" Text="Limpiar Filtros" OnClick="btnLimpiarFiltrosReservasRest_Click" CssClass="btn btn-default" style="margin-left:5px;" CausesValidation="false" />
    </asp:Panel>

    <asp:GridView ID="gvReservasRestaurante" runat="server" AutoGenerateColumns="False" CssClass="table table-striped table-bordered table-hover"
        DataKeyNames="ReservaRestauranteID" OnRowCommand="gvReservasRestaurante_RowCommand" AllowPaging="True" PageSize="10" OnPageIndexChanging="gvReservasRestaurante_PageIndexChanging" >
        <Columns>
            <asp:BoundField DataField="ReservaRestauranteID" HeaderText="ID Reserva" ReadOnly="True" />
            <asp:BoundField DataField="NombreRestaurante" HeaderText="Restaurante" />
            <asp:BoundField DataField="NombreCliente" HeaderText="Cliente" />
            <asp:BoundField DataField="FechaReserva" HeaderText="Fecha" DataFormatString="{0:yyyy-MM-dd}" />
            <asp:BoundField DataField="HoraReserva" HeaderText="Hora" DataFormatString="{0:hh\\:mm}" />
            <asp:BoundField DataField="NumeroComensales" HeaderText="Comensales" />
            <asp:BoundField DataField="Estado" HeaderText="Estado" />
            <asp:TemplateField HeaderText="Acciones">
                <ItemTemplate>
                    <asp:LinkButton ID="lnkEditarReservaRest" runat="server" CommandName="EditarReservaRest" CommandArgument='<%# Eval("ReservaRestauranteID") %>' Text="Editar" CssClass="btn btn-xs btn-info"></asp:LinkButton>
                    <asp:LinkButton ID="lnkCancelarReservaRest" runat="server" CommandName="CancelarReservaRest" CommandArgument='<%# Eval("ReservaRestauranteID") %>' Text="Cancelar" CssClass="btn btn-xs btn-warning" OnClientClick="return confirm('¿Está seguro de que desea cancelar esta reserva de restaurante?');"></asp:LinkButton>
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
        <EmptyDataTemplate>
            No se encontraron reservas de restaurante con los criterios seleccionados.
        </EmptyDataTemplate>
    </asp:GridView>
    
    <asp:Panel ID="pnlEditarReservaRestaurante" runat="server" Visible="false" CssClass="form-horizontal" style="margin-top: 20px; padding:15px; border: 1px solid #f0ad4e; border-radius:5px;">
        <h3>Editar Reserva de Restaurante #<asp:Literal ID="litReservaRestIDEditar" runat="server"></asp:Literal></h3>
        <asp:HiddenField ID="hfReservaRestIDEditar" runat="server" />
        
        <div class="form-group">
            <asp:Label runat="server" CssClass="col-md-3 control-label">Cliente:</asp:Label>
            <div class="col-md-9"><asp:Literal ID="litClienteReservaRestEditar" runat="server" /></div>
        </div>
        <div class="form-group">
            <asp:Label runat="server" CssClass="col-md-3 control-label">Restaurante:</asp:Label>
            <div class="col-md-9"><asp:Literal ID="litRestauranteReservaRestEditar" runat="server" /></div>
        </div>
        <div class="form-group">
            <asp:Label runat="server" CssClass="col-md-3 control-label">Fecha y Hora:</asp:Label>
            <div class="col-md-9"><asp:Literal ID="litFechaHoraReservaRestEditar" runat="server" /></div>
        </div>

        <div class="form-group">
            <asp:Label runat="server" AssociatedControlID="txtEditNumComensalesRest" CssClass="col-md-3 control-label">N° Comensales:</asp:Label>
            <div class="col-md-9">
                <asp:TextBox ID="txtEditNumComensalesRest" runat="server" CssClass="form-control" TextMode="Number" />
                <asp:RequiredFieldValidator runat="server" ControlToValidate="txtEditNumComensalesRest" ErrorMessage="N° Comensales es requerido." CssClass="text-danger" ValidationGroup="EditReservaRestVal" />
            </div>
        </div>
        <div class="form-group">
            <asp:Label runat="server" AssociatedControlID="txtEditNotasRest" CssClass="col-md-3 control-label">Notas:</asp:Label>
            <div class="col-md-9">
                <asp:TextBox ID="txtEditNotasRest" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3"/>
            </div>
        </div>
        <div class="form-group">
            <asp:Label runat="server" AssociatedControlID="ddlEditEstadoRest" CssClass="col-md-3 control-label">Estado:</asp:Label>
            <div class="col-md-9">
                 <asp:DropDownList ID="ddlEditEstadoRest" runat="server" CssClass="form-control">
                    <asp:ListItem Text="Confirmada" Value="Confirmada"></asp:ListItem>
                    <asp:ListItem Text="Atendida" Value="Atendida"></asp:ListItem>
                    <asp:ListItem Text="No Show" Value="No_Show"></asp:ListItem>
                    <asp:ListItem Text="Cancelada" Value="Cancelada"></asp:ListItem>
                </asp:DropDownList>
            </div>
        </div>
        <div class="form-group">
            <div class="col-md-offset-3 col-md-9">
                <asp:Button ID="btnGuardarEdicionReservaRest" runat="server" Text="Guardar Cambios" OnClick="btnGuardarEdicionReservaRest_Click" CssClass="btn btn-primary" ValidationGroup="EditReservaRestVal" />
                <asp:Button ID="btnCancelarEdicionReservaRest" runat="server" Text="Cancelar Edición" OnClick="btnCancelarEdicionReservaRest_Click" CssClass="btn btn-default" CausesValidation="false" />
            </div>
        </div>
    </asp:Panel>
</asp:Content>