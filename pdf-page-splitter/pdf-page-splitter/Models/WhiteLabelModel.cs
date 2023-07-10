using System.Collections.Generic;

namespace pdf_page_splitter.Models
{
    public record WhiteLabel(string SLAName, string PageType, string Id, string CountryCode);
    public record WhiteLabelPerPage(WhiteLabel WhiteLabel, int PageNumber);
    public record WhiteLabelsByDocumentId(string DocumentId, List<WhiteLabelPerPage> WhiteLabelPerPageList);
}
