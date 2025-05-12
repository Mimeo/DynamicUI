namespace Mimeo.DynamicUI.Blazor.Controls.ReorderComponent;

public interface ITableComponent<TItem>
{
    void AddColumn(Column<TItem> column);
}