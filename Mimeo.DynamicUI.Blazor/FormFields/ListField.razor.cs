using Microsoft.AspNetCore.Components;
using Mimeo.DynamicUI.Blazor.Forms;

namespace Mimeo.DynamicUI.Blazor.FormFields
{
    public partial class ListField<T> : FormFieldBase<List<T>>
    {
        private ListFieldDefinition<T>? ListDefinition => (ListFieldDefinition<T>?)Definition;

        private IEnumerable<SimpleViewModel>? simpleViewModels;

        // If T isn't a view model, we need to put it inside one
        // This lets us handle view models that define their own table layouts,
        // as well as simple things like strings
        private bool wrapInViewModel;

        protected override async Task OnParametersSetAsync()
        {
            wrapInViewModel = !typeof(T).IsAssignableTo(typeof(ViewModel));
            if (wrapInViewModel)
            {
                this.simpleViewModels = await LoadDataSimple();
            }
        }

        private RenderFragment RenderNonSimpleInlineEditGrid()
        {
            return new RenderFragment(builder =>
            {
                var i = 0;

                // We have to build the control in the code behind, because InlineEditGrid<TViewModel> requires TViewModel to inherit ViewModel, but the compiler can't ensure T fulfills this requirement
                builder.OpenComponent(i++, typeof(InlineEditGrid<>).MakeGenericType(typeof(T)));
                builder.AddAttribute(i++, nameof(InlineEditGrid<ViewModel>.FullInlineEditing), true);
                builder.AddAttribute(i++, nameof(InlineEditGrid<ViewModel>.ListMethod), LoadData);
                builder.AddAttribute(i++, nameof(InlineEditGrid<ViewModel>.ViewModelConstructor), CreateViewModel);
                builder.AddAttribute(i++, nameof(InlineEditGrid<ViewModel>.DisableTaskRunningDetection), true);
                if (!ReadOnly)
                {
                    builder.AddAttribute(i++, nameof(InlineEditGrid<ViewModel>.CreateMethod), OnCreate);
                    builder.AddAttribute(i++, nameof(InlineEditGrid<ViewModel>.UpdateMethod), OnUpdate);
                    builder.AddAttribute(i++, nameof(InlineEditGrid<ViewModel>.DeleteMethod), OnDelete);
                }
                builder.CloseComponent();
            });
        }

        private Task<IEnumerable<T>> LoadData()
        {
            return Task.FromResult(Value ?? Enumerable.Empty<T>());
        }

        private Task<IEnumerable<SimpleViewModel>> LoadDataSimple()
        {
            if (Value == null)
            {
                return Task.FromResult(Enumerable.Empty<SimpleViewModel>());
            }

            if (ListDefinition == null)
            {
                throw new InvalidOperationException("Definition must be a ListFieldDefinition");
            }

            var items = new List<SimpleViewModel>(Value.Count);
            for (int i = 0; i < Value.Count; i++)
            {
                int i2 = i; // Referencing i directly won't work since we're constantly changing it
                items.Add(new SimpleViewModel(ListDefinition, () => Value[i2], value => Value[i2] = value));
            }
            return Task.FromResult<IEnumerable<SimpleViewModel>>(items);
        }

        private Task OnCreate(T viewModel)
        {
            var convertedViewModel = (T)(object)viewModel!;
            Value?.Add(convertedViewModel);
            return Task.CompletedTask;
        }

        private Task OnCreateSimple(SimpleViewModel viewModel)
        {
            return Task.CompletedTask;
        }

        private Task OnUpdate(T viewModel)
        {
            // Nothing to do, we're updating the referenced object directly
            return Task.CompletedTask;
        }

        private Task OnUpdateSimple(SimpleViewModel viewModel)
        {
            Value = simpleViewModels?.Select(v => v.Value)?.ToList();
            return Task.CompletedTask;
        }

        private Task OnDelete(T viewModel)
        {
            Value?.Remove(viewModel);
            return Task.CompletedTask;
        }

        private Task OnDeleteSimple(SimpleViewModel viewModel)
        {
            Value?.Remove(viewModel.Value);
            return Task.CompletedTask;
        }

        private T CreateViewModel()
        {
            if (ListDefinition == null)
            {
                throw new InvalidOperationException("Definition must be a ListFieldDefinition");
            }

            return (T)(object)ListDefinition.CreateNewItem()!;
        }

        private SimpleViewModel CreateViewModelSimple()
        {
            if (Value == null)
            {
                throw new InvalidOperationException("Value cannot be null when adding a new item");
            }

            if (ListDefinition == null)
            {
                throw new InvalidOperationException("Definition must be a ListFieldDefinition");
            }

            Value.Add(ListDefinition.CreateNewItem());
            var i = Value.Count - 1;
            return new SimpleViewModel(ListDefinition, () => Value[i], value => Value[i] = value);
        }

        private IEnumerable<FormFieldDefinition> GetListFormFieldsSimple()
        {
            if (ListDefinition == null)
            {
                throw new InvalidOperationException("Definition must be a ListFieldDefinition");
            }

            return new SimpleViewModel(ListDefinition, () => ListDefinition.CreateNewItem(), value => { }).GetListForm().Values;
        }

        private class SimpleViewModel : ViewModel
        {
            public SimpleViewModel(ListFieldDefinition<T> stackFieldDefinition, Func<T> getter, Action<T> setter)
            {
                this.stackFieldDefinition = stackFieldDefinition ?? throw new ArgumentNullException(nameof(stackFieldDefinition));
                this.getter = getter ?? throw new ArgumentNullException(nameof(getter));
                this.setter = setter ?? throw new ArgumentNullException(nameof(setter));
            }

            private readonly ListFieldDefinition<T> stackFieldDefinition;
            private readonly Func<T> getter;
            private readonly Action<T> setter;

            public T Value { get => getter(); set => setter(value); }

            protected override IEnumerable<FormFieldDefinition> GetEditFormFields()
            {
                var formField = FormField(stackFieldDefinition.GetItemFormFieldType(), () => Value);
                formField.LanguageKey = stackFieldDefinition.LanguageKey + "_header";
                yield return formField;
            }
        }
    }
}
