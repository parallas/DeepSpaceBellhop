using System;
using System.Linq;
using ElevatorGame.Source.Characters;
using Engine;

namespace ElevatorGame.Source.Dialog;

public static class DialogParser
{
    public static Dialog.Page[] GetRandomDialog(DialogDef[] dialogDefs, out Dialog.DisplayMethod displayMethod)
    {
        Dialog.Page[] rawPages;
        displayMethod = Dialog.DisplayMethod.Human;
        if (dialogDefs.Length == 0)
        {
            int randomCharCout = Random.Shared.Next(3, 30);
            string randomString = new string(Enumerable.Range(0, randomCharCout)
                .Select(_ => (char)Random.Shared.Next('a', 'z' + 1)).ToArray());
            rawPages = [new Dialog.Page() { Content = randomString }];
            displayMethod = Dialog.DisplayMethod.Alien;
        }
        else
        {
            rawPages = dialogDefs[Random.Shared.Next(dialogDefs.Length)]
                .Pages;
        }

        return rawPages;
    }

    public static Dialog.Page[] ParseCharacterDialog(Dialog.Page[] pages, CharacterActor characterActor)
    {
        Dialog.Page[] newPages = new Dialog.Page[pages.Length];
        for (int i = 0; i < pages.Length; i++)
        {
            ref var page = ref newPages[i];
            page.Content = pages[i].Content.Replace("$floorNumDest", characterActor.FloorNumberTarget.ToString());
        }
        return newPages;
    }
}
