using GrooveDrive.Services;
using GrooveDrive.Enums;
using Microsoft.Graph;

namespace GrooveDrive;

public class PlayerState
{
    private readonly OneDriveMusicCrawlerService _crawlerService;
    public DriveItem? currentSong;

    public event Action? OnChange;
    public event Func<Task>? OnStreamChange;

    public PlayerState(OneDriveMusicCrawlerService crawlerService)
    {
        _crawlerService = crawlerService;
    }

    public List<DriveItem> AllMusic;

    public Dictionary<string, DriveItem>? Artists => AllMusic?.Where(i => i.Audio?.Artist is not null).DistinctBy(i => i.Audio.Artist, StringComparer.OrdinalIgnoreCase).ToDictionary(i => i.Audio.Artist);

    public Dictionary<string, DriveItem>? Albums => AllMusic?.Where(i => i.Audio?.Album is not null).DistinctBy(i => i.Audio.Album, StringComparer.OrdinalIgnoreCase).ToDictionary(i => i.Audio.Album);

    public List<DriveItem>? SongsByArtistName(string artistName) => AllMusic?.Where(i => i.Audio?.Artist == artistName || i.Audio?.AlbumArtist == artistName).ToList();

    public List<DriveItem>? SongsByAlbumName(string albumName) => AllMusic?.Where(i => i.Audio?.Album == albumName).ToList();

    public PlayerStateType CurrentPlayState { get; private set; } = PlayerStateType.Stop;

    public bool Repeat { get; private set; } = true;

    public bool IsLoading => AllMusic is null;

    public bool Random { get; private set; } = true;

    public string? SearchQuery { get; set; }

    public DriveItem? CurrentSong
    {
        get
        {
            return currentSong;
        }
        private set
        {
            currentSong = value;
        }
    }

    public void ToggleRandom()
    {
        Random = !Random;
        NotifyStateChanged();
    }

    public void ToggleRepeat()
    {
        Repeat = !Repeat;
        NotifyStateChanged();
    }

    public async Task LoadDataAsync()
    {
        var items = await _crawlerService.CrawlAsync();
        AllMusic = items.OrderBy(i => i.Audio?.Artist).ToList();
        NotifyStateChanged();
    }

    public void PreviousTrack()
    {
        if (AllMusic is not null && AllMusic.Count > 0 && CurrentSong is not null)
        {
            var indexOf = AllMusic.IndexOf(CurrentSong);
            CurrentSong = AllMusic[indexOf == AllMusic.Count - 1 ? 0 : indexOf + 1];
            NotifyStreamHasChanged();
            NotifyStateChanged();
        }
    }

    public void NextTrack()
    {
        if (AllMusic is not null && AllMusic.Count > 0)
        {
            if (Random || CurrentSong is null)
            {
                NextRandom();
            }
            else
            {
                var indexOf = AllMusic.IndexOf(CurrentSong);
                CurrentSong = AllMusic[indexOf == 0 ? AllMusic.Count - 1 : indexOf - 1];
            }

            NotifyStreamHasChanged();
            NotifyStateChanged();
        }
    }

    public void Play()
    {
        if (AllMusic is not null && AllMusic.Count > 0)
        {
            if (Equals(CurrentPlayState, PlayerStateType.Stop))
            {
                if (Random)
                {
                    NextRandom();
                }
            }

            CurrentPlayState = PlayerStateType.Play;
            NotifyStateChanged();
        }
    }

    public void Play(DriveItem driveItem)
    {
        if (driveItem is not null && AllMusic.Contains(driveItem))
        {
            CurrentSong = driveItem;
            NotifyStreamHasChanged();
            CurrentPlayState = PlayerStateType.Play;
            NotifyStateChanged();
        }
    }

    public void Pause()
    {
        CurrentPlayState = PlayerStateType.Pause;
        NotifyStateChanged();
    }

    private void NotifyStateChanged()
    {
        OnChange?.Invoke();
    }

    private void NotifyStreamHasChanged()
    {
        OnStreamChange?.Invoke();
    }

    private void NextRandom()
    {
        var random = new Random();
        int songIndex = random.Next(0, AllMusic.Count);

        CurrentSong = AllMusic[songIndex];
    }
}
