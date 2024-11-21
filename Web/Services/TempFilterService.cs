using CodeLab.Share.Contrib.StopWords;

namespace Web.Services;

public class TempFilterService
{
    public StopWordsToolkit Toolkit { get; }

    public bool CheckBadWord(string word)
    {
        return Toolkit.CheckBadWord(word);
    }
}