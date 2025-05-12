namespace Mimeo.DynamicUI.Data
{
    /// <summary>
    /// A variant of <see cref="IDataService"/> specific to helping populate a data list for use with selecting one or more items
    /// </summary>
    [Obsolete("Use IReadOnlyDataService instead")]
    public interface IListItemDataService : IReadOnlyDataService
    {
        ListItem ConvertViewModelToListItem(ViewModel viewModel);

        /// <summary>
        /// Gets list items using the values that would be stored in <see cref="ListItem.Value"/>
        /// </summary>
        Task<List<ListItem>> GetListItemsByValues(IEnumerable<string> values);
    }

    [Obsolete("Use IReadOnlyDataService<T> instead")]
    public interface IListItemDataService<TViewModel> : IReadOnlyDataService<TViewModel>, IListItemDataService where TViewModel : ViewModel
    {
        ListItem ConvertViewModelToListItem(TViewModel viewModel);

        ListItem IListItemDataService.ConvertViewModelToListItem(ViewModel viewModel) => ConvertViewModelToListItem((TViewModel)viewModel);
    }
}
