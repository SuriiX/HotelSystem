<%@ Page Title="Gestión de Habitaciones" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="GestionHabitaciones.aspx.cs" Inherits="HotelAuroraDreams.WebApp_Framework.Admin.GestionHabitaciones" Async="true" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h2><%: Title %></h2>
    <asp:Label ID="lblMessage" runat="server" CssClass="text-danger" EnableViewState="false"></asp:Label>
    <asp:Label ID="lblSuccessMessage" runat="server" CssClass="text-success" EnableViewState="false"></asp:Label>

    <asp:Panel ID="pnlFormHabitacion" runat="server" Visible="false" CssClass="form-horizontal" style="margin-bottom: 20px; padding:15px; border: 1px solid #ccc; border-radius:5px;">
        <h3><asp:Literal ID="litFormTitle" runat="server">Añadir Nueva Habitación</asp:Literal></h3>
        <asp:HiddenField ID="hfHabitacionID" runat="server" Value="0" />

        <div class="form-group">
            <asp:Label runat="server" AssociatedControlID="ddlHotel" CssClass="col-md-2 control-label">Hotel:</asp:Label>
            <div class="col-md-10">
                <asp:DropDownList ID="ddlHotel" runat="server" CssClass="form-control" DataTextField="Nombre" DataValueField="HotelID" />
                <asp:RequiredFieldValidator runat="server" ControlToValidate="ddlHotel" ErrorMessage="El hotel es requerido." CssClass="text-danger" Display="Dynamic" InitialValue="0" ValidationGroup="HabitacionValidation" />
            </div>
        </div>
        <div class="form-group">
            <asp:Label runat="server" AssociatedControlID="ddlTipoHabitacion" CssClass="col-md-2 control-label">Tipo Habitación:</asp:Label>
            <div class="col-md-10">
                <asp:DropDownList ID="ddlTipoHabitacion" runat="server" CssClass="form-control" DataTextField="nombre" DataValueField="tipo_habitacion_id" />
                <asp:RequiredFieldValidator runat="server" ControlToValidate="ddlTipoHabitacion" ErrorMessage="El tipo de habitación es requerido." CssClass="text-danger" Display="Dynamic" InitialValue="0" ValidationGroup="HabitacionValidation" />
            </div>
        </div>
        <div class="form-group">
            <asp:Label runat="server" AssociatedControlID="txtNumeroHabitacion" CssClass="col-md-2 control-label">Número:</asp:Label>
            <div class="col-md-10">
                <asp:TextBox runat="server" ID="txtNumeroHabitacion" CssClass="form-control" />
                <asp:RequiredFieldValidator runat="server" ControlToValidate="txtNumeroHabitacion" ErrorMessage="El número es requerido." CssClass="text-danger" Display="Dynamic" ValidationGroup="HabitacionValidation" />
            </div>
        </div>
        <div class="form-group">
            <asp:Label runat="server" AssociatedControlID="txtPisoHabitacion" CssClass="col-md-2 control-label">Piso:</asp:Label>
            <div class="col-md-10">
                <asp:TextBox runat="server" ID="txtPisoHabitacion" CssClass="form-control" TextMode="Number" />
                <asp:RequiredFieldValidator runat="server" ControlToValidate="txtPisoHabitacion" ErrorMessage="El piso es requerido." CssClass="text-danger" Display="Dynamic" ValidationGroup="HabitacionValidation" />
                <asp:CompareValidator runat="server" ControlToValidate="txtPisoHabitacion" Operator="DataTypeCheck" Type="Integer" ErrorMessage="Ingrese un número entero válido para el piso." CssClass="text-danger" Display="Dynamic" ValidationGroup="HabitacionValidation" />
            </div>
        </div>
        <div class="form-group">
            <asp:Label runat="server" AssociatedControlID="ddlEstadoHabitacion" CssClass="col-md-2 control-label">Estado:</asp:Label>
            <div class="col-md-10">
                <asp:DropDownList ID="ddlEstadoHabitacion" runat="server" CssClass="form-control">
                    <asp:ListItem Text="Disponible" Value="disponible"></asp:ListItem>
                    <asp:ListItem Text="Ocupada" Value="ocupada"></asp:ListItem>
                    <asp:ListItem Text="Mantenimiento" Value="mantenimiento"></asp:ListItem>
                    <asp:ListItem Text="Limpieza Pendiente" Value="limpieza_pendiente"></asp:ListItem>
                </asp:DropDownList>
            </div>
        </div>
         <div class="form-group">
            <asp:Label runat="server" AssociatedControlID="txtVistaHabitacion" CssClass="col-md-2 control-label">Vista:</asp:Label>
            <div class="col-md-10">
                <asp:TextBox runat="server" ID="txtVistaHabitacion" CssClass="form-control" />
            </div>
        </div>
        <div class="form-group">
            <asp:Label runat="server" AssociatedControlID="txtDescripcionHabitacion" CssClass="col-md-2 control-label">Descripción:</asp:Label>
            <div class="col-md-10">
                <asp:TextBox runat="server" ID="txtDescripcionHabitacion" TextMode="MultiLine" Rows="3" CssClass="form-control" />
            </div>
        </div>
        <div class="form-group">
            <div class="col-md-offset-2 col-md-10">
                <asp:Button runat="server" ID="btnSaveHabitacion" Text="Guardar Habitación" OnClick="btnSaveHabitacion_Click" CssClass="btn btn-primary" ValidationGroup="HabitacionValidation" />
                <asp:Button runat="server" ID="btnCancelHabitacion" Text="Cancelar" OnClick="btnCancelHabitacion_Click" CssClass="btn btn-default" CausesValidation="false" />
            </div>
        </div>
    </asp:Panel>

    <asp:Button ID="btnShowAddHabitacionForm" runat="server" Text="Añadir Nueva Habitación" OnClick="btnShowAddHabitacionForm_Click" CssClass="btn btn-success" style="margin-bottom: 15px;" CausesValidation="false" />

    <asp:GridView ID="gvHabitaciones" runat="server" AutoGenerateColumns="False" CssClass="table table-striped table-bordered table-hover"
        DataKeyNames="HabitacionID" OnRowCommand="gvHabitaciones_RowCommand" AllowPaging="True" PageSize="10" OnPageIndexChanging="gvHabitaciones_PageIndexChanging" >
        <Columns>
            <asp:BoundField DataField="HabitacionID" HeaderText="ID" ReadOnly="True" />
            <asp:BoundField DataField="NombreHotel" HeaderText="Hotel" />
            <asp:BoundField DataField="NombreTipoHabitacion" HeaderText="Tipo" />
            <asp:BoundField DataField="Numero" HeaderText="Número" />
            <asp:BoundField DataField="Piso" HeaderText="Piso" />
            <asp:BoundField DataField="Estado" HeaderText="Estado" />
            <asp:BoundField DataField="Vista" HeaderText="Vista" />
            <asp:TemplateField HeaderText="Acciones">
                <ItemTemplate>
                    <asp:LinkButton ID="lnkEditHabitacion" runat="server" CommandName="EditHabitacion" CommandArgument='<%# Eval("HabitacionID") %>' Text="Editar" CssClass="btn btn-xs btn-info"></asp:LinkButton>
                    <asp:LinkButton ID="lnkDeleteHabitacion" runat="server" CommandName="DeleteHabitacion" CommandArgument='<%# Eval("HabitacionID") %>' Text="Eliminar" CssClass="btn btn-xs btn-danger" OnClientClick="return confirm('¿Está seguro de que desea eliminar esta habitación?');"></asp:LinkButton>
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
        <EmptyDataTemplate>
            No se encontraron habitaciones.
        </EmptyDataTemplate>
    </asp:GridView>
</asp:Content>