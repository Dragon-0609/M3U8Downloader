using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace M3U8Downloader
{
	public class WebDownloader
	{
		public async Task Download (string url, string name, int count)
		{
            /*using (WebClient client = new WebClient())
			{
				string url = "http://tools.eti.pw/proxy/";
				client.Encoding = Encoding.UTF8;

				//var data = "[\"GLD24449\"]";
				var data = UTF8Encoding.UTF8.GetBytes(TblHeader.Rows[0]["id"].ToString());
				byte[] r = client.UploadData(url, data);
				using (var stream = System.IO.File.Create("FilePath"))
				{
					stream.Write(r,0,r.length);
				}
			}*/
            string filename = $"{name}_segment_{count}.ts";
            if (MainClass.useProxy)
            {

                HttpClient client = new HttpClient();
                var values = new Dictionary<string, string>
            {
                { "__proxy_action", "redirect_browse" },
                { "url", url }
            };

                FormUrlEncodedContent content = new FormUrlEncodedContent(values);

                HttpResponseMessage response = await client.PostAsync("http://tools.eti.pw/proxy/", content);

                using (var fileStream = File.Create(filename))
                {
                    await response.Content.CopyToAsync(fileStream);
                }
            }
            else
            {
                WebClient client = new WebClient();
                await client.DownloadFileTaskAsync(new Uri(url), filename);
            }

            // Console.WriteLine ("Downloaded");

        }
	}
}