using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Dsw2025Tpi.Domain.Entities;

namespace Dsw2025Tpi.Data.Helpers
{
    public static class DbContextExtensions
    {
        private static readonly JsonSerializerOptions CachedJsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
        };

        public static void SeedDatabase(this Dsw2025TpiContext context)
        {

            if (!context.Customers.Any())
            {
                var customersJson = File.ReadAllText(
                    Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Dsw2025Tpi.Data", "Sources", "customers.json"));
                var customers = JsonSerializer.Deserialize<List<Customer>>(customersJson, CachedJsonOptions);
                if (customers != null && customers.Count > 0)
                {
                    context.Customers.AddRange(customers);
                    context.SaveChanges();
                }
            }

        }
    }
}
