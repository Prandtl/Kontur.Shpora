using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace Jaccard_Index
{
    public class Gram
    {
        public string[] Words;
        public int Freq;

        public Gram(string[] words, int frequency)
        {
            Words = words;
            Freq = frequency;
        }

        public bool SameWords(string[] gram)
        {
            if (this.Words.Length == gram.Length) //this не нужен
            {
                for (int i = 0; i < gram.Length; i++)
                {
                    if (this.Words[i] != gram[i])
                    {
                        return false;
                    }
                }
                return true;
            }
            else //тут else не нужен.
                return false;
        }

        /*
         * Так как курс по Linq почему бы и не использовать Linq?
         */

        public bool SameWordsLINQ(string[] gram)
        {
            if (Words.Length == gram.Length)
            {
                return !gram.Where((t, i) => Words[i] != t).Any();
            }
            return false;
        }
    }

    internal class Program
    {   
        /*Ниже штука которая переводит предложение в Граммы. судя по названию.
         * Почему она возвращает двумерный массив строк не очень очевидно
         */
        private static string[][] Sentence2Grams(string[] sentence, int size)//Можно было IENumerable<string[]> вернуть..
        {
            string[] filteredSentence = sentence.Where(word => word != "").ToArray();//var.
            if (size > filteredSentence.Length)
            {
                return new string[][] {new string[] {}};//!?!?!?
            }
            else//else опять же не нужен. 
                //Да, это как бы не то чтобы важно. Но у меня вел чувак из Контура, он к этому придирался
            {
                int length = filteredSentence.Length - size + 1;
                string[][] result = new string[length][];
                for (int i = 0; i < filteredSentence.Length - size + 1; i++)
                {
                    result[i] = new string[size];
                    for (int j = 0; j < size; j++)
                    {
                        result[i][j] = filteredSentence[i + j].ToLower();
                    }
                }
                return result;
            }
        }

        //Maybe LINQ? 


        /*
         * А вот этот метод мне нравится.
         * его может и можно изменить, но в целом - LINQ, понятно что происходит и комментарии конечно же!
         * Это круто.
         */
        private static Gram[] GramsWithFrequency(string text, int size)
        {
            Gram[] gramsAndFreq = text
                //Split into sentences
                .Split('!', '?', '.', '(', ')', '[', ']', '{', '}')
                //Split every sentence into words
                .Select(sent => Regex.Split(sent, @"\P{L}"))
                //Selecting grams from every splitted sentence
                .SelectMany(sentence => Sentence2Grams(sentence, size).Where(grams => grams.Length != 0))
                .GroupBy(gram => gram)
                .Select(gram => new Gram(gram.Key, gram.Count()))
                .ToArray();
            return gramsAndFreq;
        }

        private static int Frequency(Gram[] textByGrams, string[] words)
        {
            if (textByGrams.Where(gram => gram.SameWords(words)).Count() == 0)//Any()
            {
                return 0;
            }
            else
            {
                return textByGrams.Where(gram => gram.SameWords(words)).First().Freq;
            }
        }

        private static List<Tuple<string[], int, int>> Merge(Gram[] firstGrams, Gram[] secondGrams)
        {
            List<Tuple<string[], int, int>> result = firstGrams.Select(gram => Tuple.Create(gram.Words, gram.Freq, Frequency(secondGrams, gram.Words))).ToList();//var.
            result.AddRange(secondGrams.Select(gram => Tuple.Create(gram.Words, Frequency(firstGrams, gram.Words), gram.Freq)));
            return result.Distinct().ToList();
        }

        private static void Main(string[] args)
        {
            string firstText = File.ReadAllText(args[0]);
            string secondText = File.ReadAllText(args[1]);
            int size = int.Parse(args[2]);
            Gram[] firstGrams = GramsWithFrequency(firstText, size);
            Gram[] secondGrams = GramsWithFrequency(secondText, size);
            int countGramsOfFirst = firstGrams.Length;
            int countGramsOfSecond = secondGrams.Length;
            List<Tuple<string[], int, int>> allGrams = Merge(firstGrams, secondGrams);
            double common = 0;
            double total = 0;
            foreach (var gram in allGrams)
            {
                common += Math.Min(gram.Item2, gram.Item3);
                total += Math.Max(gram.Item2, gram.Item3);
            }
            double jaccarIndex = common/total;
            Console.WriteLine(countGramsOfFirst);
            Console.WriteLine(countGramsOfSecond);
            Console.WriteLine(jaccarIndex);
            /*All-in-all
             * ну критичные места я показал, а похожие ты сможешь найти.
             * Хотелось бы больше логики жесткой.
             * Ну например есть методы которые вроде бы относятся к классу Грамм а реализованы в Program.
             * больше LINQ.
             * больше var.
             * Поставь Resharper - он выделяет подобные тонкие места и умеет сам их преобразовывать.
             * О правильности вычислений не буду судить.
             */
        }
    }
}

