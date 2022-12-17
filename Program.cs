namespace MetanitDownloader
{
    public static class Program
    {
        private static void Main(string[] args)
        {
            Console.Write("Введите ссылку на страницу с оглавлением (Пример: https://metanit.com/sharp/tutorial/): ");
            string url = Console.ReadLine()!;
            string html = MetanitConverter.ConvertToHtml(url);
            File.WriteAllBytes("output.pdf", PdfConverter.ConvertToPDF(html));
            Console.Write("Файл output.pdf сохранен!");
        }
    }
}