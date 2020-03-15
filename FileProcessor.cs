using ExifLib;
using NReco.VideoInfo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PhotoOrganizer
{
    /// <summary>
    /// 
    /// </summary>
    internal class FileProcessor
    {
        private readonly string[] _videoFileExtensions = new[]
        {
            ".webm", ".mpg", ".mp2", ".mpeg", ".mpe", ".mpv", ".ogg", ".mp4", ".m4p", ".m4v", ".avi", ".wmv", ".mov", ".qt", ".flv", ".swf", ".avchd"
        };

        private readonly string[] _imageFileExtensions = new[]
        {
            ".tif", ".tiff", ".gif", ".jpeg", ".jpg", ".jif", ".jfif", ".jp2", ".jpx", ".j2k", ".j2c", ".fpx", ".pcd", ".png"
        };

        /// <summary>
        /// Process file
        /// </summary>
        /// <param name="path">Input file path</param>
        internal void ProcessFile(string path, string outputDirectory, string fileNameFormat)
        {
            var fileInfo = new FileInfo(path);
            var fileDate = GetFileDate(fileInfo);

            string rootDirectoryPath = Path.Combine(outputDirectory, $"{fileDate.Year}", $"{fileDate.Month:00}");
            if (!Directory.Exists(rootDirectoryPath))
            {
                Directory.CreateDirectory(rootDirectoryPath);
            }

            string outputFilePath = Path.Combine(rootDirectoryPath, Path.ChangeExtension(fileDate.ToString(fileNameFormat), fileInfo.Extension)).ToLower();

            if (File.Exists(outputFilePath))
            {
                if (fileInfo.Length == new FileInfo(outputFilePath).Length)
                {
                    fileInfo.Delete();
                }
                else
                {
                    outputFilePath = GetAvailableFileName(rootDirectoryPath, fileDate, fileNameFormat, fileInfo.Extension).ToLower();
                    if (!string.IsNullOrEmpty(outputFilePath))
                    {
                        fileInfo.MoveTo(outputFilePath);
                    }
                }
            }
            else
            {
                fileInfo.MoveTo(outputFilePath);
            }
        }

        /// <summary>
        /// Returns media file creation date
        /// </summary>
        /// <param name="fileInfo">Media file info</param>
        private DateTime GetFileDate(FileInfo fileInfo)
        {
            IList<DateTime> dates = new List<DateTime>
            {
                fileInfo.CreationTime,
                fileInfo.LastAccessTime,
                fileInfo.LastWriteTime
            };

            if (_imageFileExtensions.Contains(fileInfo.Extension.ToLower()))
            {
                try
                {
                    using (ExifReader exifReader = new ExifReader(fileInfo.FullName))
                    {
                        if (exifReader.GetTagValue(ExifTags.DateTimeDigitized, out DateTime datePictureTaken))
                        {
                            dates.Add(datePictureTaken);
                        }
                    }
                }
                catch
                {
                    //---   ignore
                }
            }

            if (_videoFileExtensions.Contains(fileInfo.Extension.ToLower()))
            {
                try
                {
                    var ffProbe = new FFProbe();
                    var videoInfo = ffProbe.GetMediaInfo(fileInfo.FullName);

                    var creationTimePair = videoInfo.FormatTags.FirstOrDefault(x => x.Key.Equals("creation_time"));
                    if (DateTime.TryParse(creationTimePair.Value, out var dateVideoTaken))
                    {
                        dates.Add(dateVideoTaken);
                    }
                }
                catch
                {
                    //---   ignore
                }
            }

            return dates.Min();
        }

        /// <summary>
        /// Returns available file name
        /// </summary>
        /// <param name="outputDirectory">Output directory path</param>
        /// <param name="fileDate">File date time</param>
        /// <param name="format">File name format</param>
        /// <param name="extension">Input file extension</param>
        private string GetAvailableFileName(string outputDirectory, DateTime fileDate, string format, string extension)
        {
            int index = 1;
            while (index < 500)
            {
                fileDate = fileDate.AddSeconds(1);
                string filePath = Path.Combine(outputDirectory, Path.ChangeExtension(fileDate.ToString(format), extension));
                if (File.Exists(filePath))
                {
                    index++;
                    continue;
                }

                return filePath;
            }

            return string.Empty;
        }
    }
}