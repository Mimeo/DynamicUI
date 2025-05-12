using Microsoft.AspNetCore.Components;

namespace Mimeo.DynamicUI.Blazor.Controls.ReorderComponent;

public partial class ReorderTable<TItem> : ComponentBase, ITableComponent<TItem>
{
    [Parameter]
    public RenderFragment<TItem>? ItemTemplate { get; set; }

    [Parameter]
    public RenderFragment? EmptyTemplate { get; set; }

    /// <summary>
    /// A render fragment containing table columns
    /// </summary>
    /// <remarks>
    /// Add columns to the component inside <Columns>.
    /// We'll fake-render the columns, and each column will add itself to a private collection of column objects.
    /// We'll then really render specific pieces of each column in the appropriate place.
    /// This is how Radzen does it as of 2022-05-24.
    /// </remarks>
    [Parameter]
    public RenderFragment? Columns { get; set; }
    private List<Column<TItem>> columns = new();

    [Parameter]
    public List<TItem>? Items { get; set; }

    private int oldIndex;

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
    }

    public void AddColumn(Column<TItem> column)
    {
        columns.Add(column);
    }

    protected void OnDragStart(TItem draggedItem)
    {
        oldIndex = GetIndex(draggedItem);
    }

    protected void OnDrop(TItem targetItem)
    {
        if (Items == null || targetItem == null)
        {
            return;
        }

        var newIndex = GetIndex(targetItem);
        var draggedItem = Items[oldIndex];

        Items.RemoveAt(oldIndex);
        Items.Insert(newIndex, draggedItem);

        oldIndex = newIndex;

        StateHasChanged();
    }

    protected void MoveUp(TItem clickedItem)
    {
        if (Items == null || clickedItem == null)
        {
            return;
        }

        oldIndex = GetIndex(clickedItem);
        var newIndex = Math.Max(GetIndex(clickedItem) - 1, 0);

        Items.RemoveAt(oldIndex);
        Items.Insert(newIndex, clickedItem);

        oldIndex = newIndex;

        StateHasChanged();
    }

    protected void MoveDown(TItem clickedItem)
    {
        if (Items == null || clickedItem == null)
        {
            return;
        }

        oldIndex = GetIndex(clickedItem);
        var newIndex = Math.Min(GetIndex(clickedItem) + 1, Items.Count - 1);

        Items.RemoveAt(oldIndex);
        Items.Insert(newIndex, clickedItem);

        oldIndex = newIndex;

        StateHasChanged();
    }

    private int GetIndex(TItem item)
    {
        return Items?.FindIndex(a => a?.Equals(item) == true) ?? throw new ArgumentOutOfRangeException(nameof(item));
    }
}