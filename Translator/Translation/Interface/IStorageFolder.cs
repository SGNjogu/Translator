namespace Translation.Interface
{
    public interface IStorageFolder
    {
        string GetDocumentsPath();

        string GetVideosPath();

        string GetMusicPath();

        string GetDownloadsPath();

        string GetPicturesPath();
    }
}
