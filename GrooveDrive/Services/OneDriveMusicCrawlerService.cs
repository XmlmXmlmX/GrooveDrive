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
        var filesRequest = GetSearchRequest(_graphClient.Me.Drive.Root.ItemWithPath("Music"));

        List<DriveItem> driveItems = await GetDriveItemsAsync(filesRequest);

        //var folderRequest = _graphClient.Me.Drive.Root.ItemWithPath("Music").Children.Request().Select("id,name,folder,thumbnails,webDavUrl").Expand("thumbnails").OrderBy("lastModifiedDateTime%20desc");
        var foldersRequest = _graphClient.Me.Drive.Root.Children.Request();

        //var folders = await folderRequest.GetAsync();

        driveItems.AddRange(await IterateFolders(foldersRequest));

        return driveItems;
    }

    private static IDriveItemSearchRequest GetSearchRequest(IDriveItemRequestBuilder builder)
    {
        return builder.Search($"*.mp3").Request().Select("id,name,audio,file,thumbnails,webDavUrl").Expand("thumbnails").OrderBy("lastModifiedDateTime%20desc");
    }

    private async Task<IEnumerable<DriveItem>> IterateFolders(IDriveItemChildrenCollectionRequest request)
    {
        List<DriveItem> driveItems = new List<DriveItem>();
        var folders = await request.GetAsync();

        foreach (var folder in folders)
        {
            var childItems = GetSearchRequest(_graphClient.Me.Drive.Items[folder.Id]);
            driveItems.AddRange(await GetDriveItemsAsync(childItems));

            //driveItems.AddRange(await IterateFolders());
        }

        return driveItems;
    }

    private async Task<List<DriveItem>> GetDriveItemsAsync(IDriveItemSearchRequest request)
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
