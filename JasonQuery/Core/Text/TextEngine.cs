using JasonQuery.Core.Config;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace JasonQuery.Core.Text
{
    public enum TextEncodes
    {
        ASCII = 0,
        UTF7 = 1,
        UTF8 = 2,
        UTF32 = 3,
        Unicode = 4,
        BigEndianUnicode = 5,
        Default = 6
    }

    public static class TextEngine
    {
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        const string rgbKey = "ytec1688"; //8個字元！
        const string rgbIV = "ytec1688";

        public static string Encode(string data)
        {
            var byRgbKey = Encoding.ASCII.GetBytes(rgbKey);
            var byRgbIV = Encoding.ASCII.GetBytes(rgbIV);
            var cryptoProvider = new DESCryptoServiceProvider();
            var i = cryptoProvider.KeySize;

            try
            {
                var ms = new MemoryStream();
                var cst = new CryptoStream(ms, cryptoProvider.CreateEncryptor(byRgbKey, byRgbIV), CryptoStreamMode.Write);
                var sw = new StreamWriter(cst);

                sw.Write(data);
                sw.Flush();
                cst.FlushFinalBlock();
                sw.Flush();

                return Convert.ToBase64String(ms.GetBuffer(), 0, (int)ms.Length);
            }
            catch (Exception)
            {
                return string.Empty; //解密出錯，KEY 不正確！
            }
        }

        public static string Decode(string data)
        {
            var byRgbKey = Encoding.ASCII.GetBytes(rgbKey);
            var byRgbIV = Encoding.ASCII.GetBytes(rgbIV);

            try
            {
                var byEnc = Convert.FromBase64String(data);
                var cryptoProvider = new DESCryptoServiceProvider();
                var ms = new MemoryStream(byEnc);
                var cst = new CryptoStream(ms, cryptoProvider.CreateDecryptor(byRgbKey, byRgbIV), CryptoStreamMode.Read);
                var sr = new StreamReader(cst);

                return sr.ReadToEnd();
            }
            catch (Exception)
            {
                return string.Empty; //解密出錯，KEY 不正確！
            }
        }

        public static string Encrypt(string toEncrypt, string key)
        {
            var toEncryptArray = Encoding.UTF8.GetBytes(toEncrypt);
            var hashmd5 = new MD5CryptoServiceProvider();
            var keyArray = hashmd5.ComputeHash(Encoding.UTF8.GetBytes(key));

            hashmd5.Clear();

            var tdes = new TripleDESCryptoServiceProvider
            {
                Key = keyArray,
                Mode = CipherMode.ECB,
                Padding = PaddingMode.PKCS7
            };

            var cTransform = tdes.CreateEncryptor();
            var resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

            tdes.Clear();
            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }

        public static string Decrypt(string cipherString, string key)
        {
            var toEncryptArray = Convert.FromBase64String(cipherString);

            try
            {
                var hashmd5 = new MD5CryptoServiceProvider();
                var keyArray = hashmd5.ComputeHash(Encoding.UTF8.GetBytes(key));

                hashmd5.Clear();

                var tdes = new TripleDESCryptoServiceProvider
                {
                    Key = keyArray,
                    Mode = CipherMode.ECB,
                    Padding = PaddingMode.PKCS7
                };

                var cTransform = tdes.CreateDecryptor();
                var resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

                tdes.Clear();
                return Encoding.UTF8.GetString(resultArray);
            }
            catch (Exception)
            {
                return string.Empty; //解密出錯，KEY 不正確！
            }
        }

        public static int WriteBinaryFile(string SaveFileName, byte[] InData)
        {
            try
            {
                //開啟建立檔案
                var myFile = File.Open(SaveFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                var myWriter = new BinaryWriter(myFile);

                myWriter.Write(InData);
                myWriter.Close();
                myWriter.Dispose();
                myFile.Close();
                myFile.Dispose();
                return 1;
            }
            catch (InvalidCastException e)
            {
                Console.WriteLine(e);
                return -1;
            }
        }

        public static int BinRead(string OpenFileName, ref byte[] InData)
        {
            try
            {
                var myFile = File.Open(OpenFileName, FileMode.Open, FileAccess.ReadWrite);
                var myReader = new BinaryReader(myFile);
                var dl = Convert.ToInt32(myFile.Length);

                InData = myReader.ReadBytes(dl);
                myReader.Close();
                myReader.Dispose();
                myFile.Close();
                myFile.Dispose();
                return 1;
            }
            catch (InvalidCastException e)
            {
                Console.WriteLine(e);
                return -1;
            }
        }

        public static string GetFileEncoding(string srcFile)
        {
            var enc = "ANSI";
            var buffer = new byte[5];
            var file = new FileStream(srcFile, FileMode.Open);

            file.Read(buffer, 0, 5);
            file.Close();

            switch (buffer[0])
            {
                case 0xef when buffer[1] == 0xbb && buffer[2] == 0xbf:
                    {
                        enc = "UTF8";
                        break;
                    }
                case 0xfe when buffer[1] == 0xff:
                    {
                        enc = "Unicode";
                        break;
                    }
                case 0 when buffer[1] == 0 && buffer[2] == 0xfe && buffer[3] == 0xff:
                    {
                        enc = "UTF32";
                        break;
                    }
                case 0x2b when buffer[1] == 0x2f && buffer[2] == 0x76:
                    {
                        enc = "UTF7";
                        break;
                    }
                case 0xff when buffer[1] == 0xfe:
                    {
                        enc = "utf16LeBom";
                        break;
                    }
            }

            return enc;
        }

        #region 將內容寫入到指定的檔案
        public static void WriteContentToFile(string fileContent, string fileName, TextEncodes encodeMothed, FileMode fm = FileMode.OpenOrCreate, FileAccess fa = FileAccess.Write)
        {
            var fs = new FileStream(fileName, fm, fa);
            Encoding ee;

            switch ((int)encodeMothed)
            {
                case 0:
                    {
                        ee = Encoding.ASCII;
                        break;
                    }
                case 1:
                    {
                        ee = Encoding.UTF7;
                        break;
                    }
                case 2:
                    {
                        ee = Encoding.UTF8;
                        break;
                    }
                case 3:
                    {
                        ee = Encoding.UTF32;
                        break;
                    }
                case 4:
                    {
                        ee = Encoding.Unicode;
                        break;
                    }
                case 5:
                    {
                        ee = Encoding.BigEndianUnicode;
                        break;
                    }
                default:
                    {
                        ee = Encoding.Default;
                        break;
                    }
            }

            var sw = new StreamWriter(fs, ee);

            try
            {
                sw.Write(fileContent);
            }
            catch (Exception ex)
            {
                var message = $"{MyGlobal.AnUnexpectedErrorHasOccurred}\r\n\r\n{ex.Message}\r\n\r\n{MyGlobal.PleaseTryAgain}";

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

            sw.Close();
        }
        #endregion

        #region 將內容寫入到指定的檔案
        public static void WriteContentToFile(string fileContent, string fileName, string encode, FileMode fm = FileMode.OpenOrCreate, FileAccess fa = FileAccess.Write)
        {
            var fs = new FileStream(fileName, fm, fa);
            Encoding ee;

            switch (encode)
            {
                case "ASCII":
                    {
                        ee = Encoding.ASCII;
                        break;
                    }
                case "UTF-7":
                    {
                        ee = Encoding.UTF7;
                        break;
                    }
                case "UTF-8":
                    {
                        ee = Encoding.UTF8;
                        break;
                    }
                case "UTF-32":
                    {
                        ee = Encoding.UTF32;
                        break;
                    }
                case "Unicode":
                    {
                        ee = Encoding.Unicode;
                        break;
                    }
                case "Unicode big endian":
                    {
                        ee = Encoding.BigEndianUnicode;
                        break;
                    }
                default:
                    {
                        ee = Encoding.Default;
                        break;
                    }
            }

            var sw = new StreamWriter(fs, ee);

            try
            {
                sw.Write(fileContent);
            }
            catch (Exception ex)
            {
                var message = $"{MyGlobal.AnUnexpectedErrorHasOccurred}\r\n\r\n{ex.Message}\r\n\r\n{MyGlobal.PleaseTryAgain}";

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

            sw.Close();
        }
        #endregion
    }
}
