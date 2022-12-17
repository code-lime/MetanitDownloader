using WkHtmlToPdfDotNet;
using WkHtmlToPdfDotNet.Contracts;
using WkHtmlToPdfDotNet.EventDefinitions;

namespace MetanitDownloader
{
    public static class PdfConverter
    {
        private class PageCountException : Exception
        {
            public int PageCount { get; }
            public PageCountException(int count)
            {
                PageCount = count;
            }
        }

        private static void AutoHeightSize(IConverter converter, HtmlToPdfDocument document)
        {
            PechkinPaperSize size = document.GlobalSettings.PaperSize;
            document.GlobalSettings.PaperSize = new PechkinPaperSize(size.Width, "200px");
            EventHandler<ProgressChangedArgs> handler = (s, e) =>
            {
                string desc = e.Description;
                if (desc.StartsWith("Page ")) throw new PageCountException(int.Parse(desc[(desc.IndexOf(" of ") + 4)..]));
                Console.WriteLine(desc);
            };
            converter.ProgressChanged += handler;
            try
            {
                converter.Convert(document);
            }
            catch (PageCountException pages)
            {
                document.GlobalSettings.PaperSize = new PechkinPaperSize(size.Width, $"{(pages.PageCount + 1) * 200}px");
            }
            converter.ProgressChanged -= handler;
        }

        public static byte[] ConvertToPDF(string htmlPage)
        {
            using MemoryStream stream = new MemoryStream();
            BasicConverter converter = new BasicConverter(new PdfTools());
            var doc = new HtmlToPdfDocument()
            {
                GlobalSettings = {
                    ColorMode = ColorMode.Color,
                    Orientation = Orientation.Portrait,
                    PaperSize = new PechkinPaperSize("670px", ""),
                    Margins = new MarginSettings(0,0,0,0),
                    Out = @"",
                },
                Objects = {
                    new ObjectSettings()
                    {
                        PagesCount = true,
                        HtmlContent = htmlPage,
                    },
                }
            };
            Console.WriteLine("Analyze pdf height...");
            AutoHeightSize(converter, doc);
            converter.ProgressChanged += (s,e) => Console.WriteLine(e.Description);
            Console.WriteLine("Generate pdf...");
            return converter.Convert(doc);
        }
    }
}
