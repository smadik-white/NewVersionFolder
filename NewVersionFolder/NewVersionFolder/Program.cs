using System.IO.Compression;
using System.Text.RegularExpressions;

string path = Directory.GetCurrentDirectory();

path = "D:\\RSKLAD";

string[] dirs = Directory.GetDirectories(path);

Regex regex = new(@"\d");

int max = 0;

foreach (string s in dirs)
{
    DirectoryInfo dir_info = new(s);

    if (regex.IsMatch(dir_info.Name))
    {
        if (int.Parse(dir_info.Name) > max)
        {
            max = int.Parse(dir_info.Name);
        }
    }
}

string copyPath = $"{path}\\{max}";
string newPath = $"{path}\\{max + 1}";
string verPath = $"{path}\\version";

string[] files = Directory.GetFiles(verPath);
string[] folders = Directory.GetDirectories(verPath);

if (files.Length > 0 || folders.Length > 0)
{
    Console.WriteLine("Создание папки для новой версии...");
    CopyDirectory(copyPath, newPath);
    Console.WriteLine("Сжатие...");
    byte i = 1;
    try
    {
        using (ZipArchive archive = ZipFile.Open($"{newPath}\\update.zip", ZipArchiveMode.Update))
        {
            if (files.Length > 0)
            {
                // Архивирование файла
                foreach (string file in files)
                {
                    Console.Write($"{i} файл из {files.Length}");
                    DirectoryInfo dirInfo = new(file);

                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        if (entry.FullName.EndsWith(dirInfo.Name))
                        {
                            entry.Delete();
                            break;
                        }
                    }

                    archive.CreateEntryFromFile(file, dirInfo.Name, CompressionLevel.SmallestSize);
                    File.Delete(file);
                    i++;
                    ClearCurrentConsoleLine();
                }
                Console.WriteLine();
            }

            if (folders.Length > 0)
            {
                i = 1;
                // Удаление элементов папки/ Создание папки
                foreach (string folder in folders)
                {
                    bool haveFolder = false;
                    DirectoryInfo dirInfo = new(folder);
                    string[] dirFiles = Directory.GetFiles(folder);
                LoopEnd:
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        if (entry.FullName.TrimEnd('/').Contains($"{dirInfo.Name}/"))
                        {
                            foreach (string file in dirFiles)
                            {
                                FileInfo fileInfo = new(file);
                                if (entry.FullName.EndsWith($"{dirInfo.Name}/{fileInfo.Name}"))
                                {
                                    entry.Delete();
                                    haveFolder = true;
                                    goto LoopEnd;
                                }
                            }
                        }
                    }
                    if (haveFolder == false)
                    {
                        archive.CreateEntry($"{dirInfo.Name}/");
                    }
                }

                // Архивирование файлов папки
                foreach (string folder in folders)
                {
                    DirectoryInfo dirInfo = new(folder);
                    string[] dirFiles = Directory.GetFiles(folder);
                    Console.Write($"{i} папка из {folders.Length}");

                    foreach (string file in dirFiles)
                    {
                        FileInfo fileInfo = new(file);
                        archive.CreateEntryFromFile(file, $"{dirInfo.Name}/{fileInfo.Name}", CompressionLevel.SmallestSize);
                    }
                    i++;
                    ClearCurrentConsoleLine();
                    dirInfo.Delete(true);
                }
                Console.WriteLine();
            }

            archive.Dispose();

            Console.WriteLine("Архивирование...");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine();
        Console.WriteLine("Ошибка!!! При повторном запуске проверьте наличие файлов и папок для новой версии");
        Console.WriteLine();
        Console.WriteLine(ex.ToString());
        DirectoryInfo dirInfo = new(newPath);
        if (dirInfo.Exists)
            dirInfo.Delete(true);
        Console.ReadLine();
    }
}
else
{
    Console.WriteLine("Нет файлов/папок для архивирования!");
    Console.ReadLine();
}

static void ClearCurrentConsoleLine()
{
    int currentLineCursor = Console.CursorTop;
    Console.SetCursorPosition(0, Console.CursorTop);
    for (int i = 0; i < Console.WindowWidth; i++)
        Console.Write("");
    Console.SetCursorPosition(0, currentLineCursor);
}

static void CopyDirectory(string sourceDir, string destinationDir, bool recursive = false)
{
    var dir = new DirectoryInfo(sourceDir);

    DirectoryInfo[] dirs = dir.GetDirectories();

    Directory.CreateDirectory(destinationDir);

    foreach (FileInfo file in dir.GetFiles())
    {
        string targetFilePath = Path.Combine(destinationDir, file.Name);
        file.CopyTo(targetFilePath);
    }

    if (recursive)
    {
        foreach (DirectoryInfo subDir in dirs)
        {
            string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
            CopyDirectory(subDir.FullName, newDestinationDir, true);
        }
    }
}