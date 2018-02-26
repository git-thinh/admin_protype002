using System;
using System.Web;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Text;
using System.Security.Cryptography;

namespace web
{
    public class Account
    {
        public string Username { set; get; }
        public string Password { set; get; }
    }

    public class User : IHttpHandler
    {
        private static List<Account> list = new List<Account>();

        static string pathData = "";
        static void load()
        {
            list.Clear();
            string json = File.ReadAllText(pathData);
            list = JsonConvert.DeserializeObject<List<Account>>(json);
        }

        public static bool login(string key)
        {
            if (pathData == "")
            {
                pathData = HttpContext.Current.Server.MapPath("~/data/user.json");
                load();
            }

            string pass = Decrypt(key);
            bool ok = list.FindIndex(x => x.Username == "admin" && x.Password == pass) != -1;
            return ok;
        }

        public void ProcessRequest(HttpContext context)
        {
            if (pathData == "")
            {
                pathData = context.Server.MapPath("~/data/user.json");
                load();
            }

            string data = "", user = "", pass = "";
            string method = context.Request.HttpMethod;
            switch (method)
            {
                case "GET":
                    user = context.Request.QueryString["user"];
                    pass = context.Request.QueryString["pass"];

                    if (list.FindIndex(x => x.Username == user && x.Password == pass) != -1)
                    {
                        data = Encrypt(pass);
                    }

                    context.Response.ContentType = "text/plain";
                    context.Response.Write(data);
                    break;
                case "POST":
                    user = context.Request.QueryString["user"];
                    pass = context.Request.QueryString["pass"];
                    string pass_new = context.Request.QueryString["pass-new"];
                    bool login = list.FindIndex(x => x.Username == user && x.Password == pass) != -1;
                    if (login) {
                        string json = JsonConvert.SerializeObject(new Account[] {
                            new Account() { Password = pass_new, Username = user }
                        }, Formatting.Indented);
                        File.WriteAllText(pathData, json);
                        load();
                    }
                    context.Response.ContentType = "text/plain";
                    context.Response.Write(data);
                    break;
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        static string key = "123";
        public static string Encrypt(string toEncrypt, bool useHashing = true)
        {
            byte[] keyArray;
            byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(toEncrypt);

             
            //System.Windows.Forms.MessageBox.Show(key);
            //If hashing use get hashcode regards to your key
            if (useHashing)
            {
                MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
                keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
                //Always release the resources and flush data
                // of the Cryptographic service provide. Best Practice

                hashmd5.Clear();
            }
            else
                keyArray = UTF8Encoding.UTF8.GetBytes(key);

            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
            //set the secret key for the tripleDES algorithm
            tdes.Key = keyArray;
            //mode of operation. there are other 4 modes.
            //We choose ECB(Electronic code Book)
            tdes.Mode = CipherMode.ECB;
            //padding mode(if any extra byte added)

            tdes.Padding = PaddingMode.PKCS7;

            ICryptoTransform cTransform = tdes.CreateEncryptor();
            //transform the specified region of bytes array to resultArray
            byte[] resultArray =
              cTransform.TransformFinalBlock(toEncryptArray, 0,
              toEncryptArray.Length);
            //Release resources held by TripleDes Encryptor
            tdes.Clear();
            //Return the encrypted data into unreadable string format
            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }

        public static string Decrypt(string cipherString, bool useHashing = true)
        {
            byte[] keyArray;
            //get the byte code of the string

            byte[] toEncryptArray = Convert.FromBase64String(cipherString);

             

            if (useHashing)
            {
                //if hashing was used get the hash code with regards to your key
                MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
                keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
                //release any resource held by the MD5CryptoServiceProvider

                hashmd5.Clear();
            }
            else
            {
                //if hashing was not implemented get the byte code of the key
                keyArray = UTF8Encoding.UTF8.GetBytes(key);
            }

            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
            //set the secret key for the tripleDES algorithm
            tdes.Key = keyArray;
            //mode of operation. there are other 4 modes. 
            //We choose ECB(Electronic code Book)

            tdes.Mode = CipherMode.ECB;
            //padding mode(if any extra byte added)
            tdes.Padding = PaddingMode.PKCS7;

            ICryptoTransform cTransform = tdes.CreateDecryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(
                                 toEncryptArray, 0, toEncryptArray.Length);
            //Release resources held by TripleDes Encryptor                
            tdes.Clear();
            //return the Clear decrypted TEXT
            return UTF8Encoding.UTF8.GetString(resultArray);
        }
    }

}