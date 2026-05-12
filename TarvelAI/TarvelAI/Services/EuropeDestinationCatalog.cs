namespace TarvelAI.Services;

public static class EuropeDestinationCatalog
{
    // Curated static filter options for Explore (Europe-first UX).
    public static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> CountriesToCities =
        new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["Austria"] = ["Vienna", "Salzburg", "Innsbruck"],
            ["Belgium"] = ["Brussels", "Bruges", "Antwerp"],
            ["Croatia"] = ["Zagreb", "Dubrovnik", "Split"],
            ["Czech Republic"] = ["Prague", "Brno", "Cesky Krumlov"],
            ["Denmark"] = ["Copenhagen", "Aarhus", "Odense"],
            ["Finland"] = ["Helsinki", "Turku", "Tampere"],
            ["France"] = ["Paris", "Nice", "Lyon", "Marseille"],
            ["Germany"] = ["Berlin", "Munich", "Hamburg", "Cologne"],
            ["Greece"] = ["Athens", "Thessaloniki", "Heraklion"],
            ["Hungary"] = ["Budapest", "Debrecen", "Szeged"],
            ["Ireland"] = ["Dublin", "Cork", "Galway"],
            ["Italy"] = ["Rome", "Milan", "Venice", "Florence", "Naples"],
            ["Netherlands"] = ["Amsterdam", "Rotterdam", "The Hague"],
            ["Norway"] = ["Oslo", "Bergen", "Trondheim"],
            ["Poland"] = ["Warsaw", "Krakow", "Gdansk"],
            ["Portugal"] = ["Lisbon", "Porto", "Faro"],
            ["Romania"] = ["Bucharest", "Cluj-Napoca", "Brasov"],
            ["Spain"] = ["Madrid", "Barcelona", "Valencia", "Seville"],
            ["Sweden"] = ["Stockholm", "Gothenburg", "Malmo"],
            ["Switzerland"] = ["Zurich", "Geneva", "Lucerne"],
            ["United Kingdom"] = ["London", "Manchester", "Edinburgh", "Birmingham"]
        };

    public static IReadOnlyList<string> Countries =>
        CountriesToCities.Keys.OrderBy(c => c).ToList();

    public static IReadOnlyList<string> GetCities(string? country)
    {
        if (string.IsNullOrWhiteSpace(country)) return [];
        return CountriesToCities.TryGetValue(country, out var cities)
            ? cities.OrderBy(c => c).ToList()
            : [];
    }
}
