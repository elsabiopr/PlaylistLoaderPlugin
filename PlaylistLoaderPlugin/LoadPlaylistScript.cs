﻿using System.IO;
using System;
using Newtonsoft.Json.Linq;
using SongCore;
using System.Collections.Generic;
using System.Linq;

namespace PlaylistLoaderLite
{
    public class LoadPlaylistScript
    {
        public static CustomPlaylistSO[] load()
        {
            string[] playlistPaths = Directory.EnumerateFiles(Path.Combine(Environment.CurrentDirectory, "Playlists"), "*.*").Where(p => p.EndsWith(".json") || p.EndsWith(".bplist")).ToArray();
            List<CustomPlaylistSO> playlists = new List<CustomPlaylistSO>();
            for (int i = 0; i < playlistPaths.Length; i++)
            {
                try
                {
                    JObject playlistJSON = JObject.Parse(File.ReadAllText(playlistPaths[i]));
                    if (playlistJSON["songs"] != null)
                    {
                        JArray songs = (JArray)playlistJSON["songs"];
                        List<IPreviewBeatmapLevel> beatmapLevels = new List<IPreviewBeatmapLevel>();
                        for (int j = 0; j < songs.Count; j++)
                        {
                            IPreviewBeatmapLevel beatmapLevel = null;
                            String hash = (string)songs[j]["hash"];
                            if (!string.IsNullOrEmpty(hash))
                                beatmapLevel = MatchSong(hash);
                            if (beatmapLevel != null)
                                beatmapLevels.Add(beatmapLevel);
                            else
                            {
                                String levelID = (string)(songs[j]["levelId"] ?? songs[j]["levelid"]);
                                if (!string.IsNullOrEmpty(levelID))
                                {
                                    beatmapLevel = MatchSongById(levelID);
                                    if (beatmapLevel != null)
                                        beatmapLevels.Add(beatmapLevel);
                                    else
                                        Plugin.Log.Warn($"Song not downloaded, : {(string.IsNullOrEmpty(levelID) ? " unknown levelID!" : ("levelID " + levelID + "!"))}");
                                }
                                else
                                    Plugin.Log.Warn($"Song not downloaded, : {(string.IsNullOrEmpty(hash) ? " unknown hash!" : ("hash " + hash + "!"))}");
                            }
                        }
                        CustomBeatmapLevelCollectionSO customBeatmapLevelCollection = CustomBeatmapLevelCollectionSO.CreateInstance(beatmapLevels.ToArray());
                        String playlistTitle = "Untitled Playlist";
                        String playlistImage = CustomPlaylistSO.DEFAULT_IMAGE;
                        if ((string)playlistJSON["playlistTitle"] != null)
                            playlistTitle = (string)playlistJSON["playlistTitle"];
                        if ((string)playlistJSON["image"] != null)
                            playlistImage = (string)playlistJSON["image"];
                        playlists.Add(CustomPlaylistSO.CreateInstance(playlistTitle, playlistImage, customBeatmapLevelCollection));
                    }
                }
                catch (Exception e)
                {
                    Plugin.Log.Critical($"Error loading Playlist File: " + playlistPaths[i] + " Exception: " + e.Message);
                }
            }
            return playlists.ToArray();
        }

        private static IPreviewBeatmapLevel MatchSongById(string levelId)
        {
            if (!SongCore.Loader.AreSongsLoaded || SongCore.Loader.AreSongsLoading)
            {
                Plugin.Log.Info("Songs not loaded. Not Matching songs for playlist.");
                return null;
            }
            IPreviewBeatmapLevel x = null;
            try
            {
                if (!string.IsNullOrEmpty(levelId))
                {
                    if (!levelId.StartsWith(CustomLevelLoader.kCustomLevelPrefixId))
                        return Loader.GetOfficialLevelById(levelId).PreviewBeatmapLevel;
                    else
                    {
                        x = MatchSong(Collections.hashForLevelID(levelId));
                    }
                }
            }
            catch (Exception)
            {
                Plugin.Log.Warn($"Unable to match song with {(string.IsNullOrEmpty(levelId) ? " unknown levelId!" : ("levelId " + levelId + " !"))}");
            }
            return x;
        }

        private static IPreviewBeatmapLevel MatchSong(String hash)
        {
            if (!SongCore.Loader.AreSongsLoaded || SongCore.Loader.AreSongsLoading)
            {
                Plugin.Log.Info("Songs not loaded. Not Matching songs for playlist.");
                return null;
            }
            IPreviewBeatmapLevel x = null;
            try
            {
                if (!string.IsNullOrEmpty(hash))
                    x = SongCore.Loader.GetLevelByHash(hash);
            }
            catch (Exception)
            {
                Plugin.Log.Warn($"Unable to match song with {(string.IsNullOrEmpty(hash) ? " unknown hash!" : ("hash " + hash + " !"))}");
            }
            return x;
        }
    }
}
