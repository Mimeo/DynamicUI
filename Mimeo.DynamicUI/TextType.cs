namespace Mimeo.DynamicUI
{
    public enum TextType
    {
        /// <summary>
        /// Text field should be rendered as a single-line text box
        /// </summary>
        SingleLine,

        /// <summary>
        /// Text field should be rendered as a multi-line text box
        /// </summary>
        MultiLine,

        /// <summary>
        /// Text field should be rendered as a WYSIWYG HTML editor (e.g. CKEditor, RadzenHtmlEditor, etc.)
        /// </summary>
        HTML,

        [Obsolete("Use Code instead and specify the language")]
        JSON,

        /// <summary>
        /// Text field should be rendered as a multi-language code editor (e.g. Monaco Editor)
        /// </summary>
        Code
    }
}
