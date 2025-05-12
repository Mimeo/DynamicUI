using FluentAssertions;
using Mimeo.DynamicUI.Data;
using Mimeo.DynamicUI.Data.OData;
using Moq;

namespace Mimeo.DynamicUI.Tests.OData
{
    public class ODataExpressionGeneratorTests
    {
        [Fact]
        public void CanFilterSimpleString()
        {
            // Arrange
            var expressionGenerator = new ODataExpressionGenerator(Mock.Of<IDateTimeConverter>());
            var definition = new DataFieldDefinition("TestClass", typeof(string), "TestProperty");
            var filter = new DataQueryFilter(definition, DataFilterOperator.Equals, "Value");
            var query = new DataQuery
            {
                Filters = [filter]
            };

            // Act
            var expression = expressionGenerator.GenerateODataFilter(query);

            // Assert
            expression.Should().Be("tolower(TestProperty) eq 'value'");
        }

        [Fact]
        public void CanFilterListOfString()
        {
            // Arrange
            var expressionGenerator = new ODataExpressionGenerator(Mock.Of<IDateTimeConverter>());
            var definition = new DataFieldDefinition("TestClass", typeof(List<string>), "TestProperty");
            var filter = new DataQueryFilter(definition, DataFilterOperator.Equals, "Value");
            var query = new DataQuery
            {
                Filters = [filter]
            };

            // Act
            var expression = expressionGenerator.GenerateODataFilter(query);

            // Assert
            expression.Should().Be("TestProperty/any(i: tolower(i) eq 'value')");
        }
        
        [Theory]
        [InlineData(DataFilterOperator.Contains, "TestProperty/any(i: contains(tolower(i), 'value'))")]
        [InlineData(DataFilterOperator.DoesNotContain, "TestProperty/any(i: indexof(tolower(i), 'value') eq -1)")]
        [InlineData(DataFilterOperator.StartsWith, "TestProperty/any(i: startswith(tolower(i), 'value'))")]
        [InlineData(DataFilterOperator.EndsWith, "TestProperty/any(i: endswith(tolower(i), 'value'))")]
        [InlineData(DataFilterOperator.Equals, "TestProperty/any(i: tolower(i) eq 'value')")]
        [InlineData(DataFilterOperator.IsNull, "TestProperty/any(i: tolower(i) eq null)")]
        [InlineData(DataFilterOperator.IsEmpty, "TestProperty/any(i: tolower(i) eq '')")]
        public void CanFilter_CaseInsensitive(DataFilterOperator @operator, string expectedQuery)
        {
            // Arrange
            var expressionGenerator = new ODataExpressionGenerator(Mock.Of<IDateTimeConverter>());
            var definition = new DataFieldDefinition("TestClass", typeof(List<string>), "TestProperty");
            var filter = new DataQueryFilter(definition, @operator, "Value");
            var query = new DataQuery
            {
                Filters = [filter]
            };

            // Act
            var expression = expressionGenerator.GenerateODataFilter(query);

            // Assert
            expression.Should().Be(expectedQuery);
        }
        
        [Theory]
        [InlineData(DataFilterOperator.Contains, "TestProperty/any(i: contains(i, 'Value'))")]
        [InlineData(DataFilterOperator.DoesNotContain, "TestProperty/any(i: indexof(i, 'Value') eq -1)")]
        [InlineData(DataFilterOperator.StartsWith, "TestProperty/any(i: startswith(i, 'Value'))")]
        [InlineData(DataFilterOperator.EndsWith, "TestProperty/any(i: endswith(i, 'Value'))")]
        [InlineData(DataFilterOperator.Equals, "TestProperty/any(i: i eq 'Value')")]
        [InlineData(DataFilterOperator.IsNull, "TestProperty/any(i: i eq null)")]
        [InlineData(DataFilterOperator.IsEmpty, "TestProperty/any(i: i eq '')")]
        public void CanFilter_CaseSensitive(DataFilterOperator @operator, string expectedQuery)
        {
            // Arrange
            var expressionGenerator = new ODataExpressionGenerator(Mock.Of<IDateTimeConverter>());
            var definition = new DataFieldDefinition("TestClass", typeof(List<string>), "TestProperty");
            var filter = new DataQueryFilter(definition, @operator, "Value")
            {
                IgnoreCase = false
            };
            
            var query = new DataQuery
            {
                Filters = [filter]
            };

            // Act
            var expression = expressionGenerator.GenerateODataFilter(query);

            // Assert
            expression.Should().Be(expectedQuery);
        }
    }
}
