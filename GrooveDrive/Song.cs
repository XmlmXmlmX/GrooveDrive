using Microsoft.Graph;
using System.Text.Json.Serialization;

namespace GrooveDrive;

public class Song : Audio
{
    public Song(Audio audio, string _driveId, string _fileName, string? _thumbnailUrl)
    {
        AdditionalData = audio.AdditionalData;
        Album = audio.Album;
        AlbumArtist = audio.AlbumArtist;
        Artist = audio.Artist;
        Bitrate = audio.Bitrate;
        Composers = audio.Composers;
        Copyright = audio.Copyright;
        Disc = audio.Disc;
        DiscCount = audio.DiscCount;
        Duration = audio.Duration;
        Genre = audio.Genre;
        HasDrm = audio.HasDrm;
        IsVariableBitrate = audio.IsVariableBitrate;
        ODataType = audio.ODataType;
        Title = audio.Title;
        Track = audio.Track;
        TrackCount = audio.TrackCount;
        Year = audio.Year;
        DriveId = _driveId;
        ThumbnailUrl = _thumbnailUrl;
        FileName = _fileName;
    }

    [JsonConstructor]
    public Song() { }

    [JsonPropertyName("driveId")]
    public string DriveId { get; set; }

    [JsonPropertyName("favorite")]
    public string Favorite { get; set; }

    [JsonPropertyName("thumbnailUrl")]
    public string? ThumbnailUrl { get; set; }

    [JsonPropertyName("fileName")]
    public string FileName { get; set; }

    public string GetTitle () => string.IsNullOrWhiteSpace(this.Title) ? this.FileName : this.Title;
}
