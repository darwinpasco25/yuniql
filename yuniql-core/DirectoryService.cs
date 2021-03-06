﻿using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Yuniql.Core
{
    /// <summary>
    /// Wraps usage of <see cref="Directory"/>
    /// </summary>
    public class DirectoryService : IDirectoryService
    {
        private const string ENVIRONMENT_CODE_PREFIX = "_";

        ///<inheritdoc/>
        public string[] GetDirectories(string path, string searchPattern)
        {
            return Directory.GetDirectories(path, searchPattern);
        }

        ///<inheritdoc/>
        public string[] GetAllDirectories(string path, string searchPattern)
        {
            return Directory.GetDirectories(path, searchPattern, SearchOption.AllDirectories);
        }

        ///<inheritdoc/>
        public string[] GetFiles(string path, string searchPattern)
        {
            return Directory.GetFiles(path, searchPattern, SearchOption.TopDirectoryOnly);
        }

        ///<inheritdoc/>
        public string[] GetAllFiles(string path, string searchPattern)
        {
            return Directory.GetFiles(path, searchPattern, SearchOption.AllDirectories);
        }

        ///<inheritdoc/>
        public bool Exists(string path)
        {
            return Directory.Exists(path);
        }

        ///<inheritdoc/>
        public string GetFileCaseInsensitive(string path, string fileName)
        {
            return Directory.GetFiles(path, "*.dll")
                .ToList()
                .FirstOrDefault(f => new FileInfo(f).Name.ToLower() == fileName.ToLower());
        }

        //TODO: improve this code!!!!
        ///<inheritdoc/>
        public string[] FilterFiles(string workingPath, string environmentCode, List<string> files)
        {
            var rootParts = Split(new DirectoryInfo(workingPath)).ToList();
            rootParts.Reverse();

            var hasEnvironmentAwareDirectories = files.Any(f =>
            {
                var fileParts = Split(new DirectoryInfo(Path.GetDirectoryName(f))).Where(x=> !x.Equals("_transaction", System.StringComparison.InvariantCultureIgnoreCase)).ToList();
                fileParts.Reverse();
                return fileParts.Skip(rootParts.Count).Any(a => a.StartsWith(ENVIRONMENT_CODE_PREFIX));
            });

            if (string.IsNullOrEmpty(environmentCode) && !hasEnvironmentAwareDirectories)
                return files.ToArray();

            //throws exception when no environment code passed but environment-aware directories are present
            if (string.IsNullOrEmpty(environmentCode) && hasEnvironmentAwareDirectories)
                throw new YuniqlMigrationException("Found environment aware directories but no environment code passed. " +
                    "See https://github.com/rdagumampan/yuniql/wiki/environment-aware-scripts.");

            //remove all script files from environment-aware directories except the target environment
            var sqlScriptFiles = new List<string>(files);
            files.ForEach(f =>
            {
                var fileParts = Split(new DirectoryInfo(Path.GetDirectoryName(f))).Where(x => !x.Equals("_transaction", System.StringComparison.InvariantCultureIgnoreCase)).ToList();
                fileParts.Reverse();

                var foundFile = fileParts.Skip(rootParts.Count).FirstOrDefault(a => a.StartsWith(ENVIRONMENT_CODE_PREFIX) && a.ToLower()!= $"{ENVIRONMENT_CODE_PREFIX}{environmentCode}");
                if (null != foundFile)
                    sqlScriptFiles.Remove(f);
            });

            return sqlScriptFiles.ToArray();
        }

        ///<inheritdoc/>
        public string[] FilterDirectories(string workingPath, string environmentCode, List<string> directories)
        {
            throw new System.NotImplementedException();
        }

        private IEnumerable<string> Split(DirectoryInfo directory)
        {
            while (directory != null)
            {
                yield return directory.Name;
                directory = directory.Parent;
            }
        }
    }
}
