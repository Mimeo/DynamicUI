using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Mimeo.DynamicUI.Blazor.Controls.ReorderComponent;

public partial class ReorderList<TItem> : ComponentBase
{
    [Parameter]
    public RenderFragment<TItem>? ItemTemplate { get; set; }

    [Parameter]
    public RenderFragment? EmptyTemplate { get; set; }

    [Parameter]
    public List<TItem>? Items { get; set; }

    [Parameter]
    public bool DisplayReorderInterface { get; set; } = true;

    [Parameter]
    public RenderFragment<TItem>? ReorderInterfaceHeaderTemplate { get; set; }

    private int oldIndex;

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
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

    private int GetIndex(TItem item)
    {
        return Items?.FindIndex(a => a?.Equals(item) == true) ?? throw new ArgumentOutOfRangeException(nameof(item));
    }
}