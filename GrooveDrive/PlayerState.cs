using GrooveDrive.Enums;

namespace GrooveDrive;

public class PlayerState
{
    public Song? currentSong;

    public event Action? OnChange;
    public event Func<Task>? OnSongsStateChanged;
    public event Func<Task>? OnStreamChange;

    public List<Song>? AllSongs = null;

    public Dictionary<string, Song>? Artists => AllSongs?.Where(i => i.Artist is not null).DistinctBy(i => i.Artist, StringComparer.OrdinalIgnoreCase).ToDictionary(i => i.Artist);

    public Dictionary<string, Song>? Albums => AllSongs?.Where(i => i.Album is not null).DistinctBy(i => i.Album, StringComparer.OrdinalIgnoreCase).ToDictionary(i => i.Album);

    public List<Song>? SongsByArtistName(string artistName) => AllSongs?.Where(i => i.Artist == artistName || i.AlbumArtist == artistName).ToList();

    public List<Song>? SongsByAlbumName(string albumName) => AllSongs?.Where(i => i.Album == albumName).ToList();

    public PlayerStateType CurrentPlayState { get; private set; } = PlayerStateType.Stop;

    public bool Repeat { get; private set; } = true;

    public bool IsLoading => AllSongs is null;

    public bool Random { get; private set; } = true;

    public string? SearchQuery { get; set; }

    public Song? CurrentSong
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

    public void SetSongs(IEnumerable<Song> songs)
    {
        AllSongs = songs.ToList();
        NotifyStateChanged();
        NotifySongsStateChanged();
    }

    public void AddSongs(IEnumerable<Song> songs)
    {
        if (AllSongs is null)
        {
            AllSongs = new();
        }

        AllSongs.AddRange(songs);
        NotifyStateChanged();
        NotifySongsStateChanged();
    }

    public void PreviousTrack()
    {
        if (AllSongs is not null && AllSongs.Count > 0 && CurrentSong is not null)
        {
            var indexOf = AllSongs.IndexOf(CurrentSong);
            CurrentSong = AllSongs[indexOf == AllSongs.Count - 1 ? 0 : indexOf + 1];
            NotifyStreamHasChanged();
            NotifyStateChanged();
        }
    }

    public void NextTrack()
    {
        if (AllSongs is not null && AllSongs.Count > 0)
        {
            if (Random || CurrentSong is null)
            {
                NextRandom();
            }
            else
            {
                var indexOf = AllSongs.IndexOf(CurrentSong);
                CurrentSong = AllSongs[indexOf == 0 ? AllSongs.Count - 1 : indexOf - 1];
            }

            NotifyStreamHasChanged();
            NotifyStateChanged();
        }
    }

    public void Play()
    {
        if (AllSongs is not null && AllSongs.Count > 0)
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

    public void Play(Song song)
    {
        if (song is not null && AllSongs is not null && AllSongs.Contains(song))
        {
            CurrentSong = song;
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

    private void NotifySongsStateChanged()
    {
        OnSongsStateChanged?.Invoke();
    }

    private void NotifyStreamHasChanged()
    {
        OnStreamChange?.Invoke();
    }

    private void NextRandom()
    {
        if (AllSongs is not null)
        {
            var random = new Random();
            int songIndex = random.Next(0, AllSongs.Count);

            CurrentSong = AllSongs[songIndex];
        }
    }
}
