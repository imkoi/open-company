using System.IO;

namespace OpenCompany.Shared;

public static class FileUtility
{
    public static void CopyFiles(
        string sourceDirectory,
        string targetDirectory,
        string fileExtension,
        SearchOption searchOption)
    {
        if (!Directory.Exists(targetDirectory))
        {
            Directory.CreateDirectory(targetDirectory);
        }
        else
        {
            Directory.Delete(targetDirectory, true);
            Directory.CreateDirectory(targetDirectory);
        }

        var files = Directory.GetFiles(sourceDirectory, "*." + fileExtension, searchOption);

        foreach (var file in files)
        {
            var name = Path.GetFileName(file);
            var dest = Path.Combine(targetDirectory, name);
            
            File.Copy(file, dest);
        }
    }
}