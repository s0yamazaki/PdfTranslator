using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Patagames.Pdf;
using Patagames.Pdf.Net;
using static System.Console;
using System.IO;
using System.Net;
using PDF = iTextSharp.text.pdf;
using iTextSharp.text;

namespace PdfTranslator
{
    public static class Util
    {
        const string url = "https://script.google.com/macros/s/AKfycbwbcsLhwdWbGpzqx_jdl3XE1mzuCCzy3o1IyM3CjCFCA-13F0E/exec";
        static IEnumerable<string> Combine(IEnumerable<string> src, int minLength, int maxLength)
        {
            var ret = "";
            foreach(var str in src)
            {
                var buf = ret + "\n" + str;
                if (buf.Length >= maxLength && ret.Length >= minLength)
                {
                    yield return ret;
                    buf = str;
                }
                ret = buf;
            }
            yield return ret;
        }
        public static async Task<(string,string)[][]> Translate(PdfDocument pdf)
        {
            var pagetasks = pdf.Pages.Select(async page =>
            {
                var srctext = page.Text;
                
                var str = srctext.GetText(0, srctext.CountChars);
                var strs = str.Replace("\r\n", " ").Replace(". ", ".\r\n").Split('\n');
                var tasks = Combine(strs, 100, 300).Select(async src =>
                {
                    var get_url = url + string.Format("?text={0}&source=en&target=ja", Uri.EscapeDataString(src));
                    WriteLine(get_url);
                    WriteLine();
                    var req = WebRequest.Create(get_url);
                    var res = await req.GetResponseAsync();
                    var translated = await new StreamReader(res.GetResponseStream()).ReadToEndAsync();
                    return (src, translated);
                });

                return await Task.WhenAll(tasks);
            });

            return await Task.WhenAll(pagetasks);
        }

        private static void Output(PdfDocument pdf, PDF.PdfReader template, string newPdf, (string,string)[][] texts)
        {

            // テンプレの1ページ目のページサイズを取得
            var size = template.GetPageSize(1);
            // 開いたファイルのサイズでドキュメントを作成
            using (var document = new Document(size))
            using (var fs = new FileStream(newPdf, FileMode.Create, FileAccess.Write))
            using (var writer = PDF.PdfWriter.GetInstance(document, fs))
            {
                writer.Open();
                document.Open();
                foreach (var (pageTrans, page) in texts.Zip(pdf.Pages, (pageTrans, page) => (pageTrans, page)))
                {
                    var pdfContentByte = writer.DirectContent;
                    var page1 = writer.GetImportedPage(template, page.PageIndex + 1);
                    pdfContentByte.AddTemplate(page1, 0, 0);
                    var count = 0;
                    foreach (var (src, translated) in pageTrans)
                    {
                        var info = page.Text.GetTextInfo(count, src.Length);
                        count += src.Length;
                        var rect = info.Rects.Aggregate((l, r) => new FS_RECTF(
                            Math.Min(l.left, r.left),
                            Math.Max(l.top, r.top),
                            Math.Max(l.right, r.right),
                            Math.Min(l.bottom, r.bottom)));
                        
                        var ra = new Rectangle(rect.left, rect.bottom, rect.right, rect.top);

                        WriteLine(ra);
                        ra.Left -= 20;
                        var a = PDF.PdfAnnotation.CreateText(
                                    writer,
                                    ra,
                                    "訳文",
                                    translated,
                                    false,
                                    "訳文");
                        writer.AddAnnotation(a);
                    }
                    document.NewPage();
                }
                document.Close();
                writer.Close();
                WriteLine("end...");
            }
        }

        static public async Task Convert(string inputPdfPath, string outputPdfPath)
        {
            using (var pdf = PdfDocument.Load(inputPdfPath))
            using(var reader = new PDF.PdfReader(inputPdfPath))
            {
                var texts = await Translate(pdf);
                Output(pdf, reader, outputPdfPath, texts);
            }
        }
    }
}
