using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace KKSCharaDownloader
{
	class Program
	{// 信頼できないSSL証明書を「問題なし」にするメソッド
		private static bool OnRemoteCertificateValidationCallback(
		  Object sender,
		  X509Certificate certificate,
		  X509Chain chain,
		  SslPolicyErrors sslPolicyErrors)
		{
			return true;  // 「SSL証明書の使用は問題なし」と示す
		}
		static void Main(string[] args)
		{
			ServicePointManager.ServerCertificateValidationCallback = OnRemoteCertificateValidationCallback;
			var directory = Path.Combine(Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName, "OUTPUT");
			if (!Directory.Exists(directory))
			{
				Directory.CreateDirectory(directory);
			}
			if (!Directory.Exists(Path.Combine(directory, "female")))
			{
				Directory.CreateDirectory(Path.Combine(directory, "female"));
			}
			if (!Directory.Exists(Path.Combine(directory, "male")))
			{
				Directory.CreateDirectory(Path.Combine(directory, "male"));
			}
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
			var charaDic = new List<Tuple<string, string, string>>();

			using (var client = new WebClient())
			{
				client.Headers.Add("Host", "upks.illusion.jp");
				client.Headers.Add("Referer", "http://upks.illusion.jp/");
				var url = "http://upks.illusion.jp/list/chara?" + UploaderPramQuery.CreateUrlQuery(1, args[0].Equals("1"));
				var result = client.DownloadString(url).Replace("\r", "").Replace("\n", "").Replace("\t", "");
				var doc = new HtmlDocument();
				doc.LoadHtml(result);
				var maxPage = int.Parse(doc.DocumentNode.SelectNodes(@"//div[@class=""uploader__result-num""]")[0].InnerText.Replace("件", "").Trim());
				Console.WriteLine(args[0].Equals("1") ? "女キャラ取得" : "男キャラ取得");
				Console.WriteLine("一覧取得中。合計:" + maxPage + "件");
				Console.WriteLine("ページ数。合計:" + Math.Ceiling(maxPage / 100m) + "ページ");
				for (int i = 1; Math.Ceiling(maxPage / 100m) >= i; i++)
				{

					var url2 = "http://upks.illusion.jp/list/chara?" + UploaderPramQuery.CreateUrlQuery(i, args[0].Equals("1"));
					Console.WriteLine(i + " URL: " + url2);
					var result1 = client.DownloadString(url2).Replace("\r", "").Replace("\n", "").Replace("\t", "");
					var doclist = new HtmlDocument();
					doclist.LoadHtml(result1);

					foreach (var item in doclist.DocumentNode.SelectNodes(@"//div[@class=""uploader__card-chara""]"))
					{;
						var creator = item.ChildNodes[1].ChildNodes[9].InnerText.Trim();
						var imgUrl = item.ChildNodes[1].ChildNodes[3].ChildNodes[1].GetDataAttributes().Where(x=>x.Name == "data-bg").Select(x=>x.Value).FirstOrDefault();
						var imgId = item.ChildNodes[1].ChildNodes[27].ChildNodes[1].ChildNodes[1].GetAttributes().Where(x=>x.Name == "value").Select(x=>x.Value).FirstOrDefault();
						charaDic.Add(new Tuple<string, string, string>(imgUrl, imgId.PadLeft(6, '0'), creator));
					}
				}
				Console.WriteLine("一覧取得完了");
				Console.WriteLine("合計：" + charaDic.Count);

				var head = new WebHeaderCollection();
				head.Add("Host", "upks.illusion.jp");
				head.Add("Referer", "http://upks.illusion.jp/");
				Console.WriteLine("ファイル取得中");
				foreach (var item in charaDic)
				{
					var file = item.Item2 + "_" + item.Item3 + ".png";
					file = file.Replace("\"", "”").Replace("<", "＜").Replace(">", "＞").Replace("|", "｜").Replace(":", "：").Replace("*", "＊").Replace("\\", "￥").Replace("/", "／").Replace("?", "？");
					var fileName = args[0].Equals("1") ? Path.Combine(directory, "female", file) : Path.Combine(directory, "male", file);
					if (!File.Exists(fileName))
					{
						Console.WriteLine((charaDic.IndexOf(item) + 1) + " 新規：" + item.Item1);
						//client.Headers = new WebHeaderCollection();
						//client.Headers.Add("Host", "upks.illusion.jp");
						//client.Headers.Add("Referer", "http://upks.illusion.jp/");
						//client.DownloadFile(item.Item1, fileName);
						Download(item.Item1, fileName);
					}
					else
					{
						Console.WriteLine((charaDic.IndexOf(item) + 1) + " 既存：" + item.Item1);
					}
				}
				Console.WriteLine("ファイル取得完了");
			}



		}
		private static readonly HttpClient httpClient = new HttpClient();

		private static void Download(string url, string filename)
		{
			using (var request = new HttpRequestMessage(HttpMethod.Get, new Uri(url)))
			using (var response = httpClient.Send(request, HttpCompletionOption.ResponseHeadersRead))
			{
				if (response.StatusCode == HttpStatusCode.OK)
				{
					using (var content = response.Content)
					using (var stream = content.ReadAsStream())
					using (var fileStream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None))
					{
						stream.CopyTo(fileStream);
					}
				}
			}
		}
	}
}
