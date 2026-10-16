using AwesomeAssertions;
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

        [Theory]
        [InlineData(true, "'", "tolower(TestProperty) eq ''''")]
        [InlineData(true, "O'Brien", "tolower(TestProperty) eq 'o''brien'")]
        [InlineData(false, "O'Brien", "TestProperty eq 'O''Brien'")]
        [InlineData(false, "''", "TestProperty eq ''''''")]
        public void CanFilterString_EscapesSingleQuotes(bool ignoreCase, string value, string expectedQuery)
        {
            // Arrange
            var expressionGenerator = new ODataExpressionGenerator(Mock.Of<IDateTimeConverter>());
            var definition = new DataFieldDefinition("TestClass", typeof(string), "TestProperty");
            var filter = new DataQueryFilter(definition, DataFilterOperator.Equals, value)
            {
                IgnoreCase = ignoreCase
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

        [Fact]
        public void CanFilterStringIn_EscapesSingleQuotes()
        {
            // Arrange
            var expressionGenerator = new ODataExpressionGenerator(Mock.Of<IDateTimeConverter>());
            var definition = new DataFieldDefinition("TestClass", typeof(string), "TestProperty");
            var filter = new DataQueryFilter(definition, DataFilterOperator.In, new List<string> { "O'Brien", "Smith" });
            var query = new DataQuery
            {
                Filters = [filter]
            };

            // Act
            var expression = expressionGenerator.GenerateODataFilter(query);

            // Assert
            expression.Should().Be("tolower(TestProperty) in ('o''brien','smith')");
        }

        [Fact]
        public void CanFilterListOfString_EscapesSingleQuotes()
        {
            // Arrange
            var expressionGenerator = new ODataExpressionGenerator(Mock.Of<IDateTimeConverter>());
            var definition = new DataFieldDefinition("TestClass", typeof(List<string>), "TestProperty");
            var filter = new DataQueryFilter(definition, DataFilterOperator.Contains, "'");
            var query = new DataQuery
            {
                Filters = [filter]
            };

            // Act
            var expression = expressionGenerator.GenerateODataFilter(query);

            // Assert
            expression.Should().Be("TestProperty/any(i: contains(tolower(i), ''''))");
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
