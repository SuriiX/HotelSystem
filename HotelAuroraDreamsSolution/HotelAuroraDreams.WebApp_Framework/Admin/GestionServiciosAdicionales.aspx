<%@ Page Title="Gestión de Servicios Adicionales para Eventos" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="GestionServiciosAdicionales.aspx.cs" Inherits="HotelAuroraDreams.WebApp_Framework.Admin.GestionServiciosAdicionales" Async="true" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h2><%: Title %></h2>
    <asp:Label ID="lblMessage" runat="server" CssClass="text-danger" EnableViewState="false"></asp:Label>
    <asp:Label ID="lblSuccessMessage" runat="server" CssClass="text-success" EnableViewState="false"></asp:Label>

    <asp:Panel ID="pnlFormServicio" runat="server" Visible="false" CssClass="form-horizontal" style="margin-bottom: 20px; padding:15px; border: 1px solid #ccc; border-radius:5px;">
        <h3><asp:Literal ID="litFormTitle" runat="server">Añadir Nuevo Servicio Adicional</asp:Literal></h3>
        <asp:HiddenField ID="hfServicioID" runat="server" Value="0" />

        <div class="form-group">
            <asp:Label runat="server" AssociatedControlID="txtNombreServicio" CssClass="col-md-3 control-label">Nombre del Servicio:</asp:Label>
            <div class="col-md-9">
                <asp:TextBox runat="server" ID="txtNombreServicio" CssClass="form-control" />
                <asp:RequiredFieldValidator runat="server" ControlToValidate="txtNombreServicio" ErrorMessage="El nombre es requerido." CssClass="text-danger" Display="Dynamic" ValidationGroup="ServicioValidation" />
            </div>
        </div>
        <div class="form-group">
            <asp:Label runat="server" AssociatedControlID="txtDescripcionServicio" CssClass="col-md-3 control-label">Descripción:</asp:Label>
            <div class="col-md-9">
                <asp:TextBox runat="server" ID="txtDescripcionServicio" TextMode="MultiLine" Rows="3" CssClass="form-control" />
            </div>
        </div>
        <div class="form-group">
            <asp:Label runat="server" AssociatedControlID="txtPrecioBaseServicio" CssClass="col-md-3 control-label">Precio Base:</asp:Label>
            <div class="col-md-9">
                <asp:TextBox runat="server" ID="txtPrecioBaseServicio" CssClass="form-control" />
                <asp:RequiredFieldValidator runat="server" ControlToValidate="txtPrecioBaseServicio" ErrorMessage="El precio base es requerido." CssClass="text-danger" Display="Dynamic" ValidationGroup="ServicioValidation" />
                <asp:CompareValidator runat="server" ControlToValidate="txtPrecioBaseServicio" Operator="DataTypeCheck" Type="Currency" ErrorMessage="Precio inválido." CssClass="text-danger" Display="Dynamic" ValidationGroup="ServicioValidation" />
            </div>
        </div>
         <div class="form-group">
            <asp:Label runat="server" AssociatedControlID="chkRequierePersonal" CssClass="col-md-3 control-label">Requiere Personal de Pago:</asp:Label>
            <div class="col-md-9">
                <asp:CheckBox ID="chkRequierePersonal" runat="server" />
            </div>
        </div>
        <div class="form-group">
            <div class="col-md-offset-3 col-md-9">
                <asp:Button runat="server" ID="btnSaveServicio" Text="Guardar Servicio" OnClick="btnSaveServicio_Click" CssClass="btn btn-primary" ValidationGroup="ServicioValidation" />
                <asp:Button runat="server" ID="btnCancelServicio" Text="Cancelar" OnClick="btnCancelServicio_Click" CssClass="btn btn-default" CausesValidation="false" />
            </div>
        </div>
    </asp:Panel>

    <asp:Button ID="btnShowAddServicioForm" runat="server" Text="Añadir Nuevo Servicio Adicional" OnClick="btnShowAddServicioForm_Click" CssClass="btn btn-success" style="margin-bottom: 15px;" CausesValidation="false" />

    <asp:GridView ID="gvServiciosAdicionales" runat="server" AutoGenerateColumns="False" CssClass="table table-striped table-bordered table-hover"
        DataKeyNames="ServicioAdicionalID" OnRowCommand="gvServiciosAdicionales_RowCommand" AllowPaging="True" PageSize="10" OnPageIndexChanging="gvServiciosAdicionales_PageIndexChanging">
        <Columns>
            <asp:BoundField DataField="ServicioAdicionalID" HeaderText="ID" ReadOnly="True" />
            <asp:BoundField DataField="NombreServicio" HeaderText="Nombre del Servicio" />
            <asp:BoundField DataField="PrecioBase" HeaderText="Precio Base" DataFormatString="{0:C}" />
            <asp:CheckBoxField DataField="RequierePersonalPago" HeaderText="Req. Personal" />
            <asp:TemplateField HeaderText="Acciones">
                <ItemTemplate>
                    <asp:LinkButton ID="lnkEditServicio" runat="server" CommandName="EditServicio" CommandArgument='<%# Eval("ServicioAdicionalID") %>' Text="Editar" CssClass="btn btn-xs btn-info"></asp:LinkButton>
                    <asp:LinkButton ID="lnkDeleteServicio" runat="server" CommandName="DeleteServicio" CommandArgument='<%# Eval("ServicioAdicionalID") %>' Text="Eliminar" CssClass="btn btn-xs btn-danger" OnClientClick="return confirm('¿Está seguro de que desea eliminar este servicio adicional?');"></asp:LinkButton>
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
        <EmptyDataTemplate>
            No se encontraron servicios adicionales para eventos.
        </EmptyDataTemplate>
    </asp:GridView>
</asp:Content>