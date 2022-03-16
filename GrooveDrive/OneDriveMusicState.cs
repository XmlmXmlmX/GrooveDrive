using GrooveDrive.Services;
using Microsoft.Graph;

namespace GrooveDrive
{
    public class OneDriveMusicState
    {
        public IEnumerable<DriveItem> DriveItems;
        
        private readonly OneDriveMusicCrawlerService _crawlerService;

        public OneDriveMusicState(OneDriveMusicCrawlerService crawlerService)
        {
            _crawlerService = crawlerService;
        }

        public async Task LoadDataAsync()
        {
            DriveItems = await _crawlerService.CrawlAsync();
        }
    }
}
