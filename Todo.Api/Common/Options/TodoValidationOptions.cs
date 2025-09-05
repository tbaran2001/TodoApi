namespace Todo.Api.Common.Options;

public class TodoValidationOptions
{
    public int TitleMaxLength { get; set; } = 200;
    public int DescriptionMaxLength { get; set; } = 2000;
    public int CompletionPercentageMin { get; set; }
    public int CompletionPercentageMax { get; set; } = 100;
}