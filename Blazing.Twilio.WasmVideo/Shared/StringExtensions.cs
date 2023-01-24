// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;

namespace Blazing.Twilio.WasmVideo.Shared;

/// <summary>
/// Extension methods for the <see cref="string"/> type.
/// </summary>
public static class StringExtensions
{
    /// <inheritdoc cref="string.IsNullOrEmpty(string?)" />
    public static bool IsNullOrEmpty([NotNullWhen(false)] this string? value) =>
        string.IsNullOrEmpty(value);

    /// <inheritdoc cref="string.IsNullOrWhiteSpace(string?)" />
    public static bool IsNullOrWhiteSpace([NotNullWhen(false)] this string? value) =>
        string.IsNullOrWhiteSpace(value);
}