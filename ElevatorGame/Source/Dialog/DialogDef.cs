namespace ElevatorGame.Source.Dialog;

public struct DialogDef(params string[] pagesText)
{
    public string[] PagesText { get; set; } = pagesText;

    public readonly Dialog.Page[] Pages
        => [..
            (PagesText?.SelectMany(
                p => p
                    .Split("<PAGE>", StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => new Dialog.Page { Content = s })
            ) ?? [])
        ];
}
