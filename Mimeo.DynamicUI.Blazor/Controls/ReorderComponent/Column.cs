using Microsoft.AspNetCore.Components;

namespace Mimeo.DynamicUI.Blazor.Controls.ReorderComponent;

public class Column<TItem> : ComponentBase
{
    public Column()
    {
        HeaderTemplate = new RenderFragment(builder =>
        {
            builder.AddContent(0, Header);
        });
    }

    [CascadingParameter]
    public ITableComponent<TItem>? Table { get; set; }

    [Parameter]
    public string? Header { get; set; }

    [Parameter]
    public RenderFragment HeaderTemplate { get; set; }

    [Parameter]
    public RenderFragment<TItem>? DataTemplate { get; set; }

    [Parameter]
    public Func<TItem, string>? Style { get; set; }

    protected override void OnInitialized()
    {
        base.OnInitialized();

        if (Table != null)
        {
            Table.AddColumn(this);
        }
    }
}