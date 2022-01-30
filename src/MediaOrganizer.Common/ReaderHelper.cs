namespace MediaOrganizer.Common;

public static class ReaderHelper
{
    public static DateTime GetDefault(string filePath)
    {
        var fileInfo = new FileInfo(filePath);
        return new List<DateTime>
        {
            fileInfo.CreationTime,
            fileInfo.LastAccessTime,
            fileInfo.LastWriteTime
        }.Min();
    }
}