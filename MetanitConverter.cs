using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using AngleSharp;
using AngleSharp.Dom;

namespace MetanitDownloader
{
    public static class MetanitConverter
    {
        private static IConfiguration Config { get; } = Configuration.Default;
        private static IBrowsingContext Context { get; } = BrowsingContext.New(Config);
        private static IHtmlParser Parser { get; } = Context.GetService<IHtmlParser>()!;
        private const string BLANK_METANIT_KEY = "<!--INSERT BLANK-->";
        private const string CSS_METANIT_KEY = "<!--INSERT CSS-->";
        private static readonly string BLANK_METANIT_FORMATTED = File.ReadAllText("blank_metanit.html");
        private static string LAST_HTML = "";
        private static IHtmlDocument ParseHtmlDownload(string url)
        {
            using HttpClientHandler handler = new HttpClientHandler()
            {
                UseDefaultCredentials = true
            };
            using HttpClient client = new HttpClient(handler);
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/105.0.0.0 Safari/537.36 Edg/105.0.1343.53");
            Console.Write("Donwload page: " + url + "...");
            try
            {
                LAST_HTML = client.GetStringAsync(url).Result;
                return ParseHtml(LAST_HTML);
            }
            finally
            {
                Console.WriteLine("OK!");
            }
        }
        private static IHtmlDocument ParseHtml(string html) => Parser.ParseDocument(html);
        private static byte[] DownloadBytes(string url)
        {
            using HttpClientHandler handler = new HttpClientHandler()
            {
                UseDefaultCredentials = true
            };
            using HttpClient client = new HttpClient(handler);
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/105.0.0.0 Safari/537.36 Edg/105.0.1343.53");
            Console.Write("Donwload url: " + url + "...");
            try
            {
                return client.GetByteArrayAsync(url).Result;
            }
            finally
            {
                Console.WriteLine("OK!");
            }
        }

        private static string CreateHtmlByBlanks(List<IElement> contents) => BLANK_METANIT_FORMATTED.Replace(BLANK_METANIT_KEY, string.Join("\n", contents.Select(v => v.OuterHtml)));

        public static string ConvertToHtml(string tutorial_url)
        {
            IHtmlDocument desc = ParseHtmlDownload(tutorial_url);
            List<string> hrefs = desc.GetElementsByClassName("content").First().GetElementsByTagName("a")
                .Select(v => v.GetAttribute("href"))
                .Where(v => v != null)
                .Select(v => v!)
                .Distinct()
                .ToList();

            Dictionary<string, string> pages = new Dictionary<string, string>();
            Dictionary<string, byte[]> images = new Dictionary<string, byte[]>();

            string css = string.Join("\n", Directory.GetFiles("css")
                .Select(v => File.ReadAllText(v))
                .Select(v => $"<style>{v}</style>"));

            List<IElement> elements = new List<IElement>();
            foreach (string href in hrefs)
            {
                try
                {
                    IHtmlDocument page = ParseHtmlDownload(tutorial_url + href);
                    IElement pageContent = page.GetElementsByClassName("outercontainer").First();
                    foreach (IElement e in pageContent.GetElementsByTagName("div")
                        .Where(v => v.ClassName == "socBlock" || v.ClassName == "item left" || v.ClassName == "item right" || v.ClassName == "nav" || v.ClassName == "commentABl" || (v.Id?.StartsWith("disqus_") ?? false))
                    ) e.Remove();
                    page.GetElementById("jma")?.Remove();
                    foreach (IElement e in pageContent.GetElementsByTagName("script")) e.Remove();
                    foreach (IElement e in pageContent.GetElementsByTagName("img"))
                    {
                        try
                        {
                            string? src_image = e.GetAttribute("src");
                            if (src_image == null)
                                continue;
                            if (src_image.StartsWith("./"))
                            {
                                string img_path = src_image[2..];
                                if (!images.ContainsKey(img_path))
                                {
                                    string img_url = tutorial_url + img_path;
                                    images[img_path] = DownloadBytes(img_url);
                                }

                                byte[] bytes = images[img_path];
                                e.SetAttribute("src", $"data:image/{Path.GetExtension(img_path)[1..]};base64, {Convert.ToBase64String(bytes)}");
                            }
                            else
                            {
                                string file_name = "other/" + Path.GetFileName(src_image);
                                if (!images.ContainsKey(file_name)) images[file_name] = DownloadBytes(src_image);
                                e.SetAttribute("src", $"data:image/{Path.GetExtension(file_name)[1..]};base64, {Convert.ToBase64String(images[file_name])}");
                            }
                        }
                        catch (Exception _e)
                        {
                            Console.WriteLine(_e.ToString());
                        }
                    }
                    elements.Add(pageContent);
                }
                catch (Exception _e)
                {
                    Console.WriteLine(_e.ToString());
                }
            }
            return CreateHtmlByBlanks(elements)
                 .Replace(tutorial_url, "./")
                 .Replace(CSS_METANIT_KEY, css)
                 .Replace(".php", ".html");
        }
    }
}
