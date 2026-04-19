using MassTransit;
using MassTransit.Configuration;

namespace BT.Infrastructure.Logging;

internal class LoggingPipeSpecification<TConsumer> : IPipeSpecification<ConsumerConsumeContext<TConsumer>> where TConsumer : class
{
    public void Apply(IPipeBuilder<ConsumerConsumeContext<TConsumer>> builder)
    {

    }

    public IEnumerable<ValidationResult> Validate()
    {
        yield break;
    }
}