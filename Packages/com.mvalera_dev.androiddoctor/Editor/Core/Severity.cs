namespace AndroidDoctor
{
    public enum Severity { Info = 0, Warning = 1, Error = 2 }

    /// <summary>
    /// Where a finding belongs. General is for findings about the run itself, such as a run that was stopped early.
    /// It is last so that it sorts after the real categories.
    /// </summary>
    public enum Category { Environment, Project, Native, Build, Gradle, General }
}
