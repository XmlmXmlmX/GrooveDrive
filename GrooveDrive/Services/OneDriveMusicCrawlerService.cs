using Microsoft.Graph;

namespace GrooveDrive.Services;

public class OneDriveMusicCrawlerService
{
    private readonly GraphServiceClient _graphClient;

    public OneDriveMusicCrawlerService(GraphServiceClient graphClient)
    {
        _graphClient = graphClient;
    }

    public async Task<IEnumerable<Song>> CrawlAsync(string startPath = "Music")
    {
        var filesRequest = GetSearchRequest(_graphClient.Me.Drive.Root.ItemWithPath(startPath));

        List<Song> songs = await GetDriveItemsAsync(filesRequest);
        var foldersRequest = _graphClient.Me.Drive.Root.Children.Request();

        songs.AddRange(await IterateFolders(foldersRequest));

        foreach (var song in songs)
        {
            if (song.Artist is null)
            {
                song.Artist = "Unbekannter Interpret";
            }
        }

        return songs;
    }

    private static IDriveItemSearchRequest GetSearchRequest(IDriveItemRequestBuilder builder)
    {
        return builder.Search($"*.mp3").Request().Filter("audio ne null").Select("id,name,audio,file,thumbnails,webDavUrl").Expand("thumbnails").OrderBy("lastModifiedDateTime%20desc");
    }

    private async Task<IEnumerable<Song>> IterateFolders(IDriveItemChildrenCollectionRequest request)
    {
        List<Song> songs = new List<Song>();
        var folders = await request.GetAsync();

        foreach (var folder in folders)
        {
            var childItems = GetSearchRequest(_graphClient.Me.Drive.Items[folder.Id]);

            songs.AddRange(await GetDriveItemsAsync(childItems));
        }

        return songs;
    }

    private async Task<List<Song>> GetDriveItemsAsync(IDriveItemSearchRequest request)
    {
        List<Song> songs = new List<Song>();
        
        var driveResult = await request.GetAsync();

        if (driveResult is not null)
        {
            songs
                .AddRange(driveResult.Where(d => d.Audio is not null)
                .Select(r => new Song(r.Audio, r.Id, r.Name, r.Thumbnails?.CurrentPage?.Count > 0 ? r.Thumbnails.CurrentPage[0].Large?.Url : null)));

            if (driveResult.NextPageRequest is not null)
            {
                songs.AddRange(await GetDriveItemsAsync(driveResult.NextPageRequest));
            }
        }

        return songs;
    }
}
