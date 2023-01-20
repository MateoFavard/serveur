using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Server.Utils
{
    public class Util
    {
        public static JsonSerializerOptions DefaultJsonOptions => new JsonSerializerOptions {
            AllowTrailingCommas = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = true,
        };

        private const String ALPHANUM = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

        static public String RandomPassword(int size)
        {
            string result = "";
            Random random = new Random();

            for (int i = 0; i < size; i++)
            {
                result += ALPHANUM[random.Next(ALPHANUM.Length)];
            }

            return result;
        }

        static public byte[] StoB(string s, string sel)
        {
            Byte[] b = Encoding.UTF8.GetBytes(s + sel);
            return b;
        }

        static public byte[] BtoH(string mdp, string sel)
        {
            Byte[] res = new Byte[32];
            using (SHA256 sha256Hash = SHA256.Create())
            {
                res = sha256Hash.ComputeHash(StoB(mdp, sel));
            }
            return res;
        }

        static public bool VerifyH(string mdp, string sel, Byte[] hash)
        {
            Byte[] temp = BtoH(mdp, sel);
            return Enumerable.SequenceEqual(temp, hash);
        }

        public static string GenerationNom()
        {
            Random rand = new Random();
            string res = "";
            string[] BaseNom = { "ae", "gn", "or", "ran", "ir", "am", "rie", "ir", "rod", "ael", "is", "el", "na", "ro", "chi" };
            for (int i = 0; i < rand.Next(2, 3); i++)
            {
                res += BaseNom[rand.Next(BaseNom.Length)];
            }
            res += " ";
            for (int i = 0; i < rand.Next(2, 3); i++)
            {
                res += BaseNom[rand.Next(BaseNom.Length)];
            }
            return res;
        }
    }
}