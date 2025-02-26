using System.Security.Cryptography;

class Program
{
    static void Main(string[] args)
    {
        string directoryPath = @"D:\";
        string outputFilePath = @"D:\Output\duplicates.txt";

        try
        {
            string outputDirectory = Path.GetDirectoryName(outputFilePath);
            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            var fileHashes = new Dictionary<string, List<string>>();

            using (var writer = new StreamWriter(outputFilePath, false)) // false указывает на перезапись файла
            {
                ProcessDirectory(directoryPath, fileHashes, writer);
            }

            Console.WriteLine("Поиск завершен. Результаты сохранены в " + outputFilePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ошибка: " + ex.Message);
        }
    }

    static void ProcessDirectory(string path, Dictionary<string, List<string>> fileHashes, StreamWriter writer)
    {
        try
        {
            if (!IsRootDirectory(path) && IsSystemOrHiddenDirectory(path))
            {
                Console.WriteLine($"Пропуск системной или скрытой директории: {path}");
                return;
            }

            var files = Directory.GetFiles(path);
            foreach (var file in files)
            {
                Console.WriteLine($"Обработка файла: {file}");
                try
                {
                    string fileHash = GetFileHash(file);
                    if (!fileHashes.ContainsKey(fileHash))
                    {
                        fileHashes[fileHash] = new List<string>();
                    }
                    fileHashes[fileHash].Add(file);

                    if (fileHashes[fileHash].Count == 2) // Если найден первый дубликат
                    {
                        writer.WriteLine("Дубликаты:");
                        writer.WriteLine(fileHashes[fileHash][0]); // Записываем первый файл
                    }

                    if (fileHashes[fileHash].Count > 1) // Записываем все дубликаты
                    {
                        writer.WriteLine(file);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при обработке файла {file}: {ex.Message}");
                }
            }

            var directories = Directory.GetDirectories(path);
            foreach (var directory in directories)
            {
                ProcessDirectory(directory, fileHashes, writer);
            }
        }
        catch (UnauthorizedAccessException)
        {
            Console.WriteLine($"Нет доступа к директории: {path}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при обработке директории {path}: {ex.Message}");
        }
    }

    static bool IsRootDirectory(string path)
    {
        return Path.GetPathRoot(path) == path;
    }

    static bool IsSystemOrHiddenDirectory(string path)
    {
        var attributes = File.GetAttributes(path);
        return (attributes & FileAttributes.System) == FileAttributes.System ||
               (attributes & FileAttributes.Hidden) == FileAttributes.Hidden;
    }

    static string GetFileHash(string filePath)
    {
        using (var sha256 = SHA256.Create())
        {
            using (var stream = File.OpenRead(filePath))
            {
                byte[] hashBytes = sha256.ComputeHash(stream);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            }
        }
    }
}
