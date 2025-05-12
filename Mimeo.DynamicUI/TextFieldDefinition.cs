using Mimeo.DynamicUI.Extensions;
using System.Linq.Expressions;

namespace Mimeo.DynamicUI
{
    public class TextFieldDefinition : FormFieldDefinition
    {
        public TextFieldDefinition(Expression<Func<string?>> @for)
            : base(FormFieldType.Text, LinqExtensions.Cast<string?, object?>(@for))
        {
        }

        public string? PlaceholderLanguageKey { get; set; }

        public TextType TextType
        {
            get => textType;
            set 
            {
#pragma warning disable CS0618 // Type or member is obsolete
                if (value == TextType.JSON)
                {
                    textType = TextType.Code;
                    CodeLanguage = "json";
                }
                else
                {
                    textType = value;
                }
#pragma warning restore CS0618 // Type or member is obsolete
            }
        }
        private TextType textType;

        /// <summary>
        /// If <see cref="TextType"/> equals <see cref="TextType.Code"/>, the language of the code.
        /// Valid values include "json", "html", "css", "typescript", etc.
        /// </summary>
        public string? CodeLanguage { get; set; }

        /// <summary>
        /// An optional set of values that can be selected as a combobox. Requires <see cref="TextType"/> to be <see cref="DynamicUI.TextType.SingleLine"/>.
        /// </summary>
        public List<string>? Items { get; set; }
    }
}
