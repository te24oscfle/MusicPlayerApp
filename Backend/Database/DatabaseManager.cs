using Microsoft.Data.Sqlite;
using MusicPlayerApp;

namespace Database
{
    public static class DatabaseManager
    {
        private static readonly object playlistLock = new();
        
        public static void AddTracks(List<Track> tracks)
        {
            List<FailedItem<Track>> failedTracks = DatabaseHelper.WriteFromList(
                tracks,
                """
                INSERT INTO tracks (
                    file_path,
                    album_id,
                    title,
                    artist,
                    album,
                    album_artist,
                    track_number,
                    disc_number,
                    duration_seconds,
                    date,
                    genre,
                    last_modified_utc
                )
                VALUES (
                    @file_path,
                    @album_id,
                    @title,
                    @artist,
                    @album,
                    @album_artist,
                    @track_number,
                    @disc_number,
                    @duration_seconds,
                    @date,
                    @genre,
                    @last_modified_utc
                )
                """,
                DatabaseHelper.WriteTrack
            );

            if (failedTracks.Count > 0)
            {
                Console.WriteLine($"{failedTracks.Count} track(s) failed to import:");
                foreach(FailedItem<Track> item in failedTracks)
                {
                    Console.WriteLine($"\t{item.Item.FilePath}"); // TODO: Write more robust error messaging?
                }
                Console.Write("\n");
            }

            Console.WriteLine($"Added {tracks.Count - failedTracks.Count} tracks to library");
        }

        public static List<Track> GetTracks(bool shouldRescan=true)
        {
            List<Track> tracks = DatabaseHelper.ReadToList(
                """
                SELECT * FROM tracks
                ORDER BY track_id ASC
                """,
                DatabaseHelper.ReadTrack
            );

            if (shouldRescan)
            {
                List<Track> tracksToUpdate = tracks.Where(track => track.ShouldUpdate == true).ToList();
                if (tracksToUpdate.Count > 0)
                    UpdateTracks(tracksToUpdate);
                    Console.WriteLine($"Updated {tracksToUpdate.Count} tracks to the database");
            }
            
            return tracks;
        }

        public static void UpdateTracks(List<Track> tracks)
        {
            HashSet<int> albumIds = tracks.Select(track => track.AlbumId).ToHashSet();

            List<FailedItem<Track>> failedTracks = DatabaseHelper.WriteFromList(
                tracks,
                """
                UPDATE tracks
                SET title = @title,
                    artist = @artist,
                    album = @album,
                    album_artist = @album_artist,
                    track_number = @track_number,
                    disc_number = @disc_number,
                    duration_seconds = @duration_seconds,
                    date = @date,
                    genre = @genre,
                    last_modified_utc = @last_modified_utc,
                    album_id = @album_id
                WHERE track_id = @track_id
                """,
                (command, track) =>
                {
                    TrackMetadata metadata = track.Metadata;
                    int newAlbumId = GetAlbumIdFromAlbum(track.Metadata.Album, track.Metadata.AlbumArtist);

                    if (newAlbumId == 0)
                    {
                        // This album does not exist yet
                        // Create new album
                        newAlbumId = AddAlbum(new Album(track.Metadata.Album, track.Metadata.AlbumArtist));
                    }

                    Console.WriteLine($"New Album Id: {newAlbumId}");

                    command.Parameters.AddWithValue("title", metadata.Title);
                    command.Parameters.AddWithValue("artist", metadata.Artist);
                    command.Parameters.AddWithValue("album", metadata.Album);
                    command.Parameters.AddWithValue("album_artist", metadata.AlbumArtist);
                    command.Parameters.AddWithValue("track_number", metadata.TrackNumber);
                    command.Parameters.AddWithValue("disc_number", metadata.DiscNumber);
                    command.Parameters.AddWithValue("duration_seconds", metadata.DurationSeconds);
                    command.Parameters.AddWithValue("date", metadata.Date.ToString());
                    command.Parameters.AddWithValue("genre", metadata.Genre);
                    command.Parameters.AddWithValue("last_modified_utc", metadata.LastModifiedUtc.ToString("O"));
                    command.Parameters.AddWithValue("album_id", newAlbumId);
                    command.Parameters.AddWithValue("track_id", track.TrackId);  

                    track.AlbumId = newAlbumId;
                }
            );

            if (failedTracks.Count > 0)
            {
                Console.Write("\n");
                Console.WriteLine("Following tracks failed to update:");
                foreach (FailedItem<Track> item in failedTracks)
                {
                    Console.WriteLine($"\t{item.Item.FilePath}"); // TODO: Write more robust erorr messaging?
                }
                Console.Write("\n");
            }

            if (albumIds.Count > 0)
            {
                UpdateAlbums(albumIds);
            }
        }

        public static int AddAlbum(Album album)
        {
            // Create the album
            DatabaseHelper.Write(
                """
                INSERT INTO albums (album_title, album_artist)
                VALUES (@album_title, @album_artist)
                """,
                command => // TODO: Create WriteAlbum factory function?
                {
                    command.Parameters.AddWithValue("album_title", album.Title);
                    command.Parameters.AddWithValue("album_artist", album.Artist);
                }
            );

            // Get the albumId
            int albumId = GetAlbumIdFromAlbum(album);

            // Add tracks
            foreach(var pair in album.Discs)
            {
                foreach(Track track in pair.Value)
                {
                    track.AlbumId = albumId;
                }
                AddTracks(pair.Value);
            }

            album.AlbumId = albumId;
            return albumId;
        }

        public static List<Album> GetAlbums()
        {
            return GetAlbumsFromTracks(GetTracks());
        }
        
        public static List<Album> GetAlbumsFromTracks(List<Track> tracks)
        {
            List<Album> albums = DatabaseHelper.ReadToList(
                """
                SELECT * FROM albums
                ORDER BY album_id ASC
                """,
                DatabaseHelper.ReadAlbum
            );

            foreach(Album album in albums)
            {
                List<Track> albumTracks = tracks.Where(track => track.AlbumId == album.AlbumId).ToList();
                album.AddTracks(albumTracks);
            }

            return albums;
        }

        public static void UpdateAlbums(HashSet<int> albumIds)
        {
            List<Track> tracks = GetTracks(false);
            List<Album> albums = new List<Album>();
            
            foreach (int albumId in albumIds)
            {
                Album? album = DatabaseHelper.Read(
                    """
                    SELECT * FROM albums
                    WHERE album_id = @album_id
                    """,
                    DatabaseHelper.ReadAlbum,
                    command => command.Parameters.AddWithValue("album_id", albumId)
                );
                
                if (album is not null)
                    albums.Add(album);
            }

            foreach(Album album in albums)
            {
                bool hasTracks = DatabaseHelper.Read(
                    """
                    SELECT EXISTS (
                        SELECT 1
                        FROM tracks
                        WHERE album_id = @album_id
                    )
                    """,
                    reader => reader.GetBoolean(0),
                    command => command.Parameters.AddWithValue("album_id", album.AlbumId)
                );

                if (!hasTracks)
                {
                    // No tracks in this album, we can delete it
                    DatabaseHelper.Write(
                        """
                        DELETE FROM albums
                        WHERE album_id = @album_id
                        """,
                        command => command.Parameters.AddWithValue("album_id", album.AlbumId)
                    );
                }
            }
        }

        public static List<string> GetAlbumKeys()
        {
            return DatabaseHelper.ReadToList(
                """
                SELECT album_title, album_artist FROM albums
                """,
                reader =>
                {
                    string title = reader.GetString(reader.GetOrdinal("album_title")) ?? "__unknown__";
                    string artist = reader.GetString(reader.GetOrdinal("album_artist")) ?? "__unknown__";
                    return Library.SerializeAlbumKey(title, artist);
                });            
        }

        public static int GetAlbumIdFromAlbum(Album album)
        {            
            return DatabaseHelper.Read(
                """
                SELECT album_id FROM albums
                WHERE album_title = @album_title
                AND album_artist = @album_artist
                """,
                reader =>
                    reader.IsDBNull(reader.GetOrdinal("album_id")) is false 
                        ? reader.GetInt32(reader.GetOrdinal("album_id")) 
                        : 0,
                command =>
                {
                    command.Parameters.AddWithValue("@album_title", album.Title);
                    command.Parameters.AddWithValue("@album_artist", album.Artist);
                }
            );
        }
        public static int GetAlbumIdFromAlbum(string albumTitle, string albumArtist)
        {
            return GetAlbumIdFromAlbum(new Album(albumTitle, albumArtist));
        }

        public static void AddPlaylist(Playlist playlist)
        {
            lock(playlistLock)
            {
                int? id = DatabaseHelper.WriteReturning<int>(
                    """
                    INSERT INTO playlist_metadata (title, description, image_file_path, playlist_type)
                    VALUES (@title, @description, @image_file_path, @playlist_type)
                    RETURNING playlist_id
                    """, 
                    command => DatabaseHelper.WritePlaylistMetadata(command, playlist)
                );

                if (id is not null)
                    playlist.InitilizePlaylistId(id.Value);
            }
        }

        public static List<Playlist> GetPlaylists()
        {
            List<Playlist> playlists = DatabaseHelper.ReadToList(
                """
                SELECT * FROM playlist_metadata
                """,
                DatabaseHelper.ReadPlaylist
            );

            foreach (Playlist playlist in playlists)
            {
                List<Track> tracks = DatabaseHelper.ReadToList(
                    """
                    SELECT track.*
                    FROM playlist_tracks AS playlist_entry
                    JOIN tracks AS track
                        ON track.track_id = playlist_entry.track_id
                    WHERE playlist_entry.playlist_id = @playlist_id
                    ORDER BY playlist_entry.position
                    """,
                    DatabaseHelper.ReadTrack,
                    command => command.Parameters.AddWithValue("playlist_id", playlist.PlaylistId)
                );
                
                playlist.AddTracks(tracks);
            };

            return playlists;
        }
        
        public static void DeletePlaylist(Playlist playlist)
        {
            lock(playlistLock)
            {
                bool success = DatabaseHelper.Write(
                    """
                    DELETE FROM * playlist_metadata
                    WHERE playlist_id = @playlist_id
                    """,
                    command => command.Parameters.AddWithValue("playlist_id", playlist.PlaylistId)
                );
                if(success)
                    Console.WriteLine($"Deleted Playlist {playlist.Title}");
            }   
        }

        public static void UpdatePlaylistMetadata(Playlist playlist)
        {
            lock(playlistLock)
            {
               DatabaseHelper.Write(
                    """
                    UPDATE playlist_metadata
                    SET title = @title
                        description = @description
                        image_file_path = @image_file_path
                        playlist_type = @playlist_type
                    WHERE playlist_id = @playlist_id
                    """,
                    command =>
                    {
                        DatabaseHelper.WritePlaylistMetadata(command, playlist);
                        command.Parameters.AddWithValue("playlist_id", playlist.PlaylistId);
                    }
                ); 
            }
        }

        public static void UpdatePlaylistTracks(Playlist playlist)
        {
            if (!playlist.PlaylistId.HasValue)
                throw new InvalidOperationException("The playlist must have a PlaylistId");
            
            lock(playlistLock)
            {
                DatabaseHelper.ExecuteTransaction((connection, transaction) =>
                {
                    // Delete all tracks currently saved
                    DatabaseHelper.Write(
                        """
                        DELETE FROM playlist_tracks
                        WHERE playlist_id = @playlist_id
                        """,
                        command => command.Parameters.AddWithValue("playlist_id", playlist.PlaylistId),
                        connection, 
                        transaction
                    );
                    
                    // Create playlist entries
                    List<PlaylistEntry> playlistEntries = playlist.Tracks
                        .Select((track, index) => new PlaylistEntry(
                            playlist.PlaylistId.Value,
                            index, 
                            track.TrackId
                        ))
                        .ToList();

                    // Reinsert all tracks
                    DatabaseHelper.WriteFromList(
                        playlistEntries,
                        """
                        INSERT INTO playlist_tracks (playlist_id, position, track_id)
                        VALUES (@playlist_id, @position, @track_id)
                        """,
                        DatabaseHelper.WritePlaylistEntry,
                        connection,
                        transaction
                    );
                });
            }
        }

        public static List<string> GetFilePaths()
        {
            return DatabaseHelper.ReadToList(
                """
                SELECT file_path FROM tracks
                """,
                reader => 
                    reader.GetString(reader.GetOrdinal("file_path"))
            );
        }
    }    
}