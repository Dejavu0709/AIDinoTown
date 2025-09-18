using RobotGame.Security;
using UnityEngine;

namespace RobotGame.Persistence
{
    /// <summary>
    /// PlayerPrefs implementation that transparently encrypts values
    /// (int, float, string) using <see cref="CryptoUtility"/>.
    /// </summary>
    public sealed class PlayerPrefsSaveProvider : ILocalSaveProvider
    {
        private readonly byte[] key;

        public PlayerPrefsSaveProvider(string salt = "PetCareGame_Salt_v1")
        {
            key = CryptoUtility.DeriveKey(salt);
        }

        #region Int -------------------------------------------------------------

        public void SetInt(string k, int v) =>
            PlayerPrefs.SetString(k, Encrypt(v.ToString()));

        public int GetInt(string k, int def = 0) =>
            TryDecrypt(PlayerPrefs.GetString(k, null), def, int.TryParse);

        #endregion

        #region Float -----------------------------------------------------------

        public void SetFloat(string k, float v) =>
            PlayerPrefs.SetString(k, Encrypt(v.ToString("R"))); // full precision

        public float GetFloat(string k, float def = 0f) =>
            TryDecrypt(PlayerPrefs.GetString(k, null), def, float.TryParse);

        #endregion

        #region String ----------------------------------------------------------

        public void SetString(string k, string v) =>
            PlayerPrefs.SetString(k, Encrypt(v));

        public string GetString(string k, string def = "") =>
            TryDecrypt(PlayerPrefs.GetString(k, null), def, (string s, out string r) =>
            {
                r = s; return true;
            });

        #endregion

        public void ResetAccount() => PlayerPrefs.DeleteAll();
        public void Save() => PlayerPrefs.Save();

        #region Helpers ---------------------------------------------------------

        private string Encrypt(string plain) => CryptoUtility.Encrypt(plain, key);

        private T TryDecrypt<T>(string cipher, T fallback, TryParser<T> parser)
        {
            if (string.IsNullOrEmpty(cipher)) return fallback;
            try
            {
                string plain = CryptoUtility.Decrypt(cipher, key);
                return parser(plain, out T val) ? val : fallback;
            }
            catch { return fallback; }
        }

        private delegate bool TryParser<T>(string s, out T result);

        #endregion
    }
}
