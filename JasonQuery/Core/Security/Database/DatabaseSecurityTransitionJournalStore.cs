using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;

namespace JasonQuery.Core.Security.Database
{
    public sealed class DatabaseSecurityTransitionJournalStore
    {
        private readonly string _journalFilePath;
        private readonly string _temporaryFilePath;

        public DatabaseSecurityTransitionJournalStore(string journalFilePath)
        {
            if (string.IsNullOrWhiteSpace(journalFilePath))
            {
                throw new ArgumentException("A transition journal path is required.", nameof(journalFilePath));
            }

            _journalFilePath = Path.GetFullPath(journalFilePath);
            _temporaryFilePath = _journalFilePath + ".tmp";
        }

        public string JournalFilePath => _journalFilePath;
        public bool Exists => File.Exists(_journalFilePath);

        public DatabaseSecurityTransitionJournal Load()
        {
            if (!Exists)
            {
                throw new FileNotFoundException("The database security transition journal was not found.", _journalFilePath);
            }

            var json = File.ReadAllText(_journalFilePath, Encoding.UTF8);
            var journal = JsonConvert.DeserializeObject<DatabaseSecurityTransitionJournal>(json);

            if (journal == null)
            {
                throw new InvalidDataException("The database security transition journal is empty or invalid.");
            }

            journal.Validate();
            return journal;
        }

        public void Save(DatabaseSecurityTransitionJournal journal)
        {
            if (journal == null)
            {
                throw new ArgumentNullException(nameof(journal));
            }

            journal.Validate();

            var directory = Path.GetDirectoryName(_journalFilePath);

            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonConvert.SerializeObject(journal, Formatting.Indented);
            var bytes = new UTF8Encoding(false).GetBytes(json);

            DeleteTemporaryFileIfExists();

            try
            {
                using (var stream = new FileStream(_temporaryFilePath, FileMode.CreateNew, FileAccess.Write,
                                                   FileShare.None, 4096, FileOptions.WriteThrough))
                {
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Flush(true);
                }

                if (File.Exists(_journalFilePath))
                {
                    File.Replace(_temporaryFilePath, _journalFilePath, null, true);
                }
                else
                {
                    File.Move(_temporaryFilePath, _journalFilePath);
                }
            }
            finally
            {
                Array.Clear(bytes, 0, bytes.Length);
                DeleteTemporaryFileIfExists();
            }
        }

        public void Delete()
        {
            if (File.Exists(_journalFilePath))
            {
                File.Delete(_journalFilePath);
            }

            DeleteTemporaryFileIfExists();
        }

        public void DeleteTemporaryFileIfExists()
        {
            if (File.Exists(_temporaryFilePath))
            {
                File.Delete(_temporaryFilePath);
            }
        }
    }
}
