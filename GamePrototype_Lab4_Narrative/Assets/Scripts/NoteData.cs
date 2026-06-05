[System.Serializable]
public class NoteData
{
    public string title;
    public string content;

    public NoteData(string title, string content)
    {
        this.title = title;
        this.content = content;
    }
}
