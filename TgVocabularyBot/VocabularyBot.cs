using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;

namespace TgVocabularyBot
{
    class VocabularyVot
    {
        const string telegramToken = "secret";
        public static TelegramBotClient client;
        static bool IsVocabularyOpen = false;
        static int firstStepOfDiapason = 0, secondStepOfDiapason = 10;
        static int totalNumberOfRecords;


        public static void commands(object sender, MessageEventArgs e)
        {
            var text = e.Message.Text.Split(' ');
            switch (text[0])
            {
                case "/start":
                case "/listofcommands":
                    commandStart(sender, e);
                    break;
                case "/translatetoeng":
                    try
                    {
                        commandTranslateToEng(sender, e, text[1]);
                    }
                    catch (System.IndexOutOfRangeException ex)
                    {
                        client.SendTextMessageAsync(e.Message.Chat, "Введите слово которое надо перевести через пробел после команды");
                    }
                    break;
                case "/translatetorus":
                    try
                    {
                        commandTranslateToRus(sender, e, text[1]);
                    }
                    catch (System.IndexOutOfRangeException ex)
                    {
                        client.SendTextMessageAsync(e.Message.Chat, "Введите слово которое надо перевести через пробел после команды");
                    }
                    break;
                case "/addword":
                    try
                    {
                        commandAddWord(e, text[1], text[2]);
                    }
                    catch(System.IndexOutOfRangeException ex)
                    {
                        client.SendTextMessageAsync(e.Message.Chat, "Введите слово которое надо добавить и его перевод через пробел после команды");
                    }
                    break;
                case "/deleteword":
                    try
                    {
                        commandDeleteWord(e, text[1]);
                    }
                    catch (System.IndexOutOfRangeException ex)
                    {
                        client.SendTextMessageAsync(e.Message.Chat, "Введите слово которое необходимо удалить (рус/англ)");
                    }
                    break;
                case "/showvocabulary":                   
                    commandShowVocabulary(e);
                    break;
                case "/closevocabulary":
                    commandCloseVocabulary(e);
                    break;
                case "/next":
                    commandNext(e);
                    break;
                case "/prev":
                    commandPrev(e);
                    break;
                case "/wiki":
                    try
                    {
                        wiki(e, text[1]);
                    }
                    catch (System.IndexOutOfRangeException ex)
                    {
                        client.SendTextMessageAsync(e.Message.Chat, "Введите слово после команды");
                    }
                    break;
                default:
                    client.SendTextMessageAsync(e.Message.Chat, "Такой команды нет");
                    break;
            }
        }

        private static void wiki(MessageEventArgs e, string word)
        {
            string wiki = "https://wiktionary.org/wiki/";
            wiki += word;
            client.SendTextMessageAsync(e.Message.Chat, wiki);
        }

        private static void commandPrev(MessageEventArgs e)
        {
            if (IsVocabularyOpen == true)
            {
                if (firstStepOfDiapason != 0)
                {
                    firstStepOfDiapason -= 10;
                    commandShowVocabulary(e);
                }
                else
                {
                    client.SendTextMessageAsync(e.Message.Chat, "Это первая страница");
                    commandShowVocabulary(e);                 
                }
            }
            else
            {
                client.SendTextMessageAsync(e.Message.Chat, "Сначала вызовите команду /showvocabulary");
            }
        }

        private static void commandNext(MessageEventArgs e)
        {
            using (WordContext db = new WordContext())
            {
                var words = db.Words;
                totalNumberOfRecords = words.Count(); 
            }
            
            if (IsVocabularyOpen == true)
            {
                firstStepOfDiapason += 10;
                if (firstStepOfDiapason >= totalNumberOfRecords)
                {
                    client.SendTextMessageAsync(e.Message.Chat, "Это последняя страница");
                    firstStepOfDiapason -= 10;
                    commandShowVocabulary(e);                   
                }
                else
                {        
                    commandShowVocabulary(e);
                }
            }
            else
            {
                client.SendTextMessageAsync(e.Message.Chat, "Сначала вызовите команду /showvocabulary");
            }
        }

        private static void commandCloseVocabulary(MessageEventArgs e)
        {
            firstStepOfDiapason = 0;
            secondStepOfDiapason = 10;
            IsVocabularyOpen = false;
            client.SendTextMessageAsync(e.Message.Chat, "Словарь закрыт, для повторного просмотра используйте /showvocabulary");
        }

        private static void commandShowVocabulary(MessageEventArgs e)
        {
            IsVocabularyOpen = true;
            using (WordContext db = new WordContext())
            {
                var words = db.Words.Take(secondStepOfDiapason).OrderBy(x => x.Id).Skip(firstStepOfDiapason);
                string listOfWords = "";
                int index = firstStepOfDiapason + 1;
                foreach (Word w in words)
                {
                    listOfWords += $"{index}. {w.Rus} - {w.Eng}\n";
                    index++;
                }
                client.SendTextMessageAsync(e.Message.Chat, listOfWords);
            }
        }

        private static void commandDeleteWord(MessageEventArgs e, string word)
        {
            using (WordContext db = new WordContext())
            {              
                var searchWord = db.Words.Where(x => x.Eng == word || x.Rus == word);
                int delete = -1;
                foreach (Word w in searchWord)
                {
                    delete = w.Id;
                }
                if(delete == -1)
                {
                    client.SendTextMessageAsync(e.Message.Chat, "Такого слова нет в словаре");
                    return;
                }
                else
                {
                    var deleteWord = db.Words.First(c => c.Id == delete);
                    db.Words.Remove(deleteWord);
                    db.SaveChanges();
                    client.SendTextMessageAsync(e.Message.Chat, $"Удалено слово {word}");
                    return;
                }
            }
        }

        private static void commandAddWord(MessageEventArgs e, string wordRus, string wordEng)
        {
            using (WordContext db = new WordContext())
            {
                if (db.Words.Any(x => x.Rus == wordRus) || db.Words.Any(x => x.Eng == wordEng))
                {
                    client.SendTextMessageAsync(e.Message.Chat, "Такое слово уже есть в словаре");
                    return;
                }
                Word word = new Word { Rus = wordRus, Eng = wordEng };              
                db.Words.Add(word);
                db.SaveChanges();
                Console.WriteLine("Объекты успешно сохранены");
            }
        }

        private static void commandTranslateToEng(object sender, MessageEventArgs e, string word)
        {   
            using (WordContext db = new WordContext())
            {
                var searchWord = db.Words.Where(p => p.Rus == word);
                if (!searchWord.Any(x => x.Rus == word))
                {
                    client.SendTextMessageAsync(e.Message.Chat, "Такого слова нет в словаре, Вы можете добавить его с помощью команды /addword");
                    return;
                }
                foreach (Word itemWord in searchWord)
                    client.SendTextMessageAsync(e.Message.Chat,  ($"Слово \"{itemWord.Rus}\" - перевод \"{itemWord.Eng}\""));
            }
        }

        private static void commandTranslateToRus(object sender, MessageEventArgs e, string word)
        {
            using (WordContext db = new WordContext())
            {
                var searchWord = db.Words.Where(p => p.Eng == word);
                if (!searchWord.Any(x => x.Eng == word))
                {
                    client.SendTextMessageAsync(e.Message.Chat, "Такого слова нет в словаре, Вы можете добавить его с помощью команды /addword");
                    return;
                }
                foreach (Word itemWord in searchWord)
                    client.SendTextMessageAsync(e.Message.Chat, ($"Слово \"{itemWord.Eng}\" - перевод \"{itemWord.Rus}\""));
            }
        }

        private static void commandStart(object sender, MessageEventArgs e)
        {
            string listOfCommands = "Список команд:\n/listofcommands - Вывести список команд\n/translatetoeng <word>- Перевести введённое слово(на англ)" +
                "\n/translatetorus <word> - Перевести введённое слово(на рус)\n/addword <RusWord> <EngWord> - Добавить слово и перевод в словарь\n/deleteword <word> - Удалить слово из словаря" +
                "\n/showvocabulary - Показать первые 10 слов\n/next - Показать следующие 10 слов\n/prev - Показать предыдущие 10 слов" +
                "\n/closevocabulary - Выйти из словаря\n/wiki <word> - ссылка на страницу в Википедии";
            client.SendTextMessageAsync(e.Message.Chat, listOfCommands);
        }

        static void Main(string[] args)
        {
            client = new TelegramBotClient(telegramToken);
            client.OnMessage += commands;
            client.StartReceiving();
            Console.ReadLine();
            client.StopReceiving();
        }
    }
}
