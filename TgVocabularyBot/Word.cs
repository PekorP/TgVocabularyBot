using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TgVocabularyBot
{
    [Table("words")]
    public class Word
    {
        public int Id { get; set; }
        public string Rus { get; set; }
        public string Eng { get; set; }
    }
}
