using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Server.Utils
{
    public class Utils
    {
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
    }
}