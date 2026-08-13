using AwesomeAssertions;
using Bunit;
using Microsoft.AspNetCore.Components;
using Mimeo.DynamicUI.Blazor.Controls;
using Radzen;
using Radzen.Blazor;

namespace Mimeo.DynamicUI.Blazor.Tests.Controls
{
    public class LinkButtonTests
    {
        [Fact]
        public void RendersAnAnchorPointingAtPath()
        {
            using var context = CreateContext();

            var component = context.Render(RenderLinkButton("/orders/MEM/12345"));

            var anchor = component.Find("a");
            anchor.GetAttribute("href").Should().Be("/orders/MEM/12345");
        }

        [Fact]
        public void LooksTheSameAsTheEquivalentRadzenButton()
        {
            using var context = CreateContext();

            var link = context.Render(RenderLinkButton("/orders/MEM/12345")).Find("a");
            var button = context.Render(RenderRadzenButton()).Find("button");

            link.GetAttribute("class").Should().Be(button.GetAttribute("class"));
            link.InnerHtml.Should().Be(button.InnerHtml);
        }

        [Fact]
        public void DisabledLinkHasNoDestination()
        {
            using var context = CreateContext();

            var component = context.Render((RenderFragment)(builder =>
            {
                builder.OpenComponent<LinkButton>(0);
                builder.AddAttribute(1, nameof(LinkButton.Path), "/orders/MEM/12345");
                builder.AddAttribute(2, nameof(LinkButton.Disabled), true);
                builder.CloseComponent();
            }));

            component.Find("a").HasAttribute("href").Should().BeFalse();
        }

        private static RenderFragment RenderLinkButton(string path) => builder =>
        {
            builder.OpenComponent<LinkButton>(0);
            builder.AddAttribute(1, nameof(LinkButton.ButtonStyle), ButtonStyle.Base);
            builder.AddAttribute(2, nameof(LinkButton.Icon), "arrow_forward");
            builder.AddAttribute(3, nameof(LinkButton.Size), ButtonSize.Small);
            builder.AddAttribute(4, nameof(LinkButton.Text), "View Recipient");
            builder.AddAttribute(5, nameof(LinkButton.Path), path);
            builder.CloseComponent();
        };

        private static RenderFragment RenderRadzenButton() => builder =>
        {
            builder.OpenComponent<RadzenButton>(0);
            builder.AddAttribute(1, nameof(RadzenButton.ButtonStyle), ButtonStyle.Base);
            builder.AddAttribute(2, nameof(RadzenButton.Icon), "arrow_forward");
            builder.AddAttribute(3, nameof(RadzenButton.Size), ButtonSize.Small);
            builder.AddAttribute(4, nameof(RadzenButton.Text), "View Recipient");
            builder.CloseComponent();
        };

        private static BunitContext CreateContext()
        {
            var context = new BunitContext();
            context.JSInterop.Mode = JSRuntimeMode.Loose;
            return context;
        }
    }

}
