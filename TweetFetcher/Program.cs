using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Raven.Client.Document;
using LinqToTwitter;
using System.Threading;

namespace TweetFetcher
{
    class Program
    {
        static void Main(string[] args)
        {
            var documentStore = new DocumentStore { ConnectionStringName = "RavenDB" };

            documentStore.Initialize();

            while (true)
            {
                using (var context = new TwitterContext())
                {
                    using (var session = documentStore.OpenSession())
                    {
                        var lastTweet = session.Query<Tweet>()
                            .OrderByDescending(x => x.TwitterId)
                            .FirstOrDefault();

                        var search = context.Search.Where(x =>
                            x.Query == "appharbor"
                            && x.SinceID == (lastTweet == null ? 1 : lastTweet.TwitterId)
                            && x.Type == SearchType.Search).Single();

                        foreach (var result in search.Results)
                        {
                            var tweet = new Tweet { Text = result.Text, TwitterId = result.ID };
                            session.Store(tweet);
                        }

                        session.SaveChanges();

                        Thread.Sleep(20000);
                    }
                }
            }
        }
    }
}
