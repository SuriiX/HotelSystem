<%@ Page Title="Nueva Reserva de Evento" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="NuevaReservaEvento.aspx.cs" Inherits="HotelAuroraDreams.WebApp_Framework.Eventos.NuevaReservaEvento" Async="true" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h2><%: Title %></h2>
    <asp:Label ID="lblMessage" runat="server" CssClass="text-danger" EnableViewState="false"></asp:Label>
    <asp:Label ID="lblSuccessMessage" runat="server" CssClass="text-success" EnableViewState="false"></asp:Label>

    <div class="panel panel-default">
        <div class="panel-heading">Información del Evento</div>
        <div class="panel-body form-horizontal">
            <div class="form-group">
                <asp:Label runat="server" AssociatedControlID="ddlClienteEvento" CssClass="col-md-2 control-label">Cliente:</asp:Label>
                <div class="col-md-10">
                    <asp:DropDownList ID="ddlClienteEvento" runat="server" CssClass="form-control" DataTextField="NombreCompleto" DataValueField="ClienteID" AppendDataBoundItems="true">
                         <asp:ListItem Text="-- Seleccione Cliente --" Value="0"></asp:ListItem>
                    </asp:DropDownList>
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="ddlClienteEvento" ErrorMessage="Cliente es requerido." CssClass="text-danger" Display="Dynamic" InitialValue="0" ValidationGroup="EventoVal"/>
                </div>
            </div>
            <div class="form-group">
                <asp:Label runat="server" AssociatedControlID="txtNombreEvento" CssClass="col-md-2 control-label">Nombre del Evento:</asp:Label>
                <div class="col-md-10">
                    <asp:TextBox ID="txtNombreEvento" runat="server" CssClass="form-control" />
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="txtNombreEvento" ErrorMessage="Nombre del evento es requerido." CssClass="text-danger" Display="Dynamic" ValidationGroup="EventoVal"/>
                </div>
            </div>
             <div class="form-group">
                <asp:Label runat="server" AssociatedControlID="ddlSalonEvento" CssClass="col-md-2 control-label">Salón:</asp:Label>
                <div class="col-md-10">
                    <asp:DropDownList ID="ddlSalonEvento" runat="server" CssClass="form-control" DataTextField="Nombre" DataValueField="SalonEventoID" AutoPostBack="true" OnSelectedIndexChanged="ddlSalonEvento_SelectedIndexChanged" AppendDataBoundItems="true">
                         <asp:ListItem Text="-- Seleccione Salón --" Value="0"></asp:ListItem>
                    </asp:DropDownList>
                     <asp:RequiredFieldValidator runat="server" ControlToValidate="ddlSalonEvento" ErrorMessage="Salón es requerido." CssClass="text-danger" Display="Dynamic" InitialValue="0" ValidationGroup="EventoVal"/>
                </div>
            </div>
             <div class="form-group">
                <asp:Label runat="server" AssociatedControlID="ddlTipoEvento" CssClass="col-md-2 control-label">Tipo de Evento (Opc):</asp:Label>
                <div class="col-md-10">
                    <asp:DropDownList ID="ddlTipoEvento" runat="server" CssClass="form-control" DataTextField="NombreTipo" DataValueField="TipoEventoID" AppendDataBoundItems="true">
                        <asp:ListItem Text="-- Seleccione Tipo --" Value="0"></asp:ListItem>
                    </asp:DropDownList>
                </div>
            </div>
            <div class="form-group">
                <asp:Label runat="server" AssociatedControlID="txtFechaEvento" CssClass="col-md-2 control-label">Fecha Evento:</asp:Label>
                <div class="col-md-4">
                    <asp:TextBox ID="txtFechaEvento" runat="server" CssClass="form-control" TextMode="Date" AutoPostBack="true" OnTextChanged="txtFechaHora_TextChanged" />
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="txtFechaEvento" ErrorMessage="Fecha de evento requerida." CssClass="text-danger" Display="Dynamic" ValidationGroup="EventoVal"/>
                </div>
            </div>
            <div class="form-group">
                 <asp:Label runat="server" AssociatedControlID="txtHoraInicioEvento" CssClass="col-md-2 control-label">Hora Inicio:</asp:Label>
                <div class="col-md-4">
                    <asp:TextBox ID="txtHoraInicioEvento" runat="server" CssClass="form-control" TextMode="Time" AutoPostBack="true" OnTextChanged="txtFechaHora_TextChanged" />
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="txtHoraInicioEvento" ErrorMessage="Hora de inicio requerida." CssClass="text-danger" Display="Dynamic" ValidationGroup="EventoVal"/>
                </div>
                <asp:Label runat="server" AssociatedControlID="txtHoraFinEvento" CssClass="col-md-2 control-label">Hora Fin:</asp:Label>
                <div class="col-md-4">
                    <asp:TextBox ID="txtHoraFinEvento" runat="server" CssClass="form-control" TextMode="Time" AutoPostBack="true" OnTextChanged="txtFechaHora_TextChanged" />
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="txtHoraFinEvento" ErrorMessage="Hora de fin requerida." CssClass="text-danger" Display="Dynamic" ValidationGroup="EventoVal"/>
                </div>
            </div>
            <div class="form-group">
                 <div class="col-md-offset-2 col-md-10">
                    <asp:Button ID="btnVerificarDisponibilidadSalon" runat="server" Text="Verificar Disponibilidad Salón" OnClick="btnVerificarDisponibilidadSalon_Click" CssClass="btn btn-info" ValidationGroup="EventoValFechasSalon" /> <%-- Nuevo grupo de validación --%>
                    <asp:Label ID="lblDisponibilidadSalon" runat="server" EnableViewState="false" style="margin-left:10px; font-weight:bold;"></asp:Label>
                </div>
            </div>
             <div class="form-group">
                <asp:Label runat="server" AssociatedControlID="txtNumeroAsistentesEvento" CssClass="col-md-2 control-label">N° Asistentes (Est.):</asp:Label>
                <div class="col-md-4">
                    <asp:TextBox ID="txtNumeroAsistentesEvento" runat="server" CssClass="form-control" TextMode="Number" />
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="txtNumeroAsistentesEvento" ErrorMessage="N° asistentes requerido." CssClass="text-danger" Display="Dynamic" ValidationGroup="EventoVal"/>
                    <asp:RangeValidator runat="server" ControlToValidate="txtNumeroAsistentesEvento" MinimumValue="1" MaximumValue="10000" Type="Integer" ErrorMessage="N° asistentes inválido." CssClass="text-danger" Display="Dynamic" ValidationGroup="EventoVal"/>
                </div>
            </div>
        </div>
    </div>

    <asp:Panel ID="pnlServiciosAdicionales" runat="server" Visible="false" CssClass="panel panel-default" style="margin-top:20px;">
        <div class="panel-heading">Servicios Adicionales</div>
        <div class="panel-body">
             <asp:GridView ID="gvServiciosAdicionales" runat="server" AutoGenerateColumns="False" CssClass="table"
                DataKeyNames="ServicioAdicionalID">
                <Columns>
                    <asp:BoundField DataField="ServicioAdicionalID" HeaderText="ID" Visible="false" />
                    <asp:BoundField DataField="NombreServicio" HeaderText="Servicio" />
                    <asp:BoundField DataField="PrecioBase" HeaderText="Precio Base" DataFormatString="{0:C}" />
                    <asp:TemplateField HeaderText="Seleccionar">
                        <ItemTemplate>
                            <asp:CheckBox ID="chkSeleccionarServicio" runat="server" />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Cantidad">
                        <ItemTemplate>
                            <asp:TextBox ID="txtCantidadServicio" runat="server" TextMode="Number" Width="60px" Text="1" CssClass="form-control input-sm"></asp:TextBox>
                        </ItemTemplate>
                    </asp:TemplateField>
                     <asp:TemplateField HeaderText="Precio Acordado/Unidad">
                        <ItemTemplate>
                            <asp:TextBox ID="txtPrecioAcordadoServicio" runat="server" Width="100px" Text='<%# Eval("PrecioBase", "{0:N2}") %>' CssClass="form-control input-sm"></asp:TextBox>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Notas Servicio">
                        <ItemTemplate>
                            <asp:TextBox ID="txtNotasServicio" runat="server" Width="200px" CssClass="form-control input-sm"></asp:TextBox>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>
    </asp:Panel>

    <asp:Panel ID="pnlConfirmacionReservaEvento" runat="server" Visible="false" CssClass="form-horizontal" style="margin-top: 20px; padding:15px; border: 1px solid #007bff; border-radius:5px;">
        <h4>Confirmar Reserva de Evento</h4>
         <div class="form-group">
            <asp:Label runat="server" AssociatedControlID="txtNotasGeneralesEvento" CssClass="col-md-2 control-label">Notas Generales:</asp:Label>
            <div class="col-md-10">
                <asp:TextBox ID="txtNotasGeneralesEvento" runat="server" TextMode="MultiLine" Rows="3" CssClass="form-control" />
            </div>
        </div>
        <div class="form-group">
            <div class="col-md-offset-2 col-md-10">
                <asp:Button ID="btnConfirmarReservaEvento" runat="server" Text="Confirmar Reserva Evento" OnClick="btnConfirmarReservaEvento_Click" CssClass="btn btn-primary" ValidationGroup="EventoVal" />
                <asp:Button ID="btnCancelarProcesoEvento" runat="server" Text="Cancelar Proceso" OnClick="btnCancelarProcesoEvento_Click" CssClass="btn btn-default" CausesValidation="false" />
            </div>
        </div>
    </asp:Panel>
</asp:Content>