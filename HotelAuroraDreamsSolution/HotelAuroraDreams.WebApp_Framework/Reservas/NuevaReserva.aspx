<%@ Page Title="Nueva Reserva de Habitación" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="NuevaReserva.aspx.cs" Inherits="HotelAuroraDreams.WebApp_Framework.Reservas.NuevaReserva" Async="true" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h2><%: Title %></h2>
    <asp:Label ID="lblMessage" runat="server" CssClass="text-danger" EnableViewState="false"></asp:Label>
    <asp:Label ID="lblSuccessMessage" runat="server" CssClass="text-success" EnableViewState="false"></asp:Label>

    <div class="panel panel-default">
        <div class="panel-heading">Buscar Disponibilidad</div>
        <div class="panel-body form-horizontal">
            <div class="form-group">
                <asp:Label runat="server" AssociatedControlID="ddlHotelBusqueda" CssClass="col-md-2 control-label">Hotel:</asp:Label>
                <div class="col-md-4">
                    <asp:DropDownList ID="ddlHotelBusqueda" runat="server" CssClass="form-control" DataTextField="Nombre" DataValueField="HotelID" />
                </div>
                <asp:Label runat="server" AssociatedControlID="ddlTipoHabitacionBusqueda" CssClass="col-md-2 control-label">Tipo Habitación (Opc):</asp:Label>
                <div class="col-md-4">
                    <asp:DropDownList ID="ddlTipoHabitacionBusqueda" runat="server" CssClass="form-control" DataTextField="nombre" DataValueField="tipo_habitacion_id" AppendDataBoundItems="true">
                        <asp:ListItem Text="-- Todos --" Value="0"></asp:ListItem>
                    </asp:DropDownList>
                </div>
            </div>
            <div class="form-group">
                <asp:Label runat="server" AssociatedControlID="txtFechaEntrada" CssClass="col-md-2 control-label">Fecha Entrada:</asp:Label>
                <div class="col-md-4">
                    <asp:TextBox ID="txtFechaEntrada" runat="server" CssClass="form-control" TextMode="Date" />
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="txtFechaEntrada" ErrorMessage="Fecha de entrada requerida." CssClass="text-danger" Display="Dynamic" ValidationGroup="BusquedaVal"/>
                </div>
                <asp:Label runat="server" AssociatedControlID="txtFechaSalida" CssClass="col-md-2 control-label">Fecha Salida:</asp:Label>
                <div class="col-md-4">
                    <asp:TextBox ID="txtFechaSalida" runat="server" CssClass="form-control" TextMode="Date" />
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="txtFechaSalida" ErrorMessage="Fecha de salida requerida." CssClass="text-danger" Display="Dynamic" ValidationGroup="BusquedaVal"/>
                </div>
            </div>
            <div class="form-group">
                <asp:Label runat="server" AssociatedControlID="txtNumeroHuespedesBusqueda" CssClass="col-md-2 control-label">N° Huéspedes:</asp:Label>
                <div class="col-md-4">
                    <asp:TextBox ID="txtNumeroHuespedesBusqueda" runat="server" CssClass="form-control" TextMode="Number" Text="1" />
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="txtNumeroHuespedesBusqueda" ErrorMessage="N° huéspedes requerido." CssClass="text-danger" Display="Dynamic" ValidationGroup="BusquedaVal"/>
                    <asp:RangeValidator runat="server" ControlToValidate="txtNumeroHuespedesBusqueda" MinimumValue="1" MaximumValue="10" Type="Integer" ErrorMessage="N° huéspedes entre 1 y 10." CssClass="text-danger" Display="Dynamic" ValidationGroup="BusquedaVal" />
                </div>
                <div class="col-md-offset-2 col-md-4">
                    <asp:Button ID="btnVerificarDisponibilidad" runat="server" Text="Verificar Disponibilidad" OnClick="btnVerificarDisponibilidad_Click" CssClass="btn btn-info" ValidationGroup="BusquedaVal" />
                </div>
            </div>
        </div>
    </div>

    <asp:Panel ID="pnlResultadosDisponibilidad" runat="server" Visible="false" style="margin-top:20px;">
        <h4>Habitaciones Disponibles</h4>
        <asp:Label ID="lblNoDisponibilidad" runat="server" CssClass="text-info" Visible="false" Text="No se encontraron habitaciones disponibles para los criterios seleccionados."></asp:Label>
        <asp:CheckBoxList ID="cblHabitacionesDisponibles" runat="server" CssClass="checkboxlist-columns" RepeatColumns="2" DataTextField="DisplayText" DataValueField="HabitacionID">
        </asp:CheckBoxList>
    </asp:Panel>

    <asp:Panel ID="pnlDetallesReserva" runat="server" Visible="false" CssClass="form-horizontal" style="margin-top: 20px; padding:15px; border: 1px solid #007bff; border-radius:5px;">
        <h4>Confirmar Detalles de la Reserva</h4>
        <div class="form-group">
            <asp:Label runat="server" AssociatedControlID="txtClienteID" CssClass="col-md-2 control-label">ID Cliente:</asp:Label>
            <div class="col-md-10">
                <asp:TextBox ID="txtClienteID" runat="server" CssClass="form-control" TextMode="Number" />
                <asp:RequiredFieldValidator runat="server" ControlToValidate="txtClienteID" ErrorMessage="ID de Cliente requerido." CssClass="text-danger" Display="Dynamic" ValidationGroup="ReservaVal"/>
            </div>
        </div>
         <div class="form-group">
            <asp:Label runat="server" AssociatedControlID="txtNotasReserva" CssClass="col-md-2 control-label">Notas Adicionales:</asp:Label>
            <div class="col-md-10">
                <asp:TextBox ID="txtNotasReserva" runat="server" TextMode="MultiLine" Rows="3" CssClass="form-control" />
            </div>
        </div>
        <div class="form-group">
            <div class="col-md-offset-2 col-md-10">
                <asp:Button ID="btnConfirmarReserva" runat="server" Text="Confirmar Reserva" OnClick="btnConfirmarReserva_Click" CssClass="btn btn-primary" ValidationGroup="ReservaVal" />
                <asp:Button ID="btnCancelarProceso" runat="server" Text="Cancelar Proceso" OnClick="btnCancelarProceso_Click" CssClass="btn btn-default" CausesValidation="false" />
            </div>
        </div>
    </asp:Panel>
</asp:Content>