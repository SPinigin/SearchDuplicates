using System.Security.Cryptography;

class Program
{
    static void Main(string[] args)
    {
        string directoryPath = @"D:\";
        string outputFilePath = @"D:\Output\duplicates.txt";

        try
        {
            var files = Directory.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories);
            var fileHashes = new Dictionary<string, List<string>>();

            foreach (var file in files)
            {
                try
                {
                    string fileHash = GetFileHash(file);
                    if (!fileHashes.ContainsKey(fileHash))
                    {
                        fileHashes[fileHash] = new List<string>();
                    }
                    fileHashes[fileHash].Add(file);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при обработке файла {file}: {ex.Message}");
                }
            }

            using (var writer = new StreamWriter(outputFilePath))
            {
                foreach (var hash in fileHashes.Where(h => h.Value.Count > 1))
                {
                    writer.WriteLine("Дубликаты:");
                    foreach (var filePath in hash.Value)
                    {
                        writer.WriteLine(filePath);
                    }
                    writer.WriteLine();
                }
            }

            Console.WriteLine("Поиск завершен. Результаты сохранены в " + outputFilePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ошибка: " + ex.Message);
        }
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
