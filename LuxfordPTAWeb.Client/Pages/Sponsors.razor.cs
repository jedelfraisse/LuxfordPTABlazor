using Microsoft.AspNetCore.Components;

namespace LuxfordPTAWeb.Client.Pages;

public partial class Sponsors : ComponentBase
{
    private List<SponsorshipLevelDto> SponsorshipLevels = new()
    {
        new SponsorshipLevelDto
        {
            Name = "Magenta Sponsor",
            ColorCode = "#d5008f",
            Amount = 500,
            Description = "Receive LARGE logo/company name on a vinyl banner to be displayed at all events and field trips.",
            Benefits = new()
            {
                "Large logo on vinyl banner at all events and field trips",
                "Thank you and logo/company name on PTA website, Facebook page, emails, and flyers",
                "Opportunity to set up a table at events"
            }
        },
        new SponsorshipLevelDto
        {
            Name = "Purple Sponsor",
            ColorCode = "#6a1b9a",
            Amount = 250,
            Description = "Receive SMALL logo/company name on a vinyl banner to be displayed at all events and field trips.",
            Benefits = new()
            {
                "Small logo on vinyl banner at all events and field trips",
                "Thank you and logo/company name on PTA website, Facebook page, emails, and flyers"
            }
        },
        new SponsorshipLevelDto
        {
            Name = "Aqua Sponsor",
            ColorCode = "#0097a7",
            Amount = 100,
            Description = "Receive thank you and logo/company name on PTA website, Facebook page, emails, and event flyer.",
            Benefits = new()
            {
                "Thank you and logo/company name on PTA website, Facebook page, emails, and event flyer"
            }
        },
        new SponsorshipLevelDto
        {
            Name = "Green Sponsor",
            ColorCode = "#43a047",
            Amount = 50,
            Description = "Receive thank you and logo/company name on event flyer.",
            Benefits = new()
            {
                "Thank you and logo/company name on event flyer"
            }
        },
        new SponsorshipLevelDto
        {
            Name = "Spirit Night Sponsor",
            ColorCode = "#d5008f",
            Amount = 0,
            Description = "Host a Spirit Night and receive the same benefits as Magenta Sponsor.",
            Benefits = new()
            {
                "Large logo on vinyl banner at all events and field trips",
                "Thank you and logo/company name on PTA website, Facebook page, emails, and flyers",
                "Opportunity to set up a table at events"
            }
        }
    };

    public class SponsorshipLevelDto
    {
        public string Name { get; set; } = string.Empty;
        public string ColorCode { get; set; } = "#000";
        public int Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public List<string> Benefits { get; set; } = new();
    }
}