using System;
using System.Collections.Generic;
using System.Data.Entity;

namespace TgVocabularyBot
{
    class WordContext : DbContext
    {
        public WordContext()
            : base(Properties.Settings.Default.strConnection)
        { }

        public DbSet<Word>Words { get; set; }
    }
}