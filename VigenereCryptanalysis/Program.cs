using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace VigenereCryptanalysis
{
    class Program
    {

        const int MAX_KEY_LEN = 20;
        static SortedDictionary<string, int> trainingQuadgrams;


        static void Main(string[] args)
        {
            initQuadgramsDict();

            string cypheredMessage = File.ReadAllText("vigenere.in");
            Dictionary<string, double> parentKeys = new Dictionary<string, double>();

            //check for multiple possible keys
            for (int key_len = 2; key_len <= MAX_KEY_LEN; key_len++)
            {
                //create the initial parent key for a key length of key_len
                string parentKey = new string('A', key_len);
                double fitnessOfParent = quadgramStatistics(decryptVigenere(cypheredMessage, parentKey));
                bool parent_key_changed = false;

                //process parent key until there is no change
                //then we know we are done
                do
                {
                    parent_key_changed = false;
                    //get through every letter of the parent key
                    for(int i = 0; i < key_len; i++)
                    {
                        //for each letter go all the way from A to Z
                        //or until the new key is "fitter" than the parent key
                        for (char currChar = 'A'; currChar <= 'Z'; currChar++)
                        {
                            StringBuilder childKeyBuilder = new StringBuilder(parentKey);
                            childKeyBuilder[i] = currChar;
                            string childKey = childKeyBuilder.ToString();
                            double childFitness = quadgramStatistics(decryptVigenere(cypheredMessage, childKey));

                            if(childFitness > fitnessOfParent)
                            {
                                parentKey = childKey;
                                parent_key_changed = true;
                                fitnessOfParent = childFitness;
                                break;
                            }
                        }

                        if(!parentKeys.ContainsKey(parentKey))
                        {
                            parentKeys.Add(parentKey, fitnessOfParent);
                        }
                    }

                } while (parent_key_changed);


            }

            File.Delete("vigenere.out");

            for (int i = 0; i < 5; i++)
            {
                double max = double.MinValue;
                string key = "";
                for (int k = 0; k < parentKeys.Count; k++)
                {
                    if (max < parentKeys.ElementAt(k).Value)
                    {
                        max = parentKeys.ElementAt(k).Value;
                        key = parentKeys.ElementAt(k).Key;
                    }
                }
                using (StreamWriter sw = File.AppendText("vigenere.out"))
                {
                    sw.WriteLine(key + "  --  " + decryptVigenere(cypheredMessage, key) + "  --  " + parentKeys[key]);
                    sw.WriteLine();
                }
                parentKeys.Remove(key);
            }
        }
        
        /// <summary>
        /// Decrypts a message cyphered with Vigenere given a key
        /// </summary>
        /// <param name="cypheredMessage">The cyphered message</param>
        /// <param name="key">The key</param>
        /// <returns>The decrypted message</returns>
        static string decryptVigenere(string cypheredMessage, string key)
        {
            StringBuilder plainText = new StringBuilder();

            for(int i = 0; i< cypheredMessage.Length; i++)
            {
                char plainChar = (char)('A' + cypheredMessage[i] - key[i % key.Length]);
                if (plainChar < 'A')
                    plainChar = (char)(('Z' + 1) - ('A' - plainChar));

                plainText.Append(plainChar);
            }

            return plainText.ToString();
        }

        /// <summary>
        /// Computes quadgrams statistics as fitness message for a given text
        /// </summary>
        /// <param name="text">The text to be analyzed</param>
        /// <returns>The fitness measure</returns>
        static double quadgramStatistics(string text)
        {
            const int WarAndPeaceQuadgrams = 2500000;
            List<string> quadgrams = new List<string>();
            double totalFitness = 0d;

            for(int i = 0; i < text.Length - 3; i++)
            {
                string newQuadgram = text.Substring(i, 4);

                if (trainingQuadgrams.ContainsKey(newQuadgram))
                    totalFitness += (Math.Log((double)trainingQuadgrams[newQuadgram] / (double)WarAndPeaceQuadgrams));
                else
                    totalFitness += (Math.Log(Math.Pow(10, -3) / (double)WarAndPeaceQuadgrams));
            }

            return totalFitness;
        }

        static void initQuadgramsDict()
        {
            trainingQuadgrams = new SortedDictionary<string, int>();

            using (StreamReader engQuadgrams = new StreamReader("english_quadgrams.txt"))
            {
                while (!engQuadgrams.EndOfStream)
                {
                    string[] fields = engQuadgrams.ReadLine().Split(' ');
                    string key = fields[0].Trim();
                    int value = Int32.Parse(fields[1].Trim());
                    trainingQuadgrams.Add(key, value);
                }
            }
        }
    }
}
