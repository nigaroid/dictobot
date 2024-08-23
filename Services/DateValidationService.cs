using System.Globalization;

namespace Dictobot.Services;
public static class DateValidationService
{
    public static bool ValidateDate(string dateString) => DateTime.TryParseExact(dateString, "yyyy-MM-dd",
                                                                                 CultureInfo.InvariantCulture,
                                                                                 DateTimeStyles.None, out DateTime date) &&
                                                          date.CompareTo(DateTime.UtcNow) <= 0 &&
                                                          date.Year == DateTime.UtcNow.Year;
}
