﻿namespace SkillMatchPro.API.GraphQL.Inputs;

public class CreateSkillInput
{
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}