<%@ Page Title="Gestión de Clientes" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="GestionClientes.aspx.cs" Inherits="HotelAuroraDreams.WebApp_Framework.Admin.GestionClientes" Async="true" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h2><%: Title %></h2>
    <asp:Label ID="lblMessage" runat="server" CssClass="text-danger" EnableViewState="false"></asp:Label>
    <asp:Label ID="lblSuccessMessage" runat="server" CssClass="text-success" EnableViewState="false"></asp:Label>

    <asp:Panel ID="pnlFormCliente" runat="server" Visible="false" CssClass="form-horizontal" style="margin-bottom: 20px; padding:15px; border: 1px solid #ccc; border-radius:5px;">
        <h3><asp:Literal ID="litFormTitle" runat="server">Añadir Nuevo Cliente</asp:Literal></h3>
        <asp:HiddenField ID="hfClienteID" runat="server" Value="0" />

        <div class="form-group">
            <asp:Label runat="server" AssociatedControlID="txtNombreCliente" CssClass="col-md-2 control-label">Nombre:</asp:Label>
            <div class="col-md-10">
                <asp:TextBox runat="server" ID="txtNombreCliente" CssClass="form-control" />
                <asp:RequiredFieldValidator runat="server" ControlToValidate="txtNombreCliente" ErrorMessage="El nombre es requerido." CssClass="text-danger" Display="Dynamic" ValidationGroup="ClienteValidation" />
            </div>
        </div>
        <div class="form-group">
            <asp:Label runat="server" AssociatedControlID="txtApellidoCliente" CssClass="col-md-2 control-label">Apellido:</asp:Label>
            <div class="col-md-10">
                <asp:TextBox runat="server" ID="txtApellidoCliente" CssClass="form-control" />
                <asp:RequiredFieldValidator runat="server" ControlToValidate="txtApellidoCliente" ErrorMessage="El apellido es requerido." CssClass="text-danger" Display="Dynamic" ValidationGroup="ClienteValidation" />
            </div>
        </div>
         <div class="form-group">
            <asp:Label runat="server" AssociatedControlID="ddlTipoDocumento" CssClass="col-md-2 control-label">Tipo Documento:</asp:Label>
            <div class="col-md-10">
                <asp:DropDownList ID="ddlTipoDocumento" runat="server" CssClass="form-control">
                    <asp:ListItem Text="DNI" Value="DNI"></asp:ListItem>
                    <asp:ListItem Text="Pasaporte" Value="Pasaporte"></asp:ListItem>
                    <asp:ListItem Text="Cédula" Value="Cédula"></asp:ListItem>
                    <asp:ListItem Text="Otro" Value="Otro"></asp:ListItem>
                </asp:DropDownList>
                <asp:RequiredFieldValidator runat="server" ControlToValidate="ddlTipoDocumento" ErrorMessage="El tipo de documento es requerido." CssClass="text-danger" Display="Dynamic" ValidationGroup="ClienteValidation" InitialValue="" />
            </div>
        </div>
        <div class="form-group">
            <asp:Label runat="server" AssociatedControlID="txtNumeroDocumento" CssClass="col-md-2 control-label">Número Documento:</asp:Label>
            <div class="col-md-10">
                <asp:TextBox runat="server" ID="txtNumeroDocumento" CssClass="form-control" />
                <asp:RequiredFieldValidator runat="server" ControlToValidate="txtNumeroDocumento" ErrorMessage="El número de documento es requerido." CssClass="text-danger" Display="Dynamic" ValidationGroup="ClienteValidation" />
            </div>
        </div>
        <div class="form-group">
            <asp:Label runat="server" AssociatedControlID="txtEmailCliente" CssClass="col-md-2 control-label">Email:</asp:Label>
            <div class="col-md-10">
                <asp:TextBox runat="server" ID="txtEmailCliente" CssClass="form-control" TextMode="Email" />
                 <asp:RegularExpressionValidator runat="server" ControlToValidate="txtEmailCliente" ValidationExpression="^\S+@\S+\.\S+$" ErrorMessage="Email inválido." CssClass="text-danger" Display="Dynamic" ValidationGroup="ClienteValidation" />
            </div>
        </div>
        <div class="form-group">
            <asp:Label runat="server" AssociatedControlID="txtTelefonoCliente" CssClass="col-md-2 control-label">Teléfono:</asp:Label>
            <div class="col-md-10">
                <asp:TextBox runat="server" ID="txtTelefonoCliente" CssClass="form-control" />
            </div>
        </div>
        <div class="form-group">
            <asp:Label runat="server" AssociatedControlID="txtDireccionCliente" CssClass="col-md-2 control-label">Dirección:</asp:Label>
            <div class="col-md-10">
                <asp:TextBox runat="server" ID="txtDireccionCliente" CssClass="form-control" />
            </div>
        </div>
        <div class="form-group">
            <asp:Label runat="server" AssociatedControlID="txtCiudadResidenciaID" CssClass="col-md-2 control-label">ID Ciudad Residencia:</asp:Label>
            <div class="col-md-10">
                <asp:TextBox runat="server" ID="txtCiudadResidenciaID" CssClass="form-control" TextMode="Number" />
                <%-- Idealmente, esto sería un DropDownList cargado con ciudades desde la API --%>
            </div>
        </div>
        <div class="form-group">
            <asp:Label runat="server" AssociatedControlID="txtFechaNacimiento" CssClass="col-md-2 control-label">Fecha Nacimiento:</asp:Label>
            <div class="col-md-10">
                <asp:TextBox runat="server" ID="txtFechaNacimiento" CssClass="form-control" TextMode="Date" />
            </div>
        </div>
        <div class="form-group">
            <div class="col-md-offset-2 col-md-10">
                <asp:Button runat="server" ID="btnSaveCliente" Text="Guardar Cliente" OnClick="btnSaveCliente_Click" CssClass="btn btn-primary" ValidationGroup="ClienteValidation" />
                <asp:Button runat="server" ID="btnCancelCliente" Text="Cancelar" OnClick="btnCancelCliente_Click" CssClass="btn btn-default" CausesValidation="false" />
            </div>
        </div>
    </asp:Panel>

    <asp:Button ID="btnShowAddClienteForm" runat="server" Text="Añadir Nuevo Cliente" OnClick="btnShowAddClienteForm_Click" CssClass="btn btn-success" style="margin-bottom: 15px;" CausesValidation="false" />

    <asp:GridView ID="gvClientes" runat="server" AutoGenerateColumns="False" CssClass="table table-striped table-bordered table-hover"
        DataKeyNames="ClienteID" OnRowCommand="gvClientes_RowCommand" AllowPaging="True" PageSize="10" OnPageIndexChanging="gvClientes_PageIndexChanging" >
        <Columns>
            <asp:BoundField DataField="ClienteID" HeaderText="ID" ReadOnly="True" SortExpression="ClienteID" />
            <asp:BoundField DataField="Nombre" HeaderText="Nombre" SortExpression="Nombre" />
            <asp:BoundField DataField="Apellido" HeaderText="Apellido" SortExpression="Apellido" />
            <asp:BoundField DataField="TipoDocumento" HeaderText="Tipo Doc." SortExpression="TipoDocumento" />
            <asp:BoundField DataField="NumeroDocumento" HeaderText="N° Documento" SortExpression="NumeroDocumento" />
            <asp:BoundField DataField="Email" HeaderText="Email" SortExpression="Email" />
            <asp:BoundField DataField="Telefono" HeaderText="Teléfono" SortExpression="Telefono" />
            <asp:TemplateField HeaderText="Acciones">
                <ItemTemplate>
                    <asp:LinkButton ID="lnkEditCliente" runat="server" CommandName="EditCliente" CommandArgument='<%# Eval("ClienteID") %>' Text="Editar" CssClass="btn btn-xs btn-info"></asp:LinkButton>
                    <asp:LinkButton ID="lnkDeleteCliente" runat="server" CommandName="DeleteCliente" CommandArgument='<%# Eval("ClienteID") %>' Text="Eliminar" CssClass="btn btn-xs btn-danger" OnClientClick="return confirm('¿Está seguro de que desea eliminar este cliente?');"></asp:LinkButton>
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
        <EmptyDataTemplate>
            No se encontraron clientes.
        </EmptyDataTemplate>
    </asp:GridView>
</asp:Content>