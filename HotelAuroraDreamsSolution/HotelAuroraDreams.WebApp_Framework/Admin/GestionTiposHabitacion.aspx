<%@ Page Title="Gestión de Tipos de Habitación" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="GestionTiposHabitacion.aspx.cs" Inherits="HotelAuroraDreams.WebApp_Framework.Admin.GestionTiposHabitacion" Async="true" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h2><%: Title %></h2>
    <asp:Label ID="lblMessage" runat="server" CssClass="text-danger" EnableViewState="false"></asp:Label>
    <asp:Label ID="lblSuccessMessage" runat="server" CssClass="text-success" EnableViewState="false"></asp:Label>

    <asp:Panel ID="pnlForm" runat="server" Visible="false" CssClass="form-horizontal" style="margin-bottom: 20px; padding:15px; border: 1px solid #ccc; border-radius:5px;">
        <h3><asp:Literal ID="litFormTitle" runat="server">Añadir Nuevo Tipo de Habitación</asp:Literal></h3>
        <asp:HiddenField ID="hfTipoHabitacionID" runat="server" Value="0" />
        
        <div class="form-group">
            <asp:Label runat="server" AssociatedControlID="txtNombre" CssClass="col-md-2 control-label">Nombre:</asp:Label>
            <div class="col-md-10">
                <asp:TextBox runat="server" ID="txtNombre" CssClass="form-control" />
                <asp:RequiredFieldValidator runat="server" ControlToValidate="txtNombre" ErrorMessage="El nombre es requerido." CssClass="text-danger" Display="Dynamic" />
            </div>
        </div>
        <div class="form-group">
            <asp:Label runat="server" AssociatedControlID="txtDescripcion" CssClass="col-md-2 control-label">Descripción:</asp:Label>
            <div class="col-md-10">
                <asp:TextBox runat="server" ID="txtDescripcion" TextMode="MultiLine" Rows="3" CssClass="form-control" />
            </div>
        </div>
        <div class="form-group">
            <asp:Label runat="server" AssociatedControlID="txtPrecioBase" CssClass="col-md-2 control-label">Precio Base:</asp:Label>
            <div class="col-md-10">
                <asp:TextBox runat="server" ID="txtPrecioBase" CssClass="form-control" />
                <asp:RequiredFieldValidator runat="server" ControlToValidate="txtPrecioBase" ErrorMessage="El precio base es requerido." CssClass="text-danger" Display="Dynamic" />
                <asp:CompareValidator runat="server" ControlToValidate="txtPrecioBase" Operator="DataTypeCheck" Type="Currency" ErrorMessage="Ingrese un valor monetario válido." CssClass="text-danger" Display="Dynamic" />
            </div>
        </div>
        <div class="form-group">
            <asp:Label runat="server" AssociatedControlID="txtCapacidad" CssClass="col-md-2 control-label">Capacidad:</asp:Label>
            <div class="col-md-10">
                <asp:TextBox runat="server" ID="txtCapacidad" CssClass="form-control" TextMode="Number" />
                <asp:RequiredFieldValidator runat="server" ControlToValidate="txtCapacidad" ErrorMessage="La capacidad es requerida." CssClass="text-danger" Display="Dynamic" />
                <asp:CompareValidator runat="server" ControlToValidate="txtCapacidad" Operator="DataTypeCheck" Type="Integer" ErrorMessage="Ingrese un número entero válido." CssClass="text-danger" Display="Dynamic" />
            </div>
        </div>
        <div class="form-group">
            <asp:Label runat="server" AssociatedControlID="txtComodidades" CssClass="col-md-2 control-label">Comodidades:</asp:Label>
            <div class="col-md-10">
                <asp:TextBox runat="server" ID="txtComodidades" TextMode="MultiLine" Rows="3" CssClass="form-control" />
            </div>
        </div>
        <div class="form-group">
            <div class="col-md-offset-2 col-md-10">
                <asp:Button runat="server" ID="btnSave" Text="Guardar" OnClick="btnSave_Click" CssClass="btn btn-primary" />
                <asp:Button runat="server" ID="btnCancel" Text="Cancelar" OnClick="btnCancel_Click" CssClass="btn btn-default" CausesValidation="false" />
            </div>
        </div>
    </asp:Panel>

    <asp:Button ID="btnShowAddForm" runat="server" Text="Añadir Nuevo Tipo de Habitación" OnClick="btnShowAddForm_Click" CssClass="btn btn-success" style="margin-bottom: 15px;" CausesValidation="false" />

    <asp:GridView ID="gvTiposHabitacion" runat="server" AutoGenerateColumns="False" CssClass="table table-striped table-bordered table-hover"
        DataKeyNames="tipo_habitacion_id" OnRowCommand="gvTiposHabitacion_RowCommand" 
        OnRowEditing="gvTiposHabitacion_RowEditing" OnRowCancelingEdit="gvTiposHabitacion_RowCancelingEdit" 
        OnRowUpdating="gvTiposHabitacion_RowUpdating" OnRowDeleting="gvTiposHabitacion_RowDeleting">
        <Columns>
            <asp:BoundField DataField="tipo_habitacion_id" HeaderText="ID" ReadOnly="True" SortExpression="tipo_habitacion_id" />
            <asp:BoundField DataField="nombre" HeaderText="Nombre" SortExpression="nombre" />
            <asp:BoundField DataField="descripcion" HeaderText="Descripción" SortExpression="descripcion" />
            <asp:BoundField DataField="precio_base" HeaderText="Precio Base" DataFormatString="{0:C}" SortExpression="precio_base" />
            <asp:BoundField DataField="capacidad" HeaderText="Capacidad" SortExpression="capacidad" />
            <asp:BoundField DataField="comodidades" HeaderText="Comodidades" SortExpression="comodidades" />
            <asp:TemplateField HeaderText="Acciones">
                <ItemTemplate>
                    <asp:LinkButton ID="lnkEdit" runat="server" CommandName="Edit" CommandArgument='<%# Eval("tipo_habitacion_id") %>' Text="Editar" CssClass="btn btn-xs btn-info"></asp:LinkButton>
                    <asp:LinkButton ID="lnkDelete" runat="server" CommandName="Delete" CommandArgument='<%# Eval("tipo_habitacion_id") %>' Text="Eliminar" CssClass="btn btn-xs btn-danger" OnClientClick="return confirm('¿Está seguro de que desea eliminar este tipo de habitación?');"></asp:LinkButton>
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:LinkButton ID="lnkUpdate" runat="server" CommandName="Update" Text="Actualizar" CssClass="btn btn-xs btn-success"></asp:LinkButton>
                    <asp:LinkButton ID="lnkCancel" runat="server" CommandName="Cancel" Text="Cancelar" CssClass="btn btn-xs btn-warning"></asp:LinkButton>
                </EditItemTemplate>
            </asp:TemplateField>
        </Columns>
        <EmptyDataTemplate>
            No se encontraron tipos de habitación.
        </EmptyDataTemplate>
    </asp:GridView>
</asp:Content>