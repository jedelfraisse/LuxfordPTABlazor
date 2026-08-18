namespace LuxfordPTAWeb.Shared.Enums;

/// <summary>How the commit step should handle rows whose Email already exists for the target school year.</summary>
public enum DuplicateImportStrategy
{
    Skip = 1,   // Leave the existing record untouched
    Overwrite = 2 // Update the existing record's fields with the imported row's values
}
