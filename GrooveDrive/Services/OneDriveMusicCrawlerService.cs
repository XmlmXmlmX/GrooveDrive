using Microsoft.Graph;

namespace GrooveDrive.Services;

public class OneDriveMusicCrawlerService
{
    private readonly GraphServiceClient _graphClient;

    public OneDriveMusicCrawlerService(GraphServiceClient graphClient)
    {
        _graphClient = graphClient;
    }

    public async Task<IEnumerable<DriveItem>> CrawlAsync()
    {
        var initialRequest = _graphClient.Me.Drive.Root.ItemWithPath("Music").Search($"*.mp3").Request().Select("id,name,audio,file,thumbnails,webDavUrl").Expand("thumbnails").OrderBy("lastModifiedDateTime%20desc");
        
        return await GetDriveItemsAsync(initialRequest);
    }

    private async Task<IEnumerable<DriveItem>> GetDriveItemsAsync(IDriveItemSearchRequest request)
    {
        List<DriveItem> driveItems = new List<DriveItem>();
        
        var driveResult = await request.GetAsync();

        if (driveResult is not null)
        {
            driveItems.AddRange(driveResult.Select(r => r));

            if (driveResult.NextPageRequest is not null)
            {
                driveItems.AddRange(await GetDriveItemsAsync(driveResult.NextPageRequest));
            }
        }

        return driveItems;
    }
}
