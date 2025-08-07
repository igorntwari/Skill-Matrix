using HotChocolate;

namespace SkillMatchPro.API.GraphQL;

public class ErrorFilter : IErrorFilter
{
    private readonly ILogger<ErrorFilter> _logger;

    public ErrorFilter(ILogger<ErrorFilter> logger)
    {
        _logger = logger;
    }

    public IError OnError(IError error)
    {
        _logger.LogError(error.Exception, "GraphQL error occurred: {Message}", error.Message);

        return error.Exception switch
        {
            UnauthorizedAccessException => error.WithMessage("You don't have permission to perform this action")
                                                 .WithCode("UNAUTHORIZED"),

            GraphQLException => error,

            ArgumentException argEx => error.WithMessage($"Invalid input: {argEx.Message}")
                                            .WithCode("INVALID_INPUT"),

            _ => error.WithMessage("An unexpected error occurred")
                      .WithCode("INTERNAL_ERROR")
        };
    }
}
