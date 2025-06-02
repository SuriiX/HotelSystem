<%@ Page Title="Gestión de Salones de Evento" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="GestionSalonesEvento.aspx.cs" Inherits="HotelAuroraDreams.WebApp_Framework.Admin.GestionSalonesEvento" Async="true" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h2><%: Title %></h2>
    <asp:Label ID="lblMessage" runat="server" CssClass="text-danger" EnableViewState="false"></asp:Label>
    <asp:Label ID="lblSuccessMessage" runat="server" CssClass="text-success" EnableViewState="false"></asp:Label>

    <asp:Panel ID="pnlFormSalon" runat="server" Visible="false" CssClass="form-horizontal" style="margin-bottom: 20px; padding:15px; border: 1px solid #ccc; border-radius:5px;">
        <h3><asp:Literal ID="litFormTitle" runat="server">Añadir Nuevo Salón</asp:Literal></h3>
        <asp:HiddenField ID="hfSalonEventoID" runat="server" Value="0" />

        <div class="form-group">
            <asp:Label runat="server" AssociatedControlID="ddlHotelSalon" CssClass="col-md-2 control-label">Hotel:</asp:Label>
            <div class="col-md-10">
                <asp:DropDownList ID="ddlHotelSalon" runat="server" CssClass="form-control" DataTextField="Nombre" DataValueField="HotelID" />
                <asp:RequiredFieldValidator runat="server" ControlToValidate="ddlHotelSalon" ErrorMessage="El hotel es requerido." CssClass="text-danger" Display="Dynamic" InitialValue="0" ValidationGroup="SalonValidation" />
            </div>
        </div>
        <div class="form-group">
            <asp:Label runat="server" AssociatedControlID="txtNombreSalon" CssClass="col-md-2 control-label">Nombre Salón:</asp:Label>
            <div class="col-md-10">
                <asp:TextBox runat="server" ID="txtNombreSalon" CssClass="form-control" />
                <asp:RequiredFieldValidator runat="server" ControlToValidate="txtNombreSalon" ErrorMessage="El nombre del salón es requerido." CssClass="text-danger" Display="Dynamic" ValidationGroup="SalonValidation" />
            </div>
        </div>
        <div class="form-group">
            <asp:Label runat="server" AssociatedControlID="txtDescripcionSalon" CssClass="col-md-2 control-label">Descripción:</asp:Label>
            <div class="col-md-10">
                <asp:TextBox runat="server" ID="txtDescripcionSalon" TextMode="MultiLine" Rows="3" CssClass="form-control" />
            </div>
        </div>
        <div class="form-group">
            <asp:Label runat="server" AssociatedControlID="txtCapacidadSalon" CssClass="col-md-2 control-label">Capacidad Máxima:</asp:Label>
            <div class="col-md-10">
                <asp:TextBox runat="server" ID="txtCapacidadSalon" CssClass="form-control" TextMode="Number" />
                <asp:RequiredFieldValidator runat="server" ControlToValidate="txtCapacidadSalon" ErrorMessage="La capacidad es requerida." CssClass="text-danger" Display="Dynamic" ValidationGroup="SalonValidation" />
                <asp:CompareValidator runat="server" ControlToValidate="txtCapacidadSalon" Operator="DataTypeCheck" Type="Integer" ErrorMessage="Capacidad inválida." CssClass="text-danger" Display="Dynamic" ValidationGroup="SalonValidation" />
            </div>
        </div>
        <div class="form-group">
            <asp:Label runat="server" AssociatedControlID="txtUbicacionSalon" CssClass="col-md-2 control-label">Ubicación:</asp:Label>
            <div class="col-md-10">
                <asp:TextBox runat="server" ID="txtUbicacionSalon" CssClass="form-control" />
            </div>
        </div>
         <div class="form-group">
            <asp:Label runat="server" AssociatedControlID="txtPrecioHoraSalon" CssClass="col-md-2 control-label">Precio por Hora:</asp:Label>
            <div class="col-md-10">
                <asp:TextBox runat="server" ID="txtPrecioHoraSalon" CssClass="form-control" />
                 <asp:CompareValidator runat="server" ControlToValidate="txtPrecioHoraSalon" Operator="DataTypeCheck" Type="Currency" ErrorMessage="Precio inválido." CssClass="text-danger" Display="Dynamic" ValidationGroup="SalonValidation" />
            </div>
        </div>
         <div class="form-group">
            <asp:Label runat="server" AssociatedControlID="chkEstaActivoSalon" CssClass="col-md-2 control-label">Activo:</asp:Label>
            <div class="col-md-10">
                <asp:CheckBox ID="chkEstaActivoSalon" runat="server" Checked="true" />
            </div>
        </div>
        <div class="form-group">
            <div class="col-md-offset-2 col-md-10">
                <asp:Button runat="server" ID="btnSaveSalon" Text="Guardar Salón" OnClick="btnSaveSalon_Click" CssClass="btn btn-primary" ValidationGroup="SalonValidation" />
                <asp:Button runat="server" ID="btnCancelSalon" Text="Cancelar" OnClick="btnCancelSalon_Click" CssClass="btn btn-default" CausesValidation="false" />
            </div>
        </div>
    </asp:Panel>

    <asp:Button ID="btnShowAddSalonForm" runat="server" Text="Añadir Nuevo Salón" OnClick="btnShowAddSalonForm_Click" CssClass="btn btn-success" style="margin-bottom: 15px;" CausesValidation="false" />

    <asp:GridView ID="gvSalonesEvento" runat="server" AutoGenerateColumns="False" CssClass="table table-striped table-bordered table-hover"
        DataKeyNames="SalonEventoID" OnRowCommand="gvSalonesEvento_RowCommand" AllowPaging="True" PageSize="10" OnPageIndexChanging="gvSalonesEvento_PageIndexChanging" >
        <Columns>
            <asp:BoundField DataField="SalonEventoID" HeaderText="ID" ReadOnly="True" />
            <asp:BoundField DataField="NombreHotel" HeaderText="Hotel" />
            <asp:BoundField DataField="Nombre" HeaderText="Nombre Salón" />
            <asp:BoundField DataField="CapacidadMaxima" HeaderText="Capacidad" />
            <asp:BoundField DataField="PrecioPorHora" HeaderText="Precio/Hora" DataFormatString="{0:C}" />
            <asp:CheckBoxField DataField="EstaActivo" HeaderText="Activo" ReadOnly="true" />
            <asp:TemplateField HeaderText="Acciones">
                <ItemTemplate>
                    <asp:LinkButton ID="lnkEditSalon" runat="server" CommandName="EditSalon" CommandArgument='<%# Eval("SalonEventoID") %>' Text="Editar" CssClass="btn btn-xs btn-info"></asp:LinkButton>
                    <asp:LinkButton ID="lnkDeleteSalon" runat="server" CommandName="DeleteSalon" CommandArgument='<%# Eval("SalonEventoID") %>' Text="Eliminar" CssClass="btn btn-xs btn-danger" OnClientClick="return confirm('¿Está seguro de que desea eliminar este salón de evento?');"></asp:LinkButton>
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
        <EmptyDataTemplate>
            No se encontraron salones de evento.
        </EmptyDataTemplate>
    </asp:GridView>
</asp:Content>