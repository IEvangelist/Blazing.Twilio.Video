﻿// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace Blazing.Twilio.Video.Client;

/// <summary>
/// An app state class ? some properties are persisted to <c>window.localStorage</c>.
/// </summary>
public sealed class AppState
{
    readonly ILocalStorageService _storage;

    string? _selectedCameraId;
    bool _isDarkTheme;
    string? _activeRoomName;
    HashSet<RoomDetails>? _rooms;
    CameraStatus _cameraStatus;

    /// <summary>
    /// An event that is fired when the app state has changed.
    /// </summary>
    public event Action<string>? StateChanged;

    public AppState(ILocalStorageService storage) => _storage = storage;

    /// <summary>
    /// Gets or sets the selected camera device identifier.
    /// Persisted to <c>window.localStorage["camera-device-id"]</c>
    /// </summary>
    public string? SelectedCameraId
    {
        get => _selectedCameraId = _storage.GetItem<string?>(StorageKeys.CameraDeviceId);
        set
        {
            if (_selectedCameraId != value)
            {
                _storage.SetItem(StorageKeys.CameraDeviceId, _selectedCameraId = value);
                StateChanged?.Invoke(nameof(SelectedCameraId));
            }
        }
    }

    /// <summary>
    /// Gets or sets the current camera status:
    /// <list type="bullet">
    /// <item><c><see cref="CameraStatus.Idle"/></c>: (default) camera status is idle.</item>
    /// <item><c><see cref="CameraStatus.Previewing"/></c>: camera is being previewed.</item>
    /// <item><c><see cref="CameraStatus.InCall"/></c>: camera is being used in a call.</item>
    /// </list>
    /// </summary>
    public CameraStatus CameraStatus
    {
        get => _cameraStatus;
        set
        {
            if (_cameraStatus != value)
            {
                _cameraStatus = value;
                StateChanged?.Invoke(nameof(CameraStatus));
            }
        }
    }

    /// <summary>
    /// Gets or sets whether dark-theme is preferred.
    /// Persisted to <c>window.localStorage["prefers-dark-theme"]</c>
    /// </summary>
    public bool IsDarkTheme
    {
        get => _isDarkTheme = _storage.GetItem<bool>(StorageKeys.PrefersDarkTheme);
        set
        {
            if (_isDarkTheme != value)
            {
                _storage.SetItem(StorageKeys.PrefersDarkTheme, _isDarkTheme = value);
                StateChanged?.Invoke(nameof(IsDarkTheme));
            }
        }
    }

    /// <summary>
    /// Gets or sets the name of the active room.
    /// Not persisted to <c>window.localStorage</c>.
    /// </summary>
    public string? ActiveRoomName
    {
        get => _activeRoomName;
        set
        {
            if (_activeRoomName != value)
            {
                _activeRoomName = value;
                StateChanged?.Invoke(nameof(ActiveRoomName));
            }
        }
    }

    /// <summary>
    /// Gets or sets a <see cref="HashSet{T}"/> where
    /// <c>T</c> is <see cref="RoomDetails"/>. Not persisted
    /// to <c>window.localStorage</c>.
    /// </summary>
    public HashSet<RoomDetails>? Rooms
    {
        get => _rooms;
        set
        {
            _rooms = value;
            StateChanged?.Invoke(nameof(Rooms));
        }
    }
}

file sealed class StorageKeys
{
    internal const string PrefersDarkTheme = "prefers-dark-theme";
    internal const string CameraDeviceId = "camera-device-id";
}