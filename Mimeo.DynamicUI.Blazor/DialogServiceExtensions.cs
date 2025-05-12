using Microsoft.AspNetCore.Components;
using Mimeo.DynamicUI.Blazor.Forms;
using Mimeo.DynamicUI.Blazor.Views;
using Mimeo.DynamicUI.Data;
using Radzen;

namespace Mimeo.DynamicUI.Blazor;

public static class DialogServiceExtensions
{
    public static async Task<bool?> OpenEditDialogAsync(this DialogService dialogService, string title, ViewModel viewModel, string width = "1000px", Dictionary<string, Type>? customFormFieldTypes = null)
    {
        var result = await dialogService.OpenAsync<DynamicEditForm>(title, new Dictionary<string, object>
            {
                { nameof(DynamicEditForm.ViewModel), viewModel },
                { nameof(DynamicEditForm.IsModal), true },
                { nameof(DynamicEditForm.ShowCancelButton), true },
                { nameof(DynamicEditForm.CustomFormFieldTypes), customFormFieldTypes ?? [] },
                {
                    nameof(DynamicEditForm.OnSubmit),
                    EventCallback.Factory.Create(dialogService, () =>
                    {
                        dialogService.Close(true);
                    })
                },
                {
                    nameof(DynamicEditForm.OnCancel),
                    EventCallback.Factory.Create(dialogService, () =>
                    {
                        dialogService.Close(false);
                    })
                }
            },
            new DialogOptions
            {
                Width = width
            });

        return (bool?)result;
    }

    public static async Task<bool?> OpenViewDialogAsync(this DialogService dialogService, string title, ViewModel viewModel, string width = "1000px", Dictionary<string, Type>? customFormFieldTypes = null)
    {
        var result = await dialogService.OpenAsync<DynamicView>(title, new Dictionary<string, object>
            {
                { nameof(DynamicView.ViewModel), viewModel },
                { nameof(DynamicView.IsModal), true },
                { nameof(DynamicView.CustomFormFieldTypes), customFormFieldTypes ?? [] },
            },
            new DialogOptions
            {
                Width = width
            });

        return (bool?)result;
    }

    public static async Task<bool?> OpenImportDialogAsync(this DialogService dialogService, string title, IImportExportDataService importExportDataService, Func<ImportEventArgs<object?>, Task> onImport, string width = "1000px")
    {
        var result = await dialogService.OpenAsync<ImportForm>(title, new Dictionary<string, object?>
            {
                { nameof(ImportForm.ImportExportDataService), importExportDataService },
                { nameof(ImportForm.IsModal), true },
                { nameof(ImportForm.ShowCancelButton), true },
                {
                    nameof(ImportForm.OnImport),
                    EventCallback.Factory.Create<ImportEventArgs<object?>>(dialogService, async args =>
                    {
                        await onImport(args);
                        dialogService.Close(true);
                    })
                },
                {
                    nameof(ImportForm.OnCancel),
                    EventCallback.Factory.Create(dialogService, () =>
                    {
                        dialogService.Close(false);
                    })
                }
            },
            new DialogOptions
            {
                Width = width
            });

        return (bool?)result;
    }

    public static async Task<bool> OpenConfirmation(this DialogService dialogService, string title, string message, string? yesText = null, string? noText = null)
    {
        var result = await dialogService.OpenAsync<ConfirmationForm>(title, new Dictionary<string, object?>
        {
            { nameof(ConfirmationForm.Message), message },
            { nameof(ConfirmationForm.YesText), yesText },
            { nameof(ConfirmationForm.NoText), noText },
            {
                nameof(ConfirmationForm.OnYes),
                EventCallback.Factory.Create(dialogService, () =>
                {
                    dialogService.Close(true);
                })
            },
            {
                nameof(ConfirmationForm.OnNo),
                EventCallback.Factory.Create(dialogService, () =>
                {
                    dialogService.Close(false);
                })
            }
        });

        return ((bool?)result).GetValueOrDefault();
    }
}