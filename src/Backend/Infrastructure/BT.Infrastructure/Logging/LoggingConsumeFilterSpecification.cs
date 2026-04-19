using MassTransit;
using MassTransit.Configuration;

namespace BT.Infrastructure.Logging;

internal sealed class LoggingConsumeFilterSpecification<TConsumer, TMessage> : IPipeSpecification<ConsumerConsumeContext<TConsumer, TMessage>>
    where TConsumer : class
    where TMessage : class
{
    public void Apply(IPipeBuilder<ConsumerConsumeContext<TConsumer, TMessage>> builder)
    {

    }

    public IEnumerable<ValidationResult> Validate()
    {
        yield break;
    }
}
