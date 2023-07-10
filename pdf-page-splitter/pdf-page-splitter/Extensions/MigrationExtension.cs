using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using pdf_page_splitter.Data;
using System;

namespace pdf_page_splitter.Extensions
{
    public static class MigrationExtension
    {
        public static IHost MigrateDatabase(this IHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                using (var appContext = scope.ServiceProvider.GetRequiredService<PdfPageSplitterObjectContext>())
                {
                    try
                    {
                        appContext.Database.Migrate();
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }
                }
            }

            return host;
        }
    }
}
