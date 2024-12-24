namespace ElevatorGame.Source.Dialog;

public struct DialogDef(params string[] pagesText)
{
    public string[] PagesText { get; set; }
}