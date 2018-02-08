using System;
using Haverim.Models;
using Microsoft.EntityFrameworkCore;

namespace Haverim.Tests
{
    public static class Global
    {
        // inited detemines if OptionsBuilder was initialized or not
        private static bool inited = false;
        // If OptionsBuilder was not initialized, it will initialize it and then return it
        // else, it will just return OptionsBuilder
        static DbContextOptions<HaverimContext> HiddenOptions;
        public static DbContextOptions<HaverimContext> ContextOptions
        {
            get
            {
                if (inited)
                {
                    return HiddenOptions;
                }
                var HiddenOptionsBuilder = new DbContextOptionsBuilder<HaverimContext>();
                var connection = @"Server=(localdb)\mssqllocaldb;Database=HaverimProjectMock;Trusted_Connection=True;ConnectRetryCount=0";
                HiddenOptionsBuilder.EnableSensitiveDataLogging();
                HiddenOptionsBuilder
                        .UseSqlServer(connection, providerOptions => providerOptions.CommandTimeout(60))
                        .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                inited = true;
                HiddenOptions = HiddenOptionsBuilder.Options;
                return HiddenOptionsBuilder.Options;
            }
            private set
            {
                ContextOptions = value;
            }
        }

        public static void ResetDatabase(HaverimContext db)
        {
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
        }
    }
}