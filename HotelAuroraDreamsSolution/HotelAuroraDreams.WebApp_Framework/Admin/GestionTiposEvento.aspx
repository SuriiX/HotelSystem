<%@ Page Title="Gestión de Tipos de Evento" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="GestionTiposEvento.aspx.cs" Inherits="HotelAuroraDreams.WebApp_Framework.Admin.GestionTiposEvento" Async="true" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h2><%: Title %></h2>
    <asp:Label ID="lblMessage" runat="server" CssClass="text-danger" EnableViewState="false"></asp:Label>
    <asp:Label ID="lblSuccessMessage" runat="server" CssClass="text-success" EnableViewState="false"></asp:Label>

    <asp:Panel ID="pnlFormTipoEvento" runat="server" Visible="false" CssClass="form-horizontal" style="margin-bottom: 20px; padding:15px; border: 1px solid #ccc; border-radius:5px;">
        <h3><asp:Literal ID="litFormTitle" runat="server">Añadir Nuevo Tipo de Evento</asp:Literal></h3>
        <asp:HiddenField ID="hfTipoEventoID" runat="server" Value="0" />

        <div class="form-group">
            <asp:Label runat="server" AssociatedControlID="txtNombreTipoEvento" CssClass="col-md-2 control-label">Nombre del Tipo:</asp:Label>
            <div class="col-md-10">
                <asp:TextBox runat="server" ID="txtNombreTipoEvento" CssClass="form-control" />
                <asp:RequiredFieldValidator runat="server" ControlToValidate="txtNombreTipoEvento" ErrorMessage="El nombre es requerido." CssClass="text-danger" Display="Dynamic" ValidationGroup="TipoEventoValidation" />
            </div>
        </div>
        <div class="form-group">
            <asp:Label runat="server" AssociatedControlID="txtDescripcionTipoEvento" CssClass="col-md-2 control-label">Descripción:</asp:Label>
            <div class="col-md-10">
                <asp:TextBox runat="server" ID="txtDescripcionTipoEvento" TextMode="MultiLine" Rows="3" CssClass="form-control" />
            </div>
        </div>
        <div class="form-group">
            <div class="col-md-offset-2 col-md-10">
                <asp:Button runat="server" ID="btnSaveTipoEvento" Text="Guardar Tipo de Evento" OnClick="btnSaveTipoEvento_Click" CssClass="btn btn-primary" ValidationGroup="TipoEventoValidation" />
                <asp:Button runat="server" ID="btnCancelTipoEvento" Text="Cancelar" OnClick="btnCancelTipoEvento_Click" CssClass="btn btn-default" CausesValidation="false" />
            </div>
        </div>
    </asp:Panel>

    <asp:Button ID="btnShowAddTipoEventoForm" runat="server" Text="Añadir Nuevo Tipo de Evento" OnClick="btnShowAddTipoEventoForm_Click" CssClass="btn btn-success" style="margin-bottom: 15px;" CausesValidation="false" />

    <asp:GridView ID="gvTiposEvento" runat="server" AutoGenerateColumns="False" CssClass="table table-striped table-bordered table-hover"
        DataKeyNames="TipoEventoID" OnRowCommand="gvTiposEvento_RowCommand" AllowPaging="True" PageSize="10" OnPageIndexChanging="gvTiposEvento_PageIndexChanging">
        <Columns>
            <asp:BoundField DataField="TipoEventoID" HeaderText="ID" ReadOnly="True" />
            <asp:BoundField DataField="NombreTipo" HeaderText="Nombre del Tipo" />
            <asp:BoundField DataField="Descripcion" HeaderText="Descripción" />
            <asp:TemplateField HeaderText="Acciones">
                <ItemTemplate>
                    <asp:LinkButton ID="lnkEditTipoEvento" runat="server" CommandName="EditTipoEvento" CommandArgument='<%# Eval("TipoEventoID") %>' Text="Editar" CssClass="btn btn-xs btn-info"></asp:LinkButton>
                    <asp:LinkButton ID="lnkDeleteTipoEvento" runat="server" CommandName="DeleteTipoEvento" CommandArgument='<%# Eval("TipoEventoID") %>' Text="Eliminar" CssClass="btn btn-xs btn-danger" OnClientClick="return confirm('¿Está seguro de que desea eliminar este tipo de evento?');"></asp:LinkButton>
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
        <EmptyDataTemplate>
            No se encontraron tipos de evento.
        </EmptyDataTemplate>
    </asp:GridView>
</asp:Content>