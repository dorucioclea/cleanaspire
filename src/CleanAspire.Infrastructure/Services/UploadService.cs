﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CleanAspire.Application.Common.Interfaces;

namespace CleanAspire.Infrastructure.Services;

/// <summary>
/// Service for uploading files.
/// </summary>
public class UploadService : IUploadService
{
    private static readonly string NumberPattern = " ({0})";

    /// <summary>
    /// Uploads a file asynchronously.
    /// </summary>
    /// <param name="request">The upload request.</param>
    /// <returns>The path of the uploaded file.</returns>
    public async Task<string> UploadAsync(UploadRequest request)
    {
        if (request.Data == null || !request.Data.Any()) return string.Empty;

        var folder = request.UploadType.ToString().ToLower();
        var folderName = Path.Combine("files", folder);
        if (!string.IsNullOrEmpty(request.Folder))
        {
            folderName = Path.Combine(folderName, request.Folder);
        }
        var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
        if (!Directory.Exists(pathToSave))
        {
            Directory.CreateDirectory(pathToSave);
        }

        var fileName = request.FileName.Trim('"');
        var fullPath = Path.Combine(pathToSave, fileName);
        var dbPath = Path.Combine(folderName, fileName);

        if (!request.Overwrite && File.Exists(dbPath))
        {
            dbPath = NextAvailableFilename(dbPath);
            fullPath = NextAvailableFilename(fullPath);
        }

        await using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await stream.WriteAsync(request.Data, 0, request.Data.Length);
        }

        return dbPath;
    }
    /// <summary>
    /// remove file
    /// </summary>
    /// <param name="filename"></param>
    public void Remove(string filename)
    {
        var removefile = Path.Combine(Directory.GetCurrentDirectory(), filename);
        if (File.Exists(removefile))
        {
            File.Delete(removefile);
        }
    }
    /// <summary>
    /// Gets the next available filename based on the given path.
    /// </summary>
    /// <param name="path">The path to check for availability.</param>
    /// <returns>The next available filename.</returns>
    public static string NextAvailableFilename(string path)
    {
        if (!File.Exists(path))
            return path;

        path = path.Replace("{", "{{").Replace("}", "}}");
        if (Path.HasExtension(path))
            return GetNextFilename(path.Insert(path.LastIndexOf(Path.GetExtension(path)), NumberPattern));

        return GetNextFilename(path + NumberPattern);
    }

    /// <summary>
    /// Gets the next available filename based on the given pattern.
    /// </summary>
    /// <param name="pattern">The pattern to generate the filename.</param>
    /// <returns>The next available filename.</returns>
    private static string GetNextFilename(string pattern)
    {
        var tmp = string.Format(pattern, 1);

        if (!File.Exists(tmp))
            return tmp;

        int min = 1, max = 2;

        while (File.Exists(string.Format(pattern, max)))
        {
            min = max;
            max *= 2;
        }

        while (max != min + 1)
        {
            var pivot = (max + min) / 2;
            if (File.Exists(string.Format(pattern, pivot)))
                min = pivot;
            else
                max = pivot;
        }

        return string.Format(pattern, max);
    }
}
