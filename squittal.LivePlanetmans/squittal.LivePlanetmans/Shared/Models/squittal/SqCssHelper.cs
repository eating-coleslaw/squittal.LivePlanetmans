namespace squittal.LivePlanetmans.Shared.Models
{
    public static class SqCssHelper
    {
        public static string GetFactionClassFromId(int? factionId)
        {
            string cssClass;

            switch (factionId)
            {
                case 1:
                    cssClass = "vs";
                    break;

                case 2:
                    cssClass = "nc";
                    break;

                case 3:
                    cssClass = "tr";
                    break;

                case 4:
                    cssClass = "ns";
                    break;

                default:
                    cssClass = "ns";
                    break;
            }

            return cssClass;
        }

        public static string GetZoneDisplayEmojiFromName(string zoneName)
        {
            switch (zoneName)
            {
                case "Amerish":
                    return "🗻";

                case "Esamir":
                    return "❄️";

                case "Hossin":
                    return "🌳";

                case "Indar":
                    return "☀️";

                default:
                    return "❔";
            }
        }
    }
}
