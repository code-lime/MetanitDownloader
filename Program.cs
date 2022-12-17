namespace MetanitDownloader
{
    public static class Program
    {
        private static void Main(string[] args)
        {
            /*string html = MetanitConverter.ConvertToHtml("https://metanit.com/java/tutorial/");
            File.WriteAllBytes("java.pdf", PdfConverter.ConvertToPDF(html));*/
            string html = MetanitConverter.ConvertToHtml("https://metanit.com/sharp/tutorial/");
            File.WriteAllBytes("sharp.pdf", PdfConverter.ConvertToPDF(html));
        }
    }
}