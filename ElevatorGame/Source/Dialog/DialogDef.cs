namespace ElevatorGame.Source.Dialog;

public struct DialogDef(params string[] pagesText)
{
    public string[] PagesText { get; set; } = pagesText;

    public readonly Dialog.Page[] Pages
    {
        get
        {
            Dialog.Page[] pages = new Dialog.Page[PagesText?.Length ?? 0];
            for (int i = 0; i < pages.Length; i++)
            {
                pages[i] = new Dialog.Page
                {
                    Content = PagesText[i]
                };
            }
            return pages;
        }
    }
}
