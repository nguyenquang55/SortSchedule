using SortSchedule.Domain.Enums;
using SortSchedule.Infrastructure.Extensions;

namespace SortSchedule.Tests.Contracts;

public sealed class EnumParserTests
{
    [Fact]
    public void ParseStrictEnum_ShouldTrimAndIgnoreCase()
    {
        const string value = "  oNlInE  ";

        var parsed = value.ParseStrictEnum<DeliveryMode>();

        Assert.Equal(DeliveryMode.Online, parsed);
    }

    [Fact]
    public void ParseStrictEnum_ShouldHandleNormalizedUtf8Input()
    {
        const string decomposedValue = "Cafe\u0301";

        var parsed = decomposedValue.ParseStrictEnum<NormalizedValue>();

        Assert.Equal(NormalizedValue.Caf\u00E9, parsed);
    }

    [Fact]
    public void ParseStrictEnum_WhenValueIsInvalid_ShouldThrowArgumentException()
    {
        Action parse = () => _ = "invalid-mode".ParseStrictEnum<DeliveryMode>();

        Assert.Throws<ArgumentException>(parse);
    }

    private enum NormalizedValue
    {
        Caf\u00E9
    }
}
