using FluentAssertions;
using Mimeo.DynamicUI.Extensions;
using System.Linq.Expressions;

namespace Mimeo.DynamicUI.Tests.Extensions
{
    public class LinqExtensionsTests
    {
        [Fact]
        public void CanIdentifySelectedClassMember()
        {
            // Arrange
            var testClass = new TestClass();
            Expression<Func<string?>> expression = () => testClass.TestString;

            // Act
            var memberName = expression.GetSelectedMember();

            // Assert
            memberName.Name.Should().Be(nameof(TestClass.TestString));
        }

        [Fact]
        public void CanIdentifySelectedNestedClassMember()
        {
            // Arrange
            var testClass = new TestClass()
            {
                NestedClass = new NestedClass()
            };

            Expression<Func<string?>> expression = () => testClass.NestedClass.NestedTestString;

            // Act
            var memberName = expression.GetSelectedMember();

            // Assert
            memberName.Name.Should().Be(nameof(NestedClass.NestedTestString));
        }

        [Fact]
        public void CanIdentifySelectedClassMemberWithCast()
        {
            // Arrange
            var testClass = new TestClass();
            Expression<Func<bool?>> boolExpression = () => testClass.TestNullableBool;
            Expression<Func<object?>> expression = LinqExtensions.Cast<bool?, object?>(boolExpression);

            // Act
            var memberName = expression.GetSelectedMember();

            // Assert
            memberName.Name.Should().Be(nameof(TestClass.TestNullableBool));
        }

        private class TestClass
        {
            public string? TestString { get; set; }
            public bool? TestNullableBool { get; set; }
            public NestedClass? NestedClass { get; set; }
        }
        private class NestedClass
        {
            public string? NestedTestString { get; set; }
        }
    }


}
