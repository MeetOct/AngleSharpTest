using AngleSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AngleSharpTest
{
    class Program
    {
        private static IConfiguration config = Configuration.Default.WithDefaultLoader();
        private static string address = "https://github.com";
        static void Main(string[] args) 
        {
            Encoding.RegisterProvider(provider: CodePagesEncodingProvider.Instance);
            Console.WriteLine("Followers ");

            var followers = GetFollowers().GetAwaiter().GetResult();
            foreach (var item in followers)
            {
                UpdateUserInformation(item).GetAwaiter().GetResult();
            }
            Console.WriteLine("Following ");
            var followering = GetFollowing().GetAwaiter().GetResult();
            foreach (var item in followering)
            {
                UpdateUserInformation(item).GetAwaiter().GetResult();
            }
            Console.ReadKey();
        }

        private static async Task<long> GetFollowersCount()
        {
            var document = await BrowsingContext.New(config).OpenAsync($"{address}/MeetOct/followers");
            var cellSelector = "span.Counter";
            var cells = document.QuerySelectorAll(cellSelector);
            return Convert.ToInt64(cells.FirstOrDefault().TextContent);
        }

        private static async Task<long> GetFollowingCount()
        {
            var document = await BrowsingContext.New(config).OpenAsync($"{address}/MeetOct/following");
            var cellSelector = "span.Counter";
            var cells = document.QuerySelectorAll(cellSelector);
            return Convert.ToInt64(cells.FirstOrDefault().TextContent);
        }

        private static async Task UpdateUserInformation(string Username)
        {
            try
            {
                Console.WriteLine($"{Username} information");
                var Context = BrowsingContext.New(config);
                var document = await Context.OpenAsync($"{address}/{Username}");
                var nickNameSelector = "h1.vcard-names span.vcard-fullname";
                var NickName = document.QuerySelectorAll(nickNameSelector)?.FirstOrDefault()?.TextContent ?? Username;

                Console.WriteLine($"-{NickName}");

                var websiteSelector = "li[aria-label='Blog or website'] a";
                var URL = document.QuerySelectorAll(websiteSelector)?.FirstOrDefault()?.GetAttribute("href") ?? $"https://github.com/{ Username }";
                Console.WriteLine($"-{URL}");
                var avatarSelector = "a.u-photo";
                var avatarURL = document.QuerySelectorAll(avatarSelector)?.FirstOrDefault()?.GetAttribute("href")?.Replace("s=400", "s=150");

                Console.WriteLine($"-{avatarURL}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static async Task<IList<string>> GetFollowing()
        {
            var ret = new List<string>();
            var count = await GetFollowingCount();
            var pages = (count + 50) / 51;
            for (var i = 1; i <= pages; i++)
            {
                var document = await BrowsingContext.New(config).OpenAsync($"{address}/MeetOct/following?page={ i}");
                var cellSelector = "h3.follow-list-name span.css-truncate-target a";
                var cells = document.QuerySelectorAll(cellSelector);
                foreach (var cell in cells)
                {
                    ret.Add(cell.GetAttribute("href").TrimStart('/'));
                }
            }
            return ret;
        }

        private static async Task<IList<string>> GetFollowers()
        {
            var ret = new List<string>();
            var count = await GetFollowersCount();
            var pages = (count + 50) / 51;
            for (var i = 1; i <= pages; i++)
            {
                var document = await BrowsingContext.New(config).OpenAsync($"{address}/MeetOct/followers?page={ i}");
                var cellSelector = "h3.follow-list-name span.css-truncate-target a";
                var cells = document.QuerySelectorAll(cellSelector);
                foreach (var cell in cells)
                {
                    ret.Add(cell.TextContent);
                }
            }
            return ret;
        }
    }
}
