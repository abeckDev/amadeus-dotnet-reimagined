using System;
using AbeckDev.Amadeus.Pipeline;

namespace AbeckDev.Amadeus.Test;

public class PipelineContextTests
{
    [Fact]
    public void Constructor_WithoutRequestId_GeneratesNewId()
    {
        // Arrange & Act
        var context = new PipelineContext();

        // Assert
        Assert.NotNull(context.RequestId);
        Assert.NotEmpty(context.RequestId);
        // Should be a GUID without dashes (32 chars)
        Assert.Equal(32, context.RequestId.Length);
    }

    [Fact]
    public void Constructor_WithRequestId_UsesProvidedId()
    {
        // Arrange
        var requestId = "test-request-id";

        // Act
        var context = new PipelineContext(requestId);

        // Assert
        Assert.Equal(requestId, context.RequestId);
    }

    [Fact]
    public void Attempt_DefaultValue_IsZero()
    {
        // Arrange & Act
        var context = new PipelineContext();

        // Assert
        Assert.Equal(0, context.Attempt);
    }

    [Fact]
    public void Attempt_SetValue_ReturnsSetValue()
    {
        // Arrange
        var context = new PipelineContext();

        // Act
        context.Attempt = 3;

        // Assert
        Assert.Equal(3, context.Attempt);
    }

    [Fact]
    public void Items_DefaultValue_IsEmptyDictionary()
    {
        // Arrange & Act
        var context = new PipelineContext();

        // Assert
        Assert.NotNull(context.Items);
        Assert.Empty(context.Items);
    }

    [Fact]
    public void Items_AddItem_ContainsAddedItem()
    {
        // Arrange
        var context = new PipelineContext();
        var key = "testKey";
        var value = "testValue";

        // Act
        context.Items.Add(key, value);

        // Assert
        Assert.True(context.Items.ContainsKey(key));
        Assert.Equal(value, context.Items[key]);
    }

    [Fact]
    public void Items_StoreDifferentTypes_RetrievesCorrectTypes()
    {
        // Arrange
        var context = new PipelineContext();

        // Act
        context.Items.Add("string", "value");
        context.Items.Add("int", 42);
        context.Items.Add("bool", true);
        context.Items.Add("null", null);

        // Assert
        Assert.Equal("value", context.Items["string"]);
        Assert.Equal(42, context.Items["int"]);
        Assert.Equal(true, context.Items["bool"]);
        Assert.Null(context.Items["null"]);
    }
}
