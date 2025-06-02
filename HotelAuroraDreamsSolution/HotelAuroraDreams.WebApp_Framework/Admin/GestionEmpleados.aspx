<%@ Page Title="Gestión de Empleados" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="GestionEmpleados.aspx.cs" Inherits="HotelAuroraDreams.WebApp_Framework.Admin.GestionEmpleados" Async="true" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h2><%: Title %></h2>
    <asp:Label ID="lblMessage" runat="server" CssClass="text-danger" EnableViewState="false"></asp:Label>
    <asp:Label ID="lblSuccessMessage" runat="server" CssClass="text-success" EnableViewState="false"></asp:Label>


    <asp:Panel ID="pnlEditEmpleado" runat="server" Visible="false" CssClass="form-horizontal" style="margin-bottom: 20px; padding:15px; border: 1px solid #ccc; border-radius:5px;">
        <h3><asp:Literal ID="litFormTitle" runat="server">Editar Empleado</asp:Literal></h3>
        <asp:HiddenField ID="hfEmpleadoID" runat="server" />
        <asp:Label ID="lblEmailDisplay" runat="server" Font-Bold="true"></asp:Label>

        <div class="form-group">
            <asp:Label runat="server" AssociatedControlID="txtNombreEmpleado" CssClass="col-md-2 control-label">Nombre:</asp:Label>
            <div class="col-md-10">
                <asp:TextBox runat="server" ID="txtNombreEmpleado" CssClass="form-control" />
                <asp:RequiredFieldValidator runat="server" ControlToValidate="txtNombreEmpleado" ErrorMessage="El nombre es requerido." CssClass="text-danger" Display="Dynamic" ValidationGroup="EmpleadoValidation" />
            </div>
        </div>
        <div class="form-group">
            <asp:Label runat="server" AssociatedControlID="txtApellidoEmpleado" CssClass="col-md-2 control-label">Apellido:</asp:Label>
            <div class="col-md-10">
                <asp:TextBox runat="server" ID="txtApellidoEmpleado" CssClass="form-control" />
                <asp:RequiredFieldValidator runat="server" ControlToValidate="txtApellidoEmpleado" ErrorMessage="El apellido es requerido." CssClass="text-danger" Display="Dynamic" ValidationGroup="EmpleadoValidation" />
            </div>
        </div>
        <div class="form-group">
            <asp:Label runat="server" AssociatedControlID="txtTelefonoEmpleado" CssClass="col-md-2 control-label">Teléfono:</asp:Label>
            <div class="col-md-10">
                <asp:TextBox runat="server" ID="txtTelefonoEmpleado" CssClass="form-control" />
            </div>
        </div>
        <div class="form-group">
            <asp:Label runat="server" AssociatedControlID="ddlHotelEmpleado" CssClass="col-md-2 control-label">Hotel:</asp:Label>
            <div class="col-md-10">
                <asp:DropDownList ID="ddlHotelEmpleado" runat="server" CssClass="form-control" DataTextField="Nombre" DataValueField="HotelID" AppendDataBoundItems="true">
                    <asp:ListItem Text="-- Sin Asignar --" Value=""></asp:ListItem>
                </asp:DropDownList>
            </div>
        </div>
        <div class="form-group">
            <asp:Label runat="server" AssociatedControlID="ddlCargoEmpleado" CssClass="col-md-2 control-label">Cargo:</asp:Label>
            <div class="col-md-10">
                <asp:DropDownList ID="ddlCargoEmpleado" runat="server" CssClass="form-control" DataTextField="NombreCargo" DataValueField="CargoID" AppendDataBoundItems="true">
                     <asp:ListItem Text="-- Sin Asignar --" Value=""></asp:ListItem>
                </asp:DropDownList>
            </div>
        </div>
        <div class="form-group">
            <asp:Label runat="server" AssociatedControlID="txtSalarioEmpleado" CssClass="col-md-2 control-label">Salario:</asp:Label>
            <div class="col-md-10">
                <asp:TextBox runat="server" ID="txtSalarioEmpleado" CssClass="form-control" />
                 <asp:CompareValidator runat="server" ControlToValidate="txtSalarioEmpleado" Operator="DataTypeCheck" Type="Currency" ErrorMessage="Salario inválido." CssClass="text-danger" Display="Dynamic" ValidationGroup="EmpleadoValidation" />
            </div>
        </div>
         <div class="form-group">
            <asp:Label runat="server" AssociatedControlID="ddlEstadoEmpleado" CssClass="col-md-2 control-label">Estado:</asp:Label>
            <div class="col-md-10">
                <asp:DropDownList ID="ddlEstadoEmpleado" runat="server" CssClass="form-control">
                    <asp:ListItem Value="activo">Activo</asp:ListItem>
                    <asp:ListItem Value="inactivo">Inactivo</asp:ListItem>
                    <asp:ListItem Value="vacaciones">Vacaciones</asp:ListItem>
                </asp:DropDownList>
            </div>
        </div>
        <div class="form-group">
            <asp:Label runat="server" CssClass="col-md-2 control-label">Roles:</asp:Label>
            <div class="col-md-10">
                <asp:CheckBoxList ID="cblRolesEmpleado" runat="server" RepeatDirection="Horizontal" CssClass="checkboxlist-horizontal">
                </asp:CheckBoxList>
            </div>
        </div>


        <div class="form-group">
            <div class="col-md-offset-2 col-md-10">
                <asp:Button runat="server" ID="btnSaveEmpleado" Text="Guardar Cambios" OnClick="btnSaveEmpleado_Click" CssClass="btn btn-primary" ValidationGroup="EmpleadoValidation" />
                <asp:Button runat="server" ID="btnCancelEditEmpleado" Text="Cancelar" OnClick="btnCancelEditEmpleado_Click" CssClass="btn btn-default" CausesValidation="false" />
            </div>
        </div>
    </asp:Panel>

    <asp:GridView ID="gvEmpleados" runat="server" AutoGenerateColumns="False" CssClass="table table-striped table-bordered table-hover"
        DataKeyNames="Id" OnRowCommand="gvEmpleados_RowCommand" AllowPaging="True" PageSize="10" OnPageIndexChanging="gvEmpleados_PageIndexChanging" >
        <Columns>
            <asp:BoundField DataField="Id" HeaderText="User ID" ReadOnly="True" Visible="false" />
            <asp:BoundField DataField="Nombre" HeaderText="Nombre" />
            <asp:BoundField DataField="Apellido" HeaderText="Apellido" />
            <asp:BoundField DataField="Email" HeaderText="Email" />
            <asp:BoundField DataField="NombreHotel" HeaderText="Hotel" />
            <asp:BoundField DataField="NombreCargo" HeaderText="Cargo" />
            <asp:BoundField DataField="Estado" HeaderText="Estado" />
            <asp:TemplateField HeaderText="Roles">
                <ItemTemplate>
                    <%# Eval("Roles") != null ? string.Join(", ", (IList<string>)Eval("Roles")) : "" %>
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="Acciones">
                <ItemTemplate>
                    <asp:LinkButton ID="lnkEditEmpleado" runat="server" CommandName="EditEmpleado" CommandArgument='<%# Eval("Id") %>' Text="Editar" CssClass="btn btn-xs btn-info"></asp:LinkButton>
                    <asp:LinkButton ID="lnkDeleteEmpleado" runat="server" CommandName="DeleteEmpleado" CommandArgument='<%# Eval("Id") %>' Text="Eliminar" CssClass="btn btn-xs btn-danger" OnClientClick="return confirm('¿Está seguro de que desea eliminar este empleado?');"></asp:LinkButton>
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
        <EmptyDataTemplate>
            No se encontraron empleados.
        </EmptyDataTemplate>
    </asp:GridView>
</asp:Content>